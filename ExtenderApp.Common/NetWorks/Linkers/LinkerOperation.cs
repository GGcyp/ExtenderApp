using System.Net.Sockets;


namespace ExtenderApp.Common.Networks.LinkOperates
{
    public class LinkerOperation : ConcurrentOperation<LinkOperateData>
    {
        private readonly SocketAsyncEventArgs _socketAsyncEventArgs;
        private bool isSendAsync;

        private int sendTrafficLength;
        private Action<int>? sendTrafficCallback;

        private byte[] sendBytes;
        private Action<byte[]>? sendBytesCallbcak;

        public LinkerOperation()
        {
            _socketAsyncEventArgs = new SocketAsyncEventArgs();
            _socketAsyncEventArgs.Completed += Completed;
            sendBytes = Array.Empty<byte>();
        }

        public void Set(byte[] bytes, int offset, int length, Action<int>? sendCountCallback, Action<byte[]>? sendBytesCallbcak = null)
        {
            _socketAsyncEventArgs.SetBuffer(bytes, offset, length);
            this.sendTrafficCallback = sendCountCallback;
            sendTrafficLength = length;
            this.sendBytesCallbcak = sendBytesCallbcak;
            sendBytes = bytes;
        }

        public void Set(Memory<byte> memory, Action<int>? sendCountCallback)
        {
            _socketAsyncEventArgs.SetBuffer(memory);
            this.sendTrafficCallback = sendCountCallback;
            sendTrafficLength = memory.Length;
        }

        private void Completed(object? sender, SocketAsyncEventArgs e)
        {
            if (!isSendAsync)
                return;

            isSendAsync = false;
            sendTrafficCallback?.Invoke(sendTrafficLength);
            sendBytesCallbcak?.Invoke(sendBytes);
            Release();
        }

        public override void Execute(LinkOperateData item)
        {
            var socket = item.Socket;
            isSendAsync = socket.SendAsync(_socketAsyncEventArgs);
        }

        public override bool TryReset()
        {
            if (isSendAsync)
                return false;

            sendBytes = Array.Empty<byte>();
            sendBytesCallbcak = null;
            sendTrafficCallback = null;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            TryReset();
            base.Dispose(disposing);
        }
    }
}
