using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.ObjectPools;
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

        public abstract void Release();

        /// <summary>
        /// 尝试重置对象状态。
        /// </summary>
        /// <returns>如果重置成功，则返回true；否则返回false。</returns>
        public virtual bool TryReset()
        {
            return true;
        }
    }

    /// <summary>
    /// 并发操作抽象基类
    /// </summary>
    /// <typeparam name="TPolicy">并发操作策略类型，必须实现 IConcurrentOperatePolicy 接口并具备 IDisposable 接口</typeparam>
    /// <typeparam name="TOperate">并发操作类型，必须实现 IDisposable 接口</typeparam>
    /// <typeparam name="TData">并发操作数据类型，必须继承自 ConcurrentOperateData 类</typeparam>
    public class ConcurrentOperate<TPolicy, TOperate, TData> : ConcurrentOperate, IConcurrentOperate<TOperate, TData>
        where TPolicy : class, IConcurrentOperatePolicy<TOperate, TData>, IDisposable
        where TOperate : class, IDisposable
        where TData : ConcurrentOperateData
    {
        #region Pool

        /// <summary>
        /// 创建一个默认的对象池，用于管理ConcurrentOperate<TPolicy, TOperate, TData>对象的创建和重用。
        /// </summary>
        /// <returns>返回创建好的对象池。</returns>
        private readonly static ObjectPool<ConcurrentOperate<TPolicy, TOperate, TData>> _pool =
            ObjectPool.CreateDefaultPool<ConcurrentOperate<TPolicy, TOperate, TData>>();

        /// <summary>
        /// 从对象池中获取一个IConcurrentOperate<TOperate, TData>对象。
        /// </summary>
        /// <returns>返回从对象池中获取的IConcurrentOperate<TOperate, TData>对象。</returns>
        public static IConcurrentOperate<TOperate, TData> Get() => _pool.Get();

        /// <summary>
        /// 将一个IConcurrentOperate<TOperate, TData>对象释放回对象池。
        /// </summary>
        /// <param name="obj">要释放回对象池的IConcurrentOperate<TOperate, TData>对象。</param>
        public void Release(IConcurrentOperate<TOperate, TData> obj) => _pool.Release(obj);

        /// <summary>
        /// 获取一个泛型类型的实例，该类型必须实现IConcurrentOperate接口。
        /// </summary>
        /// <typeparam name="T">泛型类型，必须实现IConcurrentOperate接口</typeparam>
        /// <returns>泛型类型的实例</returns>
        public static T Get<T>() where T : class, IConcurrentOperate<TOperate, TData>
            => (T)Get();

        /// <summary>
        /// 释放一个泛型类型的实例，该类型必须实现IConcurrentOperate接口。
        /// </summary>
        /// <typeparam name="T">泛型类型，必须实现IConcurrentOperate接口</typeparam>
        /// <param name="obj">需要释放的实例</param>
        public void Release<T>(T obj) where T : class, IConcurrentOperate<TOperate, TData>
            => Release(obj);



        #endregion

        /// <summary>
        /// 操作队列
        /// </summary>
        private readonly ConcurrentQueue<IConcurrentOperation<TOperate>> _queue = new();

        /// <summary>
        /// 并发操作策略
        /// </summary>
        public TPolicy Policy { get; protected set; }

        /// <summary>
        /// 并发操作数据
        /// </summary>
        public TData Data { get; protected set; }

        /// <summary>
        /// 并发操作对象
        /// </summary>
        public TOperate Operate { get; protected set; }

        /// <summary>
        /// 获取队列中的元素数量。
        /// </summary>
        /// <returns>返回队列中的元素数量。</returns>
        public int Count => _queue.Count;

        /// <summary>
        /// 获取或设置是否可以操作。
        /// </summary>
        /// <value>如果可以操作，则返回 true；否则返回 false。</value>
        public bool CanOperate { get; protected set; }

        /// <summary>
        /// 启动方法
        /// </summary>
        /// <param name="policy">并发操作策略，可以为null</param>
        /// <param name="data">操作数据，可以为null</param>
        /// <param name="operate">操作类型，可以为null</param>
        public void Start(IConcurrentOperatePolicy<TOperate, TData>? policy = null, TData? data = null, TOperate? operate = null)
        {
            //policy.ArgumentObjectNull(typeof(TPolicy).FullName);
            Policy.ArgumentObjectNull(typeof(TPolicy).FullName);

            if (policy != null)
                this.Policy = (TPolicy)policy;

            Data = Data ?? data ?? Policy.GetData();
            Operate = Operate ?? operate ?? Policy.Create(Data);
            CanOperate = true;

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
        public void QueueOperation(IConcurrentOperation<TOperate> operation)
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
        public void QueueOperation(IEnumerable<IConcurrentOperation<TOperate>> operations)
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
        public void ExecuteOperation(IConcurrentOperation<TOperate> operation)
        {
            if (!CanOperate)
                return;

            ThrowNull();
            Data.Token.ThrowIfCancellationRequested();
            operation.ArgumentNull(nameof(operation));

            Policy.BeforeExecute(Operate, Data);
            lock (Operate)
            {
                operation.Execute(Operate);
            }
            Policy.AfterExecute(Operate, Data);
        }

        /// <summary>
        /// 执行并发操作集合
        /// </summary>
        /// <param name="operations">并发操作集合</param>
        public void ExecuteOperation(IEnumerable<IConcurrentOperation<TOperate>> operations)
        {
            if (!CanOperate)
                return;

            ThrowNull();
            Data.Token.ThrowIfCancellationRequested();
            operations.ArgumentNull(nameof(operations));

            Policy.BeforeExecute(Operate, Data);
            foreach (var operation in operations)
            {
                lock (Operate)
                {
                    operation.Execute(Operate);
                }
            }
            Policy.AfterExecute(Operate, Data);
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

                Policy.BeforeExecute(Operate, Data);
                Execute();
                Policy.AfterExecute(Operate, Data);
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

            while (_queue.Count > 0 && !Data.Token.IsCancellationRequested && !IsDisposed)
            {
                if (!_queue.TryDequeue(out var operation))
                {
                    ErrorUtil.Operation("在取出文件处理操作时出现错误");
                    break;
                }

                lock (Operate)
                {
                    operation.Execute(Operate);
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
            Operate.ArgumentNull(nameof(Data));
            Data.ArgumentNull(nameof(Data));
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
            Operate.Dispose();
            Operate = null;
            Policy.ReleaseData(Data);
            Data = null;
            CanOperate = false;
            return true;
        }

        public override void Release()
        {
            Release(this);
        }
    }
}
