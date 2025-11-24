using System.Net;
using System.Net.Sockets;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一次套接字操作（发送/接收/接收自/带报文信息接收等）的结果数据。
    /// </summary>
    public readonly struct SocketOperationResult
    {
        /// <summary>
        /// 表示一个空的、无任何操作结果的 <see cref="SocketOperationResult"/> 实例。
        /// </summary>
        public static readonly SocketOperationResult Empty = new SocketOperationResult(true, 0, null, null, default);

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
        /// 本次操作产生的套接字异常。
        /// 成功时应为 <c>null</c>；失败时为具体的 <see cref="SocketException"/>。
        /// </summary>
        public SocketException? SocketError { get; }

        /// <summary>
        /// 针对 <c>ReceiveMessageFrom</c> 操作的 IP 层报文信息（如本地 IP、接口等）。
        /// 仅在该类操作完成时有效，其他操作可忽略该字段。
        /// </summary>
        public IPPacketInformation ReceiveMessageFromPacketInfo { get; }

        /// <summary>
        /// 获取一个值，该值指示操作是否成功完成（即没有套接字错误）。
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 获取一个值，该值指示此结果实例是否为空（即未传输字节、无远端信息且无错误）。
        /// </summary>
        public bool IsEmpty => BytesTransferred == 0 && RemoteEndPoint == null && SocketError == null;

        /// <summary>
        /// 初始化一个表示失败操作的 <see cref="SocketOperationResult"/> 新实例。
        /// </summary>
        /// <param name="socketError">操作失败时产生的套接字异常。</param>
        public SocketOperationResult(SocketException? socketError) : this(false, 0, null, socketError, default)
        {
        }

        /// <summary>
        /// 初始化 <see cref="SocketOperationResult"/> 结构的新实例。
        /// </summary>
        /// <param name="isSuccess">操作是否成功。</param>
        /// <param name="bytesTransferred">传输的字节数。</param>
        /// <param name="remoteEndPoint">远端终结点。</param>
        /// <param name="socketError">套接字异常（如有）。</param>
        /// <param name="receiveMessageFromPacketInfo">IP 包信息。</param>
        public SocketOperationResult(bool isSuccess, int bytesTransferred, EndPoint? remoteEndPoint, SocketException? socketError, IPPacketInformation receiveMessageFromPacketInfo)
        {
            IsSuccess = isSuccess;
            BytesTransferred = bytesTransferred;
            RemoteEndPoint = remoteEndPoint;
            SocketError = socketError;
            ReceiveMessageFromPacketInfo = receiveMessageFromPacketInfo;
        }

        /// <summary>
        /// 定义从 <see cref="SocketOperationResult"/> 到 <see cref="Result"/> 的隐式转换。
        /// </summary>
        /// <param name="result">要转换的套接字操作结果。</param>
        /// <returns>一个表示操作结果的 <see cref="Result"/> 实例。</returns>
        public static implicit operator Result(SocketOperationResult result)
            => new Result(result.IsSuccess, result.SocketError?.Message, result.SocketError);

        public static implicit operator bool(SocketOperationResult result)
            => result.IsSuccess;

        public static implicit operator int(SocketOperationResult result)
            => result.BytesTransferred;

        public static implicit operator SocketException?(SocketOperationResult result)
            => result.SocketError;

        public static implicit operator EndPoint?(SocketOperationResult result)
            => result.RemoteEndPoint;

        public static implicit operator IPPacketInformation(SocketOperationResult result)
            => result.ReceiveMessageFromPacketInfo;
    }
}