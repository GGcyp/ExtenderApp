using System.Net;
using System.Net.Sockets;
using ExtenderApp.Common.Networks;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    public abstract class SocketLinker : Linker
    {
        private static readonly ObjectPool<AwaitableSocketEventArgs> _pool
            = ObjectPool.CreateDefaultPool<AwaitableSocketEventArgs>();

        /// <summary>
        /// 当前链接器所使用的 Socket 实例。
        /// </summary>
        protected Socket Socket { get; }

        public SocketLinker(Socket socket)
        {
            Socket = socket;
        }

        public override bool Connected => Socket.Connected;

        public override EndPoint? LocalEndPoint => Socket.LocalEndPoint;

        public override EndPoint? RemoteEndPoint => Socket.RemoteEndPoint;

        /// <summary>
        /// 从对象池中获取 AwaitableSocketEventArgs 实例。
        /// </summary>
        /// <returns>AwaitableSocketEventArgs 实例</returns>
        protected AwaitableSocketEventArgs GetArgs() => _pool.Get();

        /// <summary>
        /// 向对象池中释放 AwaitableSocketEventArgs 实例。
        /// </summary>
        /// <param name="args">要被释放的实例</param>
        protected void ReleaseArgs(AwaitableSocketEventArgs args) => _pool.Release(args);

        protected override sealed ValueTask<SocketOperationResult> ExecuteSendAsync(Memory<byte> memory, CancellationToken token)
        {
            var args = _pool.Get();
            try
            {
                return ExecuteSendAsync(args, memory, token);
            }
            finally
            {
                _pool.Release(args);
            }
        }

        protected override sealed ValueTask<SocketOperationResult> ExecuteReceiveAsync(Memory<byte> memory, CancellationToken token)
        {
            var args = _pool.Get();
            try
            {
                return ExecuteReceiveAsync(args, memory, token);
            }
            finally
            {
                _pool.Release(args);
            }
        }

        protected SocketException CreateSocketException(SocketError error)
        {
            return new SocketException((int)error);
        }

        protected abstract ValueTask<SocketOperationResult> ExecuteSendAsync(AwaitableSocketEventArgs args, Memory<byte> memory, CancellationToken token);

        protected abstract ValueTask<SocketOperationResult> ExecuteReceiveAsync(AwaitableSocketEventArgs args, Memory<byte> memory, CancellationToken token);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            Socket.Dispose();
        }
    }
}