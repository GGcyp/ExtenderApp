using System.IO;
using System.Security.Cryptography;
using System.Text;
using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using static ExtenderApp.Torrent.TorrentFile;

namespace ExtenderApp.Torrent
{
    internal class TorrentFileForamtter
    {
        private readonly IHashProvider _hashProvider;

        public TorrentFileForamtter(IHashProvider hashProvider)
        {
            _hashProvider = hashProvider;
        }

        #region Decode

        private struct TorrentBuffer
        {
            public int Position { get; set; }

            public TorrentBuffer()
            {
                Position = 0;
            }
        }

        public TorrentFile Decode(IFileOperate operate)
        {
            byte[] data = operate.Read();
            return Decode(data);
        }

        public TorrentFile Decode(byte[] data)
        {
            TorrentBuffer buffer = new TorrentBuffer();
            var dict = DecodeValue(data, ref buffer) as Dictionary<string, object>;
            if (dict == null)
                throw new InvalidDataException("无效种子文件格式");

            var torrentFile = new TorrentFile();


            if (dict.TryGetValue("announce", out var announceObj))
                torrentFile.AnnounceList.Add(DecodeString(announceObj));

            if (dict.TryGetValue("comment", out var commentObj))
                torrentFile.Comment = DecodeString(commentObj);

            if (dict.TryGetValue("created by", out var createdByObj))
                torrentFile.CreatedBy = DecodeString(createdByObj);

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
                                torrentFile.AnnounceList.Add(DecodeString(url));
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
                torrentFile.PieceLength = Convert.ToInt64(pieceLengthObj);

            if (infoDict.TryGetValue("pieces", out var piecesObj))
            {
                var dataBuffer = piecesObj as DataBuffer<int, int>;
                if (dataBuffer != null)
                {
                    byte[] pieces = new byte[dataBuffer.Item2];
                    Array.Copy(data, 0, pieces, dataBuffer.Item1, dataBuffer.Item2);
                    torrentFile.Pieces = pieces;
                }
            }

            var isSingleFile = infoDict.TryGetValue("length", out var lengthObj);
            var node = torrentFile.FileInfoNode;
            node.Name = torrentFile.Name;
            if (isSingleFile)
            {
                node.Length = Convert.ToInt64(lengthObj);
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
                            var pathName = DecodeString(pathList[j]);
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
                        childNode.Length = Convert.ToInt64(lengthObj);
                }
            }

            return torrentFile;
        }

        private string DecodeString(object obj)
        {
            if (obj == null || obj is not DataBuffer<int, int, byte[]> dataBuffer)
                return string.Empty;

            string result = Encoding.UTF8.GetString(dataBuffer.Item3, dataBuffer.Item1, dataBuffer.Item2);
            dataBuffer.Release();
            return result;
        }

        private byte[] EncodeInfoDictionary(Dictionary<string, object> infoDict)
        {
            return Encode(infoDict);
        }

        private object DecodeValue(byte[] data, ref TorrentBuffer buffer)
        {
            if (buffer.Position >= data.Length)
                return null;

            char type = (char)data[buffer.Position];

            switch (type)
            {
                case 'd': // 字典
                    return DecodeDictionary(data, ref buffer);
                case 'l': // 列表
                    return DecodeList(data, ref buffer);
                case 'i': // 整数
                    return DecodeInteger(data, ref buffer);
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
                    return DecodeDataBuffer(data, ref buffer);
                default:
                    throw new InvalidDataException($"Invalid Bencode type: {type}");
            }
        }

        private DataBuffer<int, int, byte[]> DecodeDataBuffer(byte[] data, ref TorrentBuffer buffer)
        {
            int colonPos = Array.IndexOf(data, (byte)':', buffer.Position);
            if (colonPos == -1)
                throw new InvalidDataException("Missing colon in string");

            int length = int.Parse(Encoding.ASCII.GetString(data, buffer.Position, colonPos - buffer.Position));

            var dataBuffer = DataBuffer<int, int, byte[]>.GetDataBuffer();

            int position = colonPos + 1;
            dataBuffer.Item1 = position;
            dataBuffer.Item2 = length;
            dataBuffer.Item3 = data;
            buffer.Position = position + length;
            return dataBuffer;
        }

        private long DecodeInteger(byte[] data, ref TorrentBuffer buffer)
        {
            buffer.Position++; // 跳过 'i'
            int endPos = Array.IndexOf(data, (byte)'e', buffer.Position);
            if (endPos == -1)
                throw new InvalidDataException("Missing 'e' in integer");

            long result = long.Parse(Encoding.ASCII.GetString(data, buffer.Position, endPos - buffer.Position));
            buffer.Position = endPos + 1;
            return result;
        }

        private List<object> DecodeList(byte[] data, ref TorrentBuffer buffer)
        {
            buffer.Position++; // 跳过 'l'
            var list = new List<object>();

            while ((char)data[buffer.Position] != 'e')
            {
                list.Add(DecodeValue(data, ref buffer));
            }

            buffer.Position++; // 跳过 'e'
            return list;
        }

        private Dictionary<string, object> DecodeDictionary(byte[] data, ref TorrentBuffer buffer)
        {
            buffer.Position++; // 跳过 'd'
            var dict = new Dictionary<string, object>();

            while ((char)data[buffer.Position] != 'e')
            {
                var dataBuffer = DecodeDataBuffer(data, ref buffer);
                string key = DecodeString(dataBuffer);
                object value = DecodeValue(data, ref buffer);
                dict[key] = value;
            }

            buffer.Position++; // 跳过 'e'
            return dict;
        }

        #endregion

        #region Encode

        /// <summary>
        /// 将对象（字典/列表/字符串/整数）Bencode编码为字节数组
        /// </summary>
        public byte[] Encode(object value)
        {
            using var ms = new MemoryStream();
            EncodeValue(ms, value);
            return ms.ToArray();
        }

        private void EncodeValue(Stream stream, object value)
        {
            switch (value)
            {
                case string s:
                    EncodeString(stream, s);
                    break;
                case byte[] bytes:
                    EncodeBytes(stream, bytes);
                    break;
                case long l:
                    EncodeInteger(stream, l);
                    break;
                case int i:
                    EncodeInteger(stream, i);
                    break;
                case IList<object> list:
                    EncodeList(stream, list);
                    break;
                case IDictionary<string, object> dict:
                    EncodeDictionary(stream, dict);
                    break;
                case DataBuffer<int, int, byte[]> dataB:
                    EncodeDataBuffer(stream, dataB);
                    break;
                default:
                    throw new NotSupportedException($"不支持的Bencode类型: {value?.GetType()}");
            }
        }

        private void EncodeString(Stream stream, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            EncodeBytes(stream, bytes);
        }

        private void EncodeBytes(Stream stream, byte[] bytes)
        {
            var lenBytes = Encoding.ASCII.GetBytes(bytes.Length.ToString());
            stream.Write(lenBytes, 0, lenBytes.Length);
            stream.WriteByte((byte)':');
            stream.Write(bytes, 0, bytes.Length);
        }

        private void EncodeInteger(Stream stream, long value)
        {
            stream.WriteByte((byte)'i');
            var bytes = Encoding.ASCII.GetBytes(value.ToString());
            stream.Write(bytes, 0, bytes.Length);
            stream.WriteByte((byte)'e');
        }

        private void EncodeList(Stream stream, IList<object> list)
        {
            stream.WriteByte((byte)'l');
            foreach (var item in list)
            {
                EncodeValue(stream, item);
            }
            stream.WriteByte((byte)'e');
        }

        private void EncodeDictionary(Stream stream, IDictionary<string, object> dict)
        {
            //stream.WriteByte((byte)'d');
            //foreach (var kv in dict)
            //{
            //    EncodeString(stream, kv.Key);
            //    EncodeValue(stream, kv.Value);
            //}
            //stream.WriteByte((byte)'e');

            stream.WriteByte((byte)'d');
            foreach (var key in dict.Keys.OrderBy(k => k, StringComparer.Ordinal))
            {
                EncodeString(stream, key);
                EncodeValue(stream, dict[key]);
            }
            stream.WriteByte((byte)'e');
        }

        private void EncodeDataBuffer(Stream stream, DataBuffer<int, int, byte[]> dataBuffer)
        {
            var start = dataBuffer.Item1;
            var length = dataBuffer.Item2;
            var bytes = dataBuffer.Item3;

            var lenBytes = Encoding.ASCII.GetBytes(length.ToString());
            stream.Write(lenBytes, 0, lenBytes.Length);
            stream.WriteByte((byte)':');
            stream.Write(bytes, start, length);
        }

        #endregion
    }
}
