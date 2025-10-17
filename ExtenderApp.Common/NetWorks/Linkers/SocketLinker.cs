using System.Net;
using System.Net.Sockets;
using ExtenderApp.Common.Networks;


namespace ExtenderApp.Common.NetWorks.Linkers
{
    public class SocketLinker : Linker
    {
        private readonly Socket _socket;

        public SocketLinker(Socket socket)
        {
            _socket = socket;
        }

        public override bool NoDelay
        {
            get => _socket.NoDelay;
            set => _socket.NoDelay = NoDelay;
        }

        public override bool Connected => _socket.Connected;

        public override EndPoint? LocalEndPoint => _socket.LocalEndPoint;

        public override EndPoint? RemoteEndPoint => _socket.RemoteEndPoint;

        protected override int ExecuteSend(ReadOnlySpan<byte> span)
        {
            return _socket.Send(span, SocketFlags.None);
        }

        protected override ValueTask<int> ExecuteSendAsync(ReadOnlyMemory<byte> memory, CancellationToken token)
        {
            return _socket.SendAsync(memory, SocketFlags.None, token);
        }
    }
}
