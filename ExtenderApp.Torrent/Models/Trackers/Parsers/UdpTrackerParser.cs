using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Text;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    public class UdpTrackerParser : LinkParser
    {
        // UDP Tracker 协议常量
        /// <summary>
        /// 固定初始连接ID
        /// </summary>
        private const long ConnectionId = 0x41727101980L;

        /// <summary>
        /// 连接动作
        /// </summary>
        private const int ActionConnect = 0;

        /// <summary>
        /// 宣告动作
        /// </summary>
        private const int ActionAnnounce = 1;

        /// <summary>
        /// 抓取动作
        /// </summary>
        private const int ActionScrape = 2;

        /// <summary>
        /// 错误动作
        /// </summary>
        private const int ActionError = 3;

        private int transactionId;

        private long connectionId;

        public event Action<long>? OnReceiveConnectionId;
        public event Action<TrackerResponse>? OnReceiveTrackerResponse;

        public UdpTrackerParser(SequencePool<byte> sequencePool) : base(sequencePool)
        {
        }

        public override void Serialize<T>(ref ExtenderBinaryWriter writer, T value)
        {
            switch (value)
            {
                case int transactionId:
                    this.transactionId = transactionId;
                    WriteConnectTransactionId(ref writer, transactionId);
                    break;
                case TrackerRequest request:
                    this.transactionId = request.TransactionId;
                    WriteTrackerRequest(ref writer, request);
                    break;
            }
        }

        protected override void Receive(ref ExtenderBinaryReader reader)
        {
            if (reader.Remaining < 16)
                throw new InvalidDataException($"响应长度无效：{reader.Remaining}");


            // 验证action和transactionId
            var action = BinaryPrimitives.ReadInt32BigEndian(reader.UnreadSpan.Slice(0, 4));
            var transactionId = BinaryPrimitives.ReadInt32BigEndian(reader.UnreadSpan.Slice(4, 4));
            reader.Advance(8);

            if (action == ActionError)
            {
                var errorMessage = Encoding.ASCII.GetString(reader.UnreadSpan);
                throw new InvalidOperationException($"Tracker error: {errorMessage}");
            }

            if (transactionId != this.transactionId)
            {
                // 忽略错误的transactionId，继续等待正确的响应
                return;
            }

            switch (action)
            {
                case ActionConnect:
                    var lastConnectionId = connectionId;
                    connectionId = BinaryPrimitives.ReadInt64BigEndian(reader.UnreadSpan);
                    if (lastConnectionId != connectionId)
                    {
                        OnReceiveConnectionId?.Invoke(connectionId);
                    }
                    return;
                case ActionAnnounce:
                    ReadTrackerResponse(reader);
                    return;
            }
        }

        private void WriteConnectTransactionId(ref ExtenderBinaryWriter writer, int transactionId)
        {
            var span = writer.GetSpan(16);
            BinaryPrimitives.WriteInt64BigEndian(span.Slice(0, 8), ConnectionId);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(8, 4), ActionConnect);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(12, 4), transactionId);
            writer.Advance(16);
        }

        private void WriteTrackerRequest(ref ExtenderBinaryWriter writer, TrackerRequest request)
        {
            var span = writer.GetSpan(16);
            BinaryPrimitives.WriteInt64BigEndian(span.Slice(0, 8), connectionId);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(8, 4), ActionAnnounce);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(12, 4), transactionId);
            writer.Advance(16);

            // 复制infoHash
            request.Hash.CopyTo(ref writer);

            // 复制peerId
            request.Id.CopyTo(ref writer);

            // 上传/下载/剩余字节数
            span = writer.GetSpan(24);
            BinaryPrimitives.WriteInt64BigEndian(span.Slice(0, 8), request.Downloaded);
            BinaryPrimitives.WriteInt64BigEndian(span.Slice(8, 8), request.Uploaded);
            BinaryPrimitives.WriteInt64BigEndian(span.Slice(16, 8), request.Left);
            writer.Advance(24);

            // 事件
            span = writer.GetSpan(4);
            BinaryPrimitives.WriteInt32BigEndian(span, request.Event);
            writer.Advance(4);

            // IP地址（0表示使用发送端IP）
            span = writer.GetSpan(4);
            BinaryPrimitives.WriteInt32BigEndian(span, 0);
            writer.Advance(4);

            // 随机数
            span = writer.GetSpan(4);
            BinaryPrimitives.WriteInt32BigEndian(span, request.TransactionId);
            writer.Advance(4);

            // 下载器数（-1表示不关心）
            span = writer.GetSpan(4);
            BinaryPrimitives.WriteInt32BigEndian(span, -1);
            writer.Advance(4);

            // 端口
            span = writer.GetSpan(2);
            BinaryPrimitives.WriteUInt16BigEndian(span, request.Port);
            writer.Advance(2);
        }

        private void ReadTrackerResponse(ExtenderBinaryReader reader)
        {
            var interval = BinaryPrimitives.ReadInt32BigEndian(reader.UnreadSpan);
            var leechers = BinaryPrimitives.ReadInt32BigEndian(reader.UnreadSpan.Slice(4, 4));
            var seeders = BinaryPrimitives.ReadInt32BigEndian(reader.UnreadSpan.Slice(8, 4));
            reader.Advance(12);

            // 每个Peer占6字节 (4字节IP + 2字节端口)
            if (reader.Remaining % 6 != 0)
                throw new InvalidDataException($"对等节点信息格式错误");

            ReadOnlySpan<byte> span = reader.UnreadSpan;
            for (int i = 0; i < reader.Remaining; i += 6)
            {
                var ipBytes = span.Slice(i, 4).ToArray();
                var ipAddress = new IPAddress(ipBytes);

                var port = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(i + 4, 2));
                PeerInfo peerInfo = new PeerInfo();
            }

            var result = new TrackerResponse
            {
                Interval = interval,
                Complete = seeders,
                Incomplete = leechers,
            };
            OnReceiveTrackerResponse?.Invoke(result);
        }
    }
}
