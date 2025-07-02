using System.Buffers;
using System.Text;
using System.Web;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    public class HttpTrackerParser : LinkParser
    {
        private readonly StringBuilder _stringBuilder;

        public HttpTrackerParser(SequencePool<byte> sequencePool) : base(sequencePool)
        {
            _stringBuilder = new();
        }

        public override void Serialize<T>(ref ExtenderBinaryWriter writer, T value)
        {
            if (value is TrackerRequest request)
            {
                RequsestToBytes(ref writer, request);
            }
        }

        protected override void Receive(ref ExtenderBinaryReader reader)
        {
            throw new NotImplementedException();
        }

        private void RequsestToBytes(ref ExtenderBinaryWriter writer, TrackerRequest request)
        {
            _stringBuilder.Clear();

            ToString("info_hash", request.Hash.GetSha1orSha256().ToHexString());
            ToString("peer_id", request.Id.ToString());
            ToString("port", request.Port.ToString());
            ToString("uploaded", request.Uploaded.ToString());
            ToString("downloaded", request.Downloaded.ToString());
            ToString("left", request.Left.ToString());
            ToString("compact", request.Compact);
            ToString("no_peer_id", request.NoPeerId);
            ToString("event", request.Event.ToString().ToLower());
            var resultSting = _stringBuilder.ToString();

            var length = Encoding.ASCII.GetByteCount(resultSting);
            var array = ArrayPool<byte>.Shared.Rent(length);
            Encoding.ASCII.GetBytes(resultSting, 0, resultSting.Length, array, 0);

            writer.Write(array.AsSpan(0, length));
            writer.Advance(length);
            ArrayPool<byte>.Shared.Return(array);

            void ToString(string name, string value)
            {
                _stringBuilder.Append(name);
                _stringBuilder.Append('=');
                _stringBuilder.Append(HttpUtility.UrlEncode(value));
            }
        }

        ///// <summary>
        ///// 解析 Tracker 返回的 Bencode 格式响应
        ///// </summary>
        //private TrackerResponse ParseTrackerResponse(Stream stream)
        //{
        //    // 实际实现需要完整的 Bencode 解析器
        //    // 简化版示例，仅演示结构
        //    using var reader = new StreamReader(stream);
        //    string content = reader.ReadToEnd();

        //    // 注意：实际应用中需要使用完整的 Bencode 解析器
        //    // 这里仅为示例
        //    var response = new TrackerResponse
        //    {
        //        Interval = 1800, // 默认 30 分钟
        //        Peers = new List<PeerInfo>()
        //    };

        //    // 提取错误消息（如果有）
        //    var errorMatch = Regex.Match(content, "\"failure reason\"\\s*:\\s*\"([^\"]+)\"");
        //    if (errorMatch.Success)
        //    {
        //        response.Success = false;
        //        response.ErrorMessage = errorMatch.Groups[1].Value;
        //        return response;
        //    }

        //    // 提取 interval
        //    var intervalMatch = Regex.Match(content, "\"interval\"\\s*:\\s*(\\d+)");
        //    if (intervalMatch.Success)
        //    {
        //        response.Interval = int.Parse(intervalMatch.Groups[1].Value);
        //    }

        //    // 提取 peers（简化版，实际需要解析 Bencode 格式）
        //    var peersMatch = Regex.Match(content, "\"peers\"\\s*:\\s*\"([^\"]+)\"");
        //    if (peersMatch.Success)
        //    {
        //        string peersData = peersMatch.Groups[1].Value;
        //        // 解析 peers 数据（二进制格式或字典格式）
        //        response.Peers = ParsePeersData(peersData);
        //    }

        //    response.Success = true;
        //    return response;
        //}

        ///// <summary>
        ///// 解析 peers 数据
        ///// </summary>
        //private List<PeerInfo> ParsePeersData(string peersData)
        //{
        //    var peers = new List<PeerInfo>();

        //    // 检查是否为二进制格式（每 6 字节：4 字节 IP + 2 字节端口）
        //    if (peersData.Length % 6 == 0)
        //    {
        //        byte[] peerBytes = Convert.FromBase64String(peersData);
        //        for (int i = 0; i < peerBytes.Length; i += 6)
        //        {
        //            string ip = $"{peerBytes[i]}.{peerBytes[i + 1]}.{peerBytes[i + 2]}.{peerBytes[i + 3]}";
        //            int port = (peerBytes[i + 4] << 8) | peerBytes[i + 5];

        //            peers.Add(new PeerInfo
        //            {
        //                IpAddress = ip,
        //                Port = port
        //            });
        //        }
        //    }
        //    else
        //    {
        //        // 字典格式（未实现，需要完整 Bencode 解析）
        //    }

        //    return peers;
        //}

        ///// <summary>
        ///// 对 InfoHash 进行 URL 编码
        ///// </summary>
        //private string UrlEncodeInfoHash(string infoHash)
        //{
        //    // 将十六进制字符串转换为字节数组
        //    byte[] bytes = new byte[infoHash.Length / 2];
        //    for (int i = 0; i < infoHash.Length; i += 2)
        //    {
        //        bytes[i / 2] = Convert.ToByte(infoHash.Substring(i, 2), 16);
        //    }

        //    // 使用原始字节进行 URL 编码
        //    return HttpUtility.UrlEncode(bytes, Encoding.UTF8);
        //}
    }
}
