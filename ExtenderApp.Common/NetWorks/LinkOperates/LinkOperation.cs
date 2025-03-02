using System.Net.Sockets;


namespace ExtenderApp.Common.NetWorks.LinkOperates
{
    public class LinkOperation : ConcurrentOperation<Socket>
    {
        private readonly SocketAsyncEventArgs _socketAsyncEventArgs;
        private bool isSendAsync;

        public LinkOperation()
        {
            _socketAsyncEventArgs = new SocketAsyncEventArgs();
            _socketAsyncEventArgs.Completed += Completed;
        }

        public void Set(byte[] bytes, int offset, int count)
        {
            _socketAsyncEventArgs.SetBuffer(bytes, offset, count);
        }

        public void Set(Memory<byte> memory)
        {
            _socketAsyncEventArgs.SetBuffer(memory);
        }

        private void Completed(object? sender, SocketAsyncEventArgs e)
        {
            if (!isSendAsync)
                return;

            isSendAsync = false;
            Release();
        }

        public override void Execute(Socket item)
        {
            isSendAsync = item.SendAsync(_socketAsyncEventArgs);
        }

        public override bool TryReset()
        {
            if (isSendAsync)
                return false;
            return true;
        }
    }
}
