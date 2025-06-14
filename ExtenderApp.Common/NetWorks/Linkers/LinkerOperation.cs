using System.Net.Sockets;


namespace ExtenderApp.Common.Networks.LinkOperates
{
    public class LinkerOperation : ConcurrentOperation<LinkOperateData>
    {
        private readonly SocketAsyncEventArgs _socketAsyncEventArgs;
        private bool isSendAsync;
        private Action<int>? sendCountCallback;
        private int sendPacketsCount;

        private byte[] sendBytes;
        private Action<byte[]>? sendBytesCallbcak;

        public LinkerOperation()
        {
            _socketAsyncEventArgs = new SocketAsyncEventArgs();
            _socketAsyncEventArgs.Completed += Completed;
            sendBytes = Array.Empty<byte>();
        }

        public void Set(byte[] bytes, int offset, int count, Action<int>? sendCountCallback, Action<byte[]>? sendBytesCallbcak = null)
        {
            _socketAsyncEventArgs.SetBuffer(bytes, offset, count);
            this.sendCountCallback = sendCountCallback;
            sendPacketsCount = count;
            this.sendBytesCallbcak = sendBytesCallbcak;
            sendBytes = bytes;
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

        public override void Execute(LinkOperateData item)
        {
            var socket = item.Socket;
            isSendAsync = socket.SendAsync(_socketAsyncEventArgs);
            sendCountCallback?.Invoke(sendPacketsCount);
            sendBytesCallbcak?.Invoke(sendBytes);
        }

        public override bool TryReset()
        {
            if (isSendAsync)
                return false;

            sendBytes = Array.Empty<byte>();
            sendBytesCallbcak = null;
            sendCountCallback = null;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            TryReset();
            base.Dispose(disposing);
        }
    }
}
