using System.Net.Sockets;


namespace ExtenderApp.Common.NetWorks.LinkOperates
{
    public class LinkerOperation : ConcurrentOperation<Socket>
    {
        private readonly SocketAsyncEventArgs _socketAsyncEventArgs;
        private bool isSendAsync;
        private Action<int>? sendCountCallback;
        private int sendPacketsCount;

        public byte[] SendBytes;
        private Action<LinkerOperation>? sendBytesCallbcak;

        public LinkerOperation()
        {
            _socketAsyncEventArgs = new SocketAsyncEventArgs();
            _socketAsyncEventArgs.Completed += Completed;
            SendBytes = Array.Empty<byte>();
        }

        public void Set(byte[] bytes, int offset, int count, Action<int>? sendCountCallback, Action<LinkerOperation> sendBytesCallbcak = null)
        {
            _socketAsyncEventArgs.SetBuffer(bytes, offset, count);
            this.sendCountCallback = sendCountCallback;
            sendPacketsCount = count;
            this.sendBytesCallbcak = sendBytesCallbcak;
            SendBytes = bytes;
        }

        public void Set(Memory<byte> memory, Action<int>? sendCountCallback)
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
            sendBytesCallbcak?.Invoke(this);
        }

        public override bool TryReset()
        {
            if (isSendAsync)
                return false;

            SendBytes = Array.Empty<byte>();
            sendBytesCallbcak = null;
            sendCountCallback = null;
            return true;
        }
    }
}
