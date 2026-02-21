using System.Net;
using System.Net.Sockets;

namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 表示链路操作的结果值，包含传输字节数、远端终结点信息及IP层报文信息。
    /// </summary>
    public readonly struct LinkOperationValue
    {
        /// <summary>
        /// 表示一个空的 <see cref="LinkOperationValue"/>，即未发生任何操作或无有效结果。
        /// </summary>
        public static readonly LinkOperationValue Empty = new LinkOperationValue(0, null, default);

        /// <summary>
        /// 获取实际传输的字节数。
        /// 对于接收操作，若为 0 通常表示远端已优雅关闭连接。
        /// </summary>
        public int BytesTransferred { get; }

        /// <summary>
        /// 获取远端终结点信息。
        /// 仅在需要远端地址的操作（如 <c>ReceiveFrom</c>、<c>ReceiveMessageFrom</c>）中有效，其余操作为 <c>null</c>。
        /// </summary>
        public EndPoint? RemoteEndPoint { get; }

        /// <summary>
        /// 获取针对 <c>ReceiveMessageFrom</c> 操作的 IP 层报文信息（如本地 IP、接口等）。
        /// 仅在该类操作完成时有效，其余操作可忽略。
        /// </summary>
        public IPPacketInformation ReceiveMessageFromPacketInfo { get; }

        /// <summary>
        /// 获取一个值，指示此结果是否为空（即未传输字节且无远端信息）。
        /// </summary>
        public bool IsEmpty => BytesTransferred == 0 && RemoteEndPoint == null;

        /// <summary>
        /// 初始化 <see cref="LinkOperationValue"/> 结构的新实例。
        /// </summary>
        /// <param name="bytesTransferred">传输的字节数。</param>
        /// <param name="remoteEndPoint">远端终结点。</param>
        /// <param name="receiveMessageFromPacketInfo">IP 层报文信息。</param>
        public LinkOperationValue(int bytesTransferred, EndPoint? remoteEndPoint, IPPacketInformation receiveMessageFromPacketInfo)
        {
            BytesTransferred = bytesTransferred;
            RemoteEndPoint = remoteEndPoint;
            ReceiveMessageFromPacketInfo = receiveMessageFromPacketInfo;
        }

        /// <summary>
        /// 隐式转换为传输的字节数。
        /// </summary>
        /// <param name="result">操作结果。</param>
        /// <returns>实际传输的字节数。</returns>
        public static implicit operator int(LinkOperationValue result)
            => result.BytesTransferred;

        /// <summary>
        /// 隐式转换为远端终结点。
        /// </summary>
        /// <param name="result">操作结果。</param>
        /// <returns>远端终结点信息。</returns>
        public static implicit operator EndPoint?(LinkOperationValue result)
            => result.RemoteEndPoint;

        /// <summary>
        /// 隐式转换为 IP 层报文信息。
        /// </summary>
        /// <param name="result">操作结果。</param>
        /// <returns>IP 层报文信息。</returns>
        public static implicit operator IPPacketInformation(LinkOperationValue result)
            => result.ReceiveMessageFromPacketInfo;
    }
}