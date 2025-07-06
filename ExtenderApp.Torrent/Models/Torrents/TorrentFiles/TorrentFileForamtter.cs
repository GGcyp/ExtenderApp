using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Caches;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    public class TorrentFileForamtter
    {
        private readonly IHashProvider _hashProvider;
        private readonly SequencePool<byte> _sequencePool;
        private readonly StringCache _stringCache;

        public TorrentFileForamtter(IHashProvider hashProvider, SequencePool<byte> sequencePool, StringCache stringCache)
        {
            _hashProvider = hashProvider;
            _sequencePool = sequencePool;
            _stringCache = stringCache;
        }

        #region Decode

        public TorrentFile Decode(Memory<byte> data)
        {
            int position = 0;
            var dict = DecodeValue(data, ref position) as Dictionary<string, object>;
            if (dict == null)
                throw new InvalidDataException("无效种子文件格式");

            var torrentFile = new TorrentFile();


            if (dict.TryGetValue("announce", out var announceObj))
                torrentFile.AnnounceList.Add(DecodeStringForCache(announceObj));

            if (dict.TryGetValue("comment", out var commentObj))
                torrentFile.Comment = DecodeString(commentObj);

            if (dict.TryGetValue("created by", out var createdByObj))
                torrentFile.CreatedBy = DecodeStringForCache(createdByObj);

            //if (dict.TryGetValue("creation date", out var creationDateObj))
            //{
            //    var creationDateString = DecodeString(creationDateObj);
            //    torrentFile.CreationDate = DateTime.Parse(creationDateString);
            //}

            if (dict.TryGetValue("announce-list", out var announceListObj))
            {
                var tierList = announceListObj as List<object>;
                if (tierList != null)
                {
                    foreach (var tier in tierList)
                    {
                        var urls = tier as List<object>;
                        if (urls != null)
                        {
                            foreach (var url in urls)
                            {
                                torrentFile.AnnounceList.Add(DecodeStringForCache(url));
                            }
                        }
                    }
                }
            }

            // 解析info字典
            if (!dict.TryGetValue("info", out var infoObj))
                throw new InvalidDataException("种子文件中缺少“info”字典");

            var infoDict = infoObj as Dictionary<string, object>;
            if (infoDict == null)
                throw new InvalidDataException("“info”字典的格式无效");

            var infoBytes = EncodeInfoDictionary(infoDict);
            var sha1 = _hashProvider.ComputeHash<SHA1>(infoBytes);
            var sha256 = _hashProvider.ComputeHash<SHA256>(infoBytes);
            torrentFile.Hash = new InfoHash(sha1, sha256);

            // 解析info字典内容
            if (infoDict.TryGetValue("name", out var nameObj))
                torrentFile.Name = DecodeString(nameObj);

            if (infoDict.TryGetValue("piece length", out var pieceLengthObj))
                torrentFile.PieceLength = DecodeLong(pieceLengthObj);

            if (infoDict.TryGetValue("pieces", out var piecesObj))
            {
                var dataBuffer = piecesObj as DataBuffer<int, int, byte[]>;
                if (dataBuffer != null)
                {
                    byte[] pieces = dataBuffer.Item3.AsSpan(dataBuffer.Item1, dataBuffer.Item2).ToArray();
                    torrentFile.Pieces = pieces;
                }
            }

            var isSingleFile = infoDict.TryGetValue("length", out var lengthObj);
            var node = torrentFile.FileInfoNode;
            node.Name = torrentFile.Name;
            if (isSingleFile)
            {
                node.Length = DecodeLong(lengthObj);
                node.IsFile = true;
            }
            else
            {
                if (!infoDict.TryGetValue("files", out var filesObj))
                {
                    return torrentFile;
                }

                var filesList = filesObj as List<object>;
                if (filesList == null)
                {
                    return torrentFile;
                }

                for (int i = 0; i < filesList.Count; i++)
                {
                    var fileObj = filesList[i];
                    var fileDict = fileObj as Dictionary<string, object>;
                    if (fileDict == null) continue;

                    TorrentFileInfoNode pathNode = node;
                    TorrentFileInfoNode childNode = node;
                    if (fileDict.TryGetValue("path", out var pathObj))
                    {
                        var pathList = pathObj as List<object>;
                        if (pathList == null) continue;

                        for (int j = 0; j < pathList.Count - 1; j++)
                        {
                            var pathName = DecodeStringForCache(pathList[j]);
                            if (pathNode.Find(n => n.Name == pathName, out childNode))
                            {
                                if (childNode.IsFile)
                                    throw new InvalidDataException($"当前节点为文件节点，但是还包含文件{pathName}");

                                pathNode = childNode;
                                continue;
                            }
                            else
                            {
                                childNode = TorrentFileInfoNode.Get();
                                childNode.Name = pathName;
                                childNode.IsFile = false;
                                pathNode.Add(childNode);
                                pathNode = childNode;
                            }
                        }

                        childNode = TorrentFileInfoNode.Get();
                        childNode.Name = DecodeString(pathList[^1]);
                        childNode.IsFile = true;
                        pathNode.Add(childNode);
                    }

                    if (childNode == node)
                        throw new InvalidDataException("路径节点未正确创建，childNode 仍为根节点，可能路径解析逻辑有误。请检查种子文件的 path 字段或解析实现。");

                    if (fileDict.TryGetValue("length", out lengthObj))
                        childNode.Length = DecodeLong(lengthObj);
                }
            }

            return torrentFile;
        }

        private long DecodeLong(object obj)
        {
            if (obj == null || obj is not DataBuffer<int, int, (byte[], byte)> dataBuffer)
                return 0;

            string result = DecodeString(dataBuffer.Item1, dataBuffer.Item2, dataBuffer.Item3.Item1);
            dataBuffer.Release();
            return long.Parse(result);
        }

        private string DecodeString(object obj)
        {
            return DecodeString(obj as DataBuffer<int, int, Memory<byte>, byte>);
        }

        private string DecodeString(DataBuffer<int, int, Memory<byte>, byte>? dataBuffer)
        {
            if (dataBuffer == null)
                return string.Empty;

            string result = DecodeString(dataBuffer.Item1, dataBuffer.Item2, dataBuffer.Item3);
            dataBuffer.Release();
            return result;
        }

        private string DecodeStringForCache(object obj)
        {
            return DecodeStringForCache(obj as DataBuffer<int, int, Memory<byte>, byte>);
        }

        private string DecodeStringForCache(DataBuffer<int, int, Memory<byte>, byte>? dataBuffer)
        {
            if (dataBuffer == null)
                return string.Empty;

            //string result = DecodeString(dataBuffer.Item1, dataBuffer.Item2, dataBuffer.Item3);
            string result = _stringCache.GetString(dataBuffer.Item3.Span.Slice(dataBuffer.Item1, dataBuffer.Item2), Encoding.UTF8);
            dataBuffer.Release();
            return result;
        }

        private string DecodeString(int start, int length, Memory<byte> data)
        {
            return Encoding.UTF8.GetString(data.Span.Slice(start, length));
        }

        private byte[] EncodeInfoDictionary(Dictionary<string, object> infoDict)
        {
            return Encode(infoDict);
        }

        private object DecodeValue(Memory<byte> data, ref int position)
        {
            if (position >= data.Length)
                return null;

            char type = (char)data.Span[position];

            switch (type)
            {
                case 'd': // 字典
                    return DecodeDictionary(data, ref position);
                case 'l': // 列表
                    return DecodeList(data, ref position);
                case 'i': // 整数
                    return DecodeInteger(data, ref position);
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9': // 字符串
                    return DecodeDataBuffer(data, ref position);
                default:
                    throw new InvalidDataException($"Invalid Bencode type: {type}");
            }
        }

        private DataBuffer<int, int, Memory<byte>, byte> DecodeDataBuffer(Memory<byte> data, ref int position)
        {
            //int colonPos = Array.IndexOf(data, (byte)':', position);
            int colonLength = data.Span.Slice(position).IndexOf((byte)':');
            if (colonLength == -1)
                throw new InvalidDataException("Missing colon in string");

            int length = int.Parse(Encoding.ASCII.GetString(data.Span.Slice(position, colonLength)));

            var dataBuffer = DataBuffer<int, int, Memory<byte>, byte>.GetDataBuffer();
            dataBuffer.Item1 = position + colonLength + 1;
            dataBuffer.Item2 = length;
            dataBuffer.Item3 = data;
            position += colonLength + 1 + length;
            return dataBuffer;
        }

        private DataBuffer<int, int, Memory<byte>, byte> DecodeInteger(Memory<byte> data, ref int position)
        {
            position++; // 跳过 'i'
            //int endPos = Array.IndexOf(data, (byte)'e', position);
            int length = data.Span.Slice(position).IndexOf((byte)'e');
            if (length == -1)
                throw new InvalidDataException("Missing 'e' in integer");

            //long result = long.Parse(Encoding.ASCII.GetString(data, position, endPos - position));

            var dataBuffer = DataBuffer<int, int, Memory<byte>, byte>.GetDataBuffer();
            dataBuffer.Item1 = position;
            dataBuffer.Item2 = length;
            dataBuffer.Item3 = data;
            dataBuffer.Item4 = (byte)'i';
            position += length + 1;
            return dataBuffer;
        }

        private List<object> DecodeList(Memory<byte> data, ref int position)
        {
            position++; // 跳过 'l'
            var list = new List<object>();

            while ((char)data.Span[position] != 'e')
            {
                list.Add(DecodeValue(data, ref position));
            }

            position++; // 跳过 'e'
            return list;
        }

        private Dictionary<string, object> DecodeDictionary(Memory<byte> data, ref int position)
        {
            position++; // 跳过 'd'
            var dict = new Dictionary<string, object>();

            while ((char)data.Span[position] != 'e')
            {
                var dataBuffer = DecodeDataBuffer(data, ref position);
                string key = DecodeStringForCache(dataBuffer);
                object value = DecodeValue(data, ref position);
                dict[key] = value;
            }

            position++; // 跳过 'e'
            return dict;
        }

        #endregion

        #region Encode

        /// <summary>
        /// 将对象（字典/列表/字符串/整数）Bencode编码为字节数组
        /// </summary>
        public byte[] Encode(object value)
        {
            var writer = new ExtenderBinaryWriter(_sequencePool.Rent());
            EncodeValue(ref writer, value);
            return writer.FlushAndGetArray();
        }

        private void EncodeValue(ref ExtenderBinaryWriter writer, object value)
        {
            switch (value)
            {
                case string s:
                    EncodeString(ref writer, s);
                    break;
                case byte[] bytes:
                    EncodeBytes(ref writer, bytes);
                    break;
                case long l:
                    EncodeInteger(ref writer, l);
                    break;
                case int i:
                    EncodeInteger(ref writer, i);
                    break;
                case IList<object> list:
                    EncodeList(ref writer, list);
                    break;
                case IDictionary<string, object> dict:
                    EncodeDictionary(ref writer, dict);
                    break;
                case DataBuffer<int, int, Memory<byte>, byte> dataB:
                    EncodeDataBuffer(ref writer, dataB);
                    break;
                default:
                    throw new NotSupportedException($"不支持的Bencode类型: {value?.GetType()}");
            }
        }

        private void EncodeString(ref ExtenderBinaryWriter writer, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            EncodeBytes(ref writer, bytes);
        }

        private void EncodeBytes(ref ExtenderBinaryWriter writer, byte[] bytes)
        {
            var lenBytes = Encoding.ASCII.GetBytes(bytes.Length.ToString());
            writer.Write(lenBytes.AsSpan(0, lenBytes.Length));
            writer.Write((byte)':');
            writer.Write(bytes.AsSpan());
        }

        private void EncodeInteger(ref ExtenderBinaryWriter writer, long value)
        {
            writer.Write((byte)'i');
            var bytes = Encoding.ASCII.GetBytes(value.ToString());
            writer.Write(bytes.AsSpan());
            writer.Write((byte)'e');
        }

        private void EncodeList(ref ExtenderBinaryWriter writer, IList<object> list)
        {
            writer.Write((byte)'l');
            foreach (var item in list)
            {
                EncodeValue(ref writer, item);
            }
            writer.Write((byte)'e');
        }

        private void EncodeDictionary(ref ExtenderBinaryWriter writer, IDictionary<string, object> dict)
        {
            //stream.WriteByte((byte)'d');
            //foreach (var kv in dict)
            //{
            //    EncodeString(stream, kv.Key);
            //    EncodeValue(stream, kv.Value);
            //}
            //stream.WriteByte((byte)'e');

            writer.Write((byte)'d');
            foreach (var key in dict.Keys.OrderBy(k => k, StringComparer.Ordinal))
            {
                EncodeString(ref writer, key);
                EncodeValue(ref writer, dict[key]);
            }
            writer.Write((byte)'e');
        }

        private void EncodeDataBuffer(ref ExtenderBinaryWriter writer, DataBuffer<int, int, Memory<byte>, byte> dataBuffer)
        {
            var start = dataBuffer.Item1;
            var length = dataBuffer.Item2;
            var bytes = dataBuffer.Item3;
            var type = dataBuffer.Item4;

            switch (type)
            {
                case (byte)'i':
                    writer.Write(type);
                    writer.Write(bytes.Span.Slice(start, length));
                    writer.Write((byte)'e');
                    break;
                default:
                    var lengthString = length.ToString();
                    int len = Encoding.ASCII.GetByteCount(lengthString);
                    byte[] buffer = ArrayPool<byte>.Shared.Rent(len);

                    // 推荐用这个重载，避免内容不对
                    int written = Encoding.ASCII.GetBytes(lengthString, 0, lengthString.Length, buffer, 0);
                    writer.Write(buffer.AsSpan(0, written));
                    writer.Write((byte)':');
                    writer.Write(bytes.Span.Slice(start, length));
                    ArrayPool<byte>.Shared.Return(buffer);
                    break;
            }
        }

        #endregion
    }
}
