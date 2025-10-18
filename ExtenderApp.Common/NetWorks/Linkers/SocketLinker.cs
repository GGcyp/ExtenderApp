using System.Buffers;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;


namespace ExtenderApp.Common.NetWorks
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

        protected override int ExecuteSend(ByteBuffer buffer)
        {
            int totalSent = 0;
            while (!buffer.End)
            {
                int len = _socket.Send(buffer.UnreadSpan);
                if (len <= 0)
                    break;
                totalSent += len;
                buffer.ReadAdvance(len);
            }
            return totalSent;
        }

        protected override Task<int> ExecuteSendAsync(in ReadOnlySequence<byte> readOnlyMemories, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
