using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;
using System.Collections.Concurrent;

namespace ExtenderApp.Common.ConcurrentOperates
{
    /// <summary>
    /// 并发操作基类，继承自可释放对象类，并实现了可重置接口。
    /// </summary>
    public abstract class ConcurrentOperate : DisposableObject, IConcurrentOperate
    {
        /// <summary>
        /// 操作计数，用于跟踪当前有多少操作正在进行。
        /// </summary>
        protected volatile int operationCount;

        /// <summary>
        /// 指示是否正在执行操作。
        /// </summary>
        protected volatile int _isExecuting;

        /// <summary>
        /// 获取一个值，指示是否正在执行操作或操作计数是否小于等于0。
        /// </summary>
        public bool IsExecuting => Interlocked.CompareExchange(ref _isExecuting, 1, 0) == 0;

        public abstract bool CanOperate { get; protected set; }

        public abstract void Start();

        /// <summary>
        /// 尝试重置对象状态。
        /// </summary>
        /// <returns>如果重置成功，则返回true；否则返回false。</returns>
        public virtual bool TryReset()
        {
            return true;
        }
    }


    public class ConcurrentOperate<TData> : ConcurrentOperate, IConcurrentOperate<TData> where TData : ConcurrentOperateData
    {
        /// <summary>
        /// 操作队列
        /// </summary>
        private readonly ConcurrentQueue<IConcurrentOperation<TData>> _queue = new();

        /// <summary>
        /// 并发操作对象
        /// </summary>
        public TData Data { get; protected set; }

        /// <summary>
        /// 获取队列中的元素数量。
        /// </summary>
        /// <returns>返回队列中的元素数量。</returns>
        public int Count => _queue.Count;

        /// <summary>
        /// 获取或设置是否可以操作。
        /// </summary>
        /// <value>如果可以操作，则返回 true；否则返回 false。</value>
        public override bool CanOperate { get; protected set; }

        /// <summary>
        /// 启动方法
        /// </summary>
        /// <param name="policy">并发操作策略，可以为null</param>
        /// <param name="data">操作数据，可以为null</param>
        /// <param name="operate">操作类型，可以为null</param>
        public void Start(TData operate)
        {
            Data = operate ?? throw new ArgumentNullException(nameof(operate), "被操作类不能为空");
            CanOperate = true;

            ProtectedStart();
        }

        public override void Start()
        {
            if (Data == null)
            {
                throw new InvalidOperationException("并发操作策略或被操作类未设置");
            }
            ProtectedStart();
        }

        /// <summary>
        /// 受保护的启动方法
        /// </summary>
        protected virtual void ProtectedStart()
        {

        }

        /// <summary>
        /// 将操作加入队列
        /// </summary>
        /// <param name="operation">并发操作</param>
        public void QueueOperation(IConcurrentOperation<TData> operation)
        {
            if (!CanOperate)
                return;

            ThrowNull();
            Data.Token.ThrowIfCancellationRequested();
            operation.ArgumentNull(typeof(TData).FullName);

            _queue.Enqueue(operation);
            Interlocked.Increment(ref operationCount);

            //lock (_queue)
            //{
            //    if (!isExecuting)
            //    {
            //        isExecuting = true;
            //        Task.Run(Run);
            //    }
            //}

            if (Interlocked.CompareExchange(ref _isExecuting, 1, 0) == 0)
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ => Run(), null);
            }
        }

        /// <summary>
        /// 将操作集合加入队列
        /// </summary>
        /// <param name="operations">并发操作集合</param>
        public void QueueOperation(IEnumerable<IConcurrentOperation<TData>> operations)
        {
            if (!CanOperate)
                return;

            ThrowNull();
            Data.Token.ThrowIfCancellationRequested();
            operations.ArgumentNull(nameof(operations));

            foreach (var operation in operations)
            {
                _queue.Enqueue(operation);
                Interlocked.Increment(ref operationCount);
            }

            if (Interlocked.CompareExchange(ref _isExecuting, 1, 0) == 0)
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ => Run(), null);
            }
        }

        /// <summary>
        /// 执行单个并发操作
        /// </summary>
        /// <param name="operation">并发操作</param>
        public void ExecuteOperation(IConcurrentOperation<TData> operation)
        {
            if (!CanOperate)
                return;

            ThrowNull();
            Data.Token.ThrowIfCancellationRequested();
            operation.ArgumentNull(nameof(operation));

            lock (Data)
            {
                operation.Execute(Data);
            }
        }

        /// <summary>
        /// 执行并发操作集合
        /// </summary>
        /// <param name="operations">并发操作集合</param>
        public void ExecuteOperation(IEnumerable<IConcurrentOperation<TData>> operations)
        {
            if (!CanOperate)
                return;

            ThrowNull();
            Data.Token.ThrowIfCancellationRequested();
            operations.ArgumentNull(nameof(operations));

            foreach (var operation in operations)
            {
                lock (Data)
                {
                    operation.Execute(Data);
                }
            }
        }

        /// <summary>
        /// 运行队列中的并发操作
        /// </summary>
        private void Run()
        {
            try
            {
                if (Data.Token.IsCancellationRequested)
                {
                    return;
                }
                Execute();
            }
            finally
            {
                lock (_queue)
                {
                    //if (operationCount > 0)
                    //{
                    //    Task.Run(Run);
                    //    isExecuting = true;
                    //}
                    //else
                    //{
                    //    isExecuting = false;
                    //}
                    if (operationCount > 0 && Interlocked.CompareExchange(ref _isExecuting, 1, 0) != 0)
                    {
                        //Task.Run(Run);
                        ThreadPool.UnsafeQueueUserWorkItem(_ => Run(), null);
                    }
                }
            }
        }

        /// <summary>
        /// 执行队列中的所有操作
        /// </summary>
        private void Execute()
        {
            ThrowNull();

            while (_queue.TryDequeue(out var operation) &&
                !Data.Token.IsCancellationRequested &&
                !IsDisposed)
            {
                lock (Data)
                {
                    operation.Execute(Data);
                    Interlocked.Decrement(ref operationCount);
                }
                operation.Release();
            }
            Interlocked.Exchange(ref _isExecuting, 0);
        }

        /// <summary>
        /// 抛出空引用异常
        /// </summary>
        private void ThrowNull()
        {
            ThrowIfDisposed();
            Data.ArgumentNull(nameof(ExtenderApp.Data));
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            TryReset();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 重置资源
        /// </summary>
        /// <returns>是否重置成功</returns>
        public override bool TryReset()
        {
            Data = null;
            CanOperate = false;
            return true;
        }
    }
}
