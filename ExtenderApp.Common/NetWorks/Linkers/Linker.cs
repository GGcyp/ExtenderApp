using System.Net.Sockets;
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
        private readonly SemaphoreSlim _sendSlim;

        private ValueCounter sendCounter;

        private ValueCounter receiveCounter;

        public bool NoDelay { get; protected set; }


        public Linker()
        {
            _sendSlim = new(1, 1);

            sendCounter = new();
            receiveCounter = new();
        }

        //public void Send(in ReadOnlySpan<byte> span)
        //{
        //    ByteBlock block = new(span);
        //    Send(ref block);
        //    block.Dispose();
        //}

        //public void Send(in ReadOnlyMemory<byte> memory)
        //{
        //    ByteBuffer buffer = new(memory);
        //    Send(ref buffer);
        //    buffer.Dispose();
        //}

        //public void Send(ref ByteBlock block)
        //{
        //    ByteBuffer buffer = block;
        //    Send(ref buffer);
        //    buffer.Dispose();
        //}

        public void Send(ref ByteBuffer buffer)
        {
            _sendSlim.Wait();
            try
            {
                ExecuteSend(ref buffer);
            }
            finally
            {
                _sendSlim.Release();
            }
        }

        protected abstract void ExecuteSend(ref ByteBuffer buffer);

        protected override void Dispose(bool disposing)
        {
            sendCounter.Dispose();
            receiveCounter.Dispose();
        }
    }
}
