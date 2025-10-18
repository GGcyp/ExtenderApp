using System.Net;
using System.Net.Sockets;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一次套接字操作（发送/接收/接收自/带报文信息接收等）的结果数据。
    /// </summary>
    /// <remarks>
    /// 该结构通常由底层 Socket/SocketAsyncEventArgs 回调或 awaitable 包装在操作完成时填充，
    /// 用于向上层传递本次操作的状态、数据长度以及可选的远端信息和 IP 层报文信息。
    /// </remarks>
    public struct SocketOperationResult
    {
        /// <summary>
        /// 实际传输的字节数。
        /// 对于接收操作，值为 0 通常表示对端已优雅关闭连接。
        /// </summary>
        public int BytesTransferred;

        /// <summary>
        /// 远端终结点信息。
        /// 仅在需要远端地址的操作（如 ReceiveFrom/ReceiveMessageFrom）中有效；其他操作可能为空。
        /// </summary>
        public EndPoint? RemoteEndPoint;

        /// <summary>
        /// 本次操作产生的套接字异常。
        /// 成功时应为 <c>null</c>；失败时为具体的 <see cref="SocketException"/>。
        /// </summary>
        public SocketException? SocketError;

        /// <summary>
        /// 针对 <c>ReceiveMessageFrom</c> 操作的 IP 层报文信息（如本地 IP、接口等）。
        /// 仅在该类操作完成时有效，其他操作可忽略该字段。
        /// </summary>
        public IPPacketInformation ReceiveMessageFromPacketInfo;

        /// <summary>
        /// 当前返回值的结果状态码。
        /// </summary>
        public ResultCode Code => SocketError == null ? ResultCode.Success : ResultCode.Failed;

        public SocketOperationResult(SocketException? socketError) : this(0, null, socketError, default)
        {
        }

        public SocketOperationResult(int bytesTransferred, EndPoint? remoteEndPoint, SocketException? socketError, IPPacketInformation receiveMessageFromPacketInfo)
        {
            BytesTransferred = bytesTransferred;
            RemoteEndPoint = remoteEndPoint;
            SocketError = socketError;
            ReceiveMessageFromPacketInfo = receiveMessageFromPacketInfo;
        }

        public static implicit operator Result(SocketOperationResult result)
            => new Result(result.Code, result.SocketError?.Message);
    }
}