using System;
using System.Buffers;
using System.Net;
using System.Runtime.ExceptionServices;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 抽象基类 Linker，继承自 ConcurrentOperate 类，实现了 ILinker 接口。
    /// </summary>
    /// <typeparam name="TPolicy">操作策略类型，需要继承自 LinkOperatePolicy 并约束 TData 类型。</typeparam>
    /// <typeparam name="TData">数据类型，需要继承自 LinkerData。</typeparam>
    public abstract class Linker : DisposableObject, ILinker
    {
        private const int DefaultCapacity = 1024 * 512;

        private readonly SemaphoreSlim _sendSlim;

        protected readonly ExceptionDispatchInfo ExceptionDispatchInfo;
        public CapacityLimiter CapacityLimiter { get; }

        public ValueCounter SendCounter { get; }

        public ValueCounter ReceiveCounter { get; }

        #region 子类实现

        public abstract bool NoDelay { get; set; }

        public abstract bool Connected { get; }

        public abstract EndPoint? LocalEndPoint { get; }

        public abstract EndPoint? RemoteEndPoint { get; }

        #endregion 子类实现

        public Linker() : this(DefaultCapacity)
        {

        }

        public Linker(long capacity)
        {
            _sendSlim = new(1, 1);

            CapacityLimiter = new(capacity);

            SendCounter = new();
            ReceiveCounter = new();
            SendCounter.Start();
            ReceiveCounter.Start();
        }

        public int Send(ref ByteBuffer buffer)
        {
            if (buffer.IsEmpty || buffer.Remaining == 0)
                return 0;

            ThrowIfDisposed();
            var lease = CapacityLimiter.Acquire(buffer.Remaining);
            _sendSlim.Wait();
            try
            {
                int len = ExecuteSend(buffer);
                SendCounter.Increment(len);
                buffer.ReadAdvance(len);
                return len;
            }
            finally
            {
                _sendSlim.Release();
                lease.Dispose();
            }
        }

        public async Task<int> SendAsync(ReadOnlySequence<byte> readOnlyMemories, CancellationToken token = default)
        {
            if (readOnlyMemories.IsEmpty || readOnlyMemories.Length == 0)
                return 0;

            ThrowIfDisposed();
            var lease = await CapacityLimiter.AcquireAsync(readOnlyMemories.Length, token).ConfigureAwait(false);
            await _sendSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                return await ExecuteSendAsync(readOnlyMemories, token).ConfigureAwait(false);
            }
            finally
            {
                _sendSlim.Release();
                lease.Dispose();
            }
        }

        protected abstract int ExecuteSend(ByteBuffer buffer);

        protected abstract Task<int> ExecuteSendAsync(in ReadOnlySequence<byte> readOnlyMemories, CancellationToken token);

        protected override void Dispose(bool disposing)
        {
            SendCounter.Dispose();
            ReceiveCounter.Dispose();
            _sendSlim.Dispose();
            CapacityLimiter.Dispose();
        }
    }
}