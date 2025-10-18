using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// UDP连接类，继承自Linker类，并实现IUdpLinker接口。
    /// </summary>
    internal class UdpLinker : SocketLinker, IUdpLinker
    {
        private EndPoint? endPoint;

        public override EndPoint? RemoteEndPoint => endPoint;

        public UdpLinker(Socket socket) : base(socket)
        {
        }

        protected override ValueTask ExecuteConnectAsync(EndPoint remoteEndPoint, CancellationToken token)
        {
            endPoint = remoteEndPoint;
            return ValueTask.CompletedTask;
        }

        protected override ValueTask ExecuteDisconnectAsync(CancellationToken token)
        {
            endPoint = null;
            return ValueTask.CompletedTask;
        }

        protected override ValueTask<SocketOperationResult> ExecuteReceiveAsync(AwaitableSocketEventArgs args, Memory<byte> memory, CancellationToken token)
        {
            if (endPoint == null)
            {
                return ValueTask.FromResult(new SocketOperationResult(CreateSocketException(SocketError.NotConnected)));
            }
            return args.ReceiveFromAsync(Socket, memory, endPoint, token);
        }

        protected override ValueTask<SocketOperationResult> ExecuteSendAsync(AwaitableSocketEventArgs args, Memory<byte> memory, CancellationToken token)
        {
            if (endPoint == null)
            {
                return ValueTask.FromResult(new SocketOperationResult(CreateSocketException(SocketError.NotConnected)));
            }
            return args.SendToAsync(Socket, memory, endPoint, token);
        }

        public SocketOperationResult SendTo(Memory<byte> memory, EndPoint endPoint)
        {
            if (memory.IsEmpty || memory.Length <= 0)
            {
                return new SocketOperationResult(CreateSocketException(SocketError.NoBufferSpaceAvailable));
            }

            if (endPoint == null)
            {
                return new SocketOperationResult(CreateSocketException(SocketError.NotConnected));
            }

            var args = GetArgs();
            try
            {
                return args.SendToAsync(Socket, memory, endPoint, default).GetAwaiter().GetResult();
            }
            finally
            {
                ReleaseArgs(args);
            }
        }

        public ValueTask<SocketOperationResult> SendToAsync(Memory<byte> memory, EndPoint endPoint, CancellationToken token = default)
        {
            if (memory.IsEmpty || memory.Length <= 0)
            {
                return ValueTask.FromResult(new SocketOperationResult(CreateSocketException(SocketError.NoBufferSpaceAvailable)));
            }

            if (endPoint == null)
            {
                return ValueTask.FromResult(new SocketOperationResult(CreateSocketException(SocketError.NotConnected)));
            }

            var args = GetArgs();
            try
            {
                return args.SendToAsync(Socket, memory, endPoint, default);
            }
            finally
            {
                ReleaseArgs(args);
            }
        }
    }
}