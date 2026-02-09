using System.Net;
using System.Net.Sockets;

namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 表示一次套接字操作（发送/接收/接收自/带报文信息接收等）的成功结果值。
    /// </summary>
    public readonly struct SocketOperationValue
    {
        /// <summary>
        /// 表示一个空的、无任何操作结果的 <see cref="SocketOperationValue"/> 实例。
        /// </summary>
        public static readonly SocketOperationValue Empty = new SocketOperationValue(0, null, default);

        /// <summary>
        /// 实际传输的字节数。
        /// 对于接收操作，值为 0 通常表示对端已优雅关闭连接。
        /// </summary>
        public int BytesTransferred { get; }

        /// <summary>
        /// 远端终结点信息。
        /// 仅在需要远端地址的操作（如 ReceiveFrom/ReceiveMessageFrom）中有效；其他操作可能为空。
        /// </summary>
        public EndPoint? RemoteEndPoint { get; }

        /// <summary>
        /// 针对 <c>ReceiveMessageFrom</c> 操作的 IP 层报文信息（如本地 IP、接口等）。
        /// 仅在该类操作完成时有效，其他操作可忽略该字段。
        /// </summary>
        public IPPacketInformation ReceiveMessageFromPacketInfo { get; }

        /// <summary>
        /// 获取一个值，该值指示此结果实例是否为空（即未传输字节且无远端信息）。
        /// </summary>
        public bool IsEmpty => BytesTransferred == 0 && RemoteEndPoint == null;

        /// <summary>
        /// 初始化 <see cref="SocketOperationValue"/> 结构的新实例。
        /// </summary>
        /// <param name="bytesTransferred">传输的字节数。</param>
        /// <param name="remoteEndPoint">远端终结点。</param>
        /// <param name="receiveMessageFromPacketInfo">IP 包信息。</param>
        public SocketOperationValue(int bytesTransferred, EndPoint? remoteEndPoint, IPPacketInformation receiveMessageFromPacketInfo)
        {
            BytesTransferred = bytesTransferred;
            RemoteEndPoint = remoteEndPoint;
            ReceiveMessageFromPacketInfo = receiveMessageFromPacketInfo;
        }

        public static implicit operator int(SocketOperationValue result)
            => result.BytesTransferred;

        public static implicit operator EndPoint?(SocketOperationValue result)
            => result.RemoteEndPoint;

        public static implicit operator IPPacketInformation(SocketOperationValue result)
            => result.ReceiveMessageFromPacketInfo;
    }
}