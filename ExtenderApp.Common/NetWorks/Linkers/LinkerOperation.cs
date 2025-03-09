using System.Net.Sockets;


namespace ExtenderApp.Common.NetWorks.LinkOperates
{
    public class LinkerOperation : ConcurrentOperation<Socket>
    {
        private readonly SocketAsyncEventArgs _socketAsyncEventArgs;
        private bool isSendAsync;
        private Action<long>? sendCountCallback;
        private long sendPacketsCount;

        public LinkerOperation()
        {
            _socketAsyncEventArgs = new SocketAsyncEventArgs();
            _socketAsyncEventArgs.Completed += Completed;
        }

        public void Set(byte[] bytes, int offset, int count, Action<long>? sendCountCallback = null)
        {
            _socketAsyncEventArgs.SetBuffer(bytes, offset, count);
            this.sendCountCallback = sendCountCallback;
            sendPacketsCount = count;
        }

        public void Set(Memory<byte> memory, Action<long>? sendCountCallback = null)
        {
            _socketAsyncEventArgs.SetBuffer(memory);
            this.sendCountCallback = sendCountCallback;
            sendPacketsCount = memory.Length;
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
            sendCountCallback?.Invoke(sendPacketsCount);
        }

        public override bool TryReset()
        {
            if (isSendAsync)
                return false;
            return true;
        }
    }
}
