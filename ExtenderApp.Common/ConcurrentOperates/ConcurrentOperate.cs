using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;
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
        /// 最大并发执行数，可由子类重写
        /// </summary>
        protected virtual int MaxConcurrency => 10;

        /// <summary>
        /// 并发信号量，控制最大执行数
        /// </summary>
        protected readonly SemaphoreSlim _concurrencySemaphore;

        protected ConcurrentOperate()
        {
            _concurrencySemaphore = new SemaphoreSlim(1, MaxConcurrency);
        }

        /// <summary>
        /// 获取一个值，指示是否正在执行操作
        /// </summary>
        public bool IsExecuting => _isExecuting > 0;

        public abstract bool CanOperate { get; protected set; }

        public abstract void Start();

        /// <summary>
        /// 尝试重置对象状态。
        /// </summary>
        /// <returns>如果重置成功，则返回true；否则返回false。</returns>
        public virtual bool TryReset()
        {
            operationCount = 0;
            Interlocked.Exchange(ref _isExecuting, 0);
            return true;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            _concurrencySemaphore.Dispose();
        }

        public abstract void Execute(IConcurrentOperation operation);

        public abstract void Execute(IEnumerable<IConcurrentOperation> operations);

        public abstract void ExecuteAsync(IConcurrentOperation operation);

        public abstract void ExecuteAsync(IEnumerable<IConcurrentOperation> operations);
    }

    /// <summary>
    /// 泛型并发操作类
    /// </summary>
    public class ConcurrentOperate<TData> : ConcurrentOperate, IConcurrentOperate<TData>
        where TData : ConcurrentOperateData
    {
        /// <summary>
        /// 操作队列
        /// </summary>
        protected readonly ConcurrentQueue<IConcurrentOperation<TData>> _concurrentQueue = new();

        /// <summary>
        /// 并发操作对象
        /// </summary>
        public TData Data { get; protected set; }

        /// <summary>
        /// 获取队列中的元素数量。
        /// </summary>
        public int Count => _concurrentQueue.Count;

        public override bool CanOperate { get; protected set; }

        /// <summary>
        /// 数据访问锁，使用细粒度锁
        /// </summary>
        private readonly object _dataLock = new();

        /// <summary>
        /// 取消令牌源
        /// </summary>
        private CancellationTokenSource _cts;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ConcurrentOperate() : base()
        {
            CanOperate = false;
            _cts = new CancellationTokenSource();
            Data = default;
        }

        /// <summary>
        /// 启动方法
        /// </summary>
        public void Start(TData operate)
        {
            Data = operate ?? throw new ArgumentNullException(nameof(operate), "被操作类不能为空");
            CanOperate = true;
            _cts = _cts.IsCancellationRequested ? new CancellationTokenSource() : _cts;
            ProtectedStart();
        }

        public override void Start()
        {
            if (Data == null)
            {
                throw new InvalidOperationException("并发操作策略或被操作类未设置");
            }
            CanOperate = true;
            _cts = _cts.IsCancellationRequested ? new CancellationTokenSource() : _cts;
            ProtectedStart();
        }

        /// <summary>
        /// 受保护的启动方法
        /// </summary>
        protected virtual void ProtectedStart()
        {

        }

        #region ExecuteAsync

        /// <summary>
        /// 将操作加入队列并异步执行
        /// </summary>
        public void ExecuteAsync(IConcurrentOperation<TData> operation)
        {
            if (!CanOperate || _cts.IsCancellationRequested)
                return;

            ValidateOperation(operation);

            _concurrentQueue.Enqueue(operation);
            int count = Interlocked.Increment(ref operationCount);

            // 启动处理任务（如果是第一个操作）
            if (count == 1 && Interlocked.CompareExchange(ref _isExecuting, 1, 0) == 0)
            {
                _ = ProcessQueueAsync();
            }
        }

        /// <summary>
        /// 将操作集合加入队列并异步执行
        /// </summary>
        public void ExecuteAsync(IEnumerable<IConcurrentOperation<TData>> operations)
        {
            if (!CanOperate || _cts.IsCancellationRequested)
                return;

            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            bool hasOperations = false;
            foreach (var operation in operations)
            {
                if (operation == null)
                    continue;

                ValidateOperation(operation);
                _concurrentQueue.Enqueue(operation);
                hasOperations = true;
            }

            if (!hasOperations)
                return;

            int count = Interlocked.Add(ref operationCount, 1);
            if (count == 1 && Interlocked.CompareExchange(ref _isExecuting, 1, 0) == 0)
            {
                _ = ProcessQueueAsync();
            }
        }

        public override void ExecuteAsync(IConcurrentOperation operation)
        {
            if (operation is not IConcurrentOperation<TData> dataOperation)
                throw new ArgumentNullException(nameof(operation));

            ExecuteAsync(dataOperation);
        }

        public override void ExecuteAsync(IEnumerable<IConcurrentOperation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            if (operations is not IEnumerable<IConcurrentOperation<TData>> dataOperations)
                throw new ArgumentNullException(nameof(operations));

            ExecuteAsync(dataOperations);
        }

        #endregion

        #region Execute

        public override void Execute(IConcurrentOperation operation)
        {
            if (operation is not IConcurrentOperation<TData> dataOperation)
                throw new ArgumentNullException(nameof(operation));

            Execute(dataOperation);
        }

        public override void Execute(IEnumerable<IConcurrentOperation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            if (operations is not IEnumerable<IConcurrentOperation<TData>> dataOperations)
                throw new ArgumentNullException(nameof(operations));

            Execute(dataOperations);
        }

        /// <summary>
        /// 立即执行单个并发操作
        /// </summary>
        public void Execute(IConcurrentOperation<TData> operation)
        {
            if (!CanOperate || _cts.IsCancellationRequested)
                return;

            ValidateOperation(operation);

            // 使用细粒度锁保护数据访问
            lock (_dataLock)
            {
                if (!_cts.IsCancellationRequested)
                {
                    operation.Execute(Data);
                }
            }
        }

        /// <summary>
        /// 立即执行并发操作集合
        /// </summary>
        public void Execute(IEnumerable<IConcurrentOperation<TData>> operations)
        {
            if (!CanOperate || _cts.IsCancellationRequested)
                return;

            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            // 使用细粒度锁保护数据访问
            lock (_dataLock)
            {

                foreach (var operation in operations)
                {
                    if (_cts.IsCancellationRequested)
                        return;

                    if (operation != null)
                    {
                        operation.Execute(Data);
                    }
                }
            }
        }

        #endregion

        #region  ExecuteOperation

        /// <summary>
        /// 异步处理队列中的操作
        /// </summary>
        private async Task ProcessQueueAsync()
        {
            try
            {
                while (true)
                {
                    // 等待并发许可
                    await _concurrencySemaphore.WaitAsync();

                    try
                    {
                        // 检查取消请求
                        if (_cts.IsCancellationRequested)
                            return;

                        // 尝试获取操作
                        if (!_concurrentQueue.TryDequeue(out var operation))
                            break;

                        // 执行操作并减少计数
                        await ExecuteOperationAsync(operation);
                    }
                    catch (Exception ex)
                    {
                        // 记录操作执行异常
                        Console.WriteLine($"操作执行异常: {ex.Message}");
                    }
                    finally
                    {
                        // 释放并发许可
                        _concurrencySemaphore.Release();

                        // 检查是否有新操作需要处理
                        if (operationCount > 0 && Interlocked.CompareExchange(ref _isExecuting, 1, 0) != 0)
                        {
                            // 短暂延迟，避免CPU过度占用
                            await Task.Delay(10);
                        }
                    }
                }
            }
            finally
            {
                // 重置执行状态
                Interlocked.Exchange(ref _isExecuting, 0);
            }
        }

        /// <summary>
        /// 执行单个操作
        /// </summary>
        private async Task ExecuteOperationAsync(IConcurrentOperation<TData> operation)
        {
            try
            {
                // 使用细粒度锁保护数据访问
                lock (_dataLock)
                {
                    if (_cts.IsCancellationRequested)
                        return;

                    operation.Execute(Data);
                }

                // 模拟异步操作（可根据实际情况修改）
                await Task.Yield();
            }
            finally
            {
                // 减少操作计数并释放资源
                Interlocked.Decrement(ref operationCount);
                operation.Release();
            }
        }

        private void ValidateOperation(IConcurrentOperation<TData> operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            ThrowNull();
            operation.ArgumentNull();
        }

        #endregion

        public void Cancel()
        {
            ThrowIfDisposed();
            _cts.Cancel();
        }

        private void ThrowNull()
        {
            ThrowIfDisposed();
            if (Data == null)
                throw new InvalidOperationException("Data未初始化");
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            try
            {
                // 取消所有操作
                _cts.Cancel();

                // 清空队列并释放操作
                while (_concurrentQueue.TryDequeue(out var operation))
                {
                    try { operation.Release(); }
                    catch { /* 忽略释放异常 */ }
                }

                // 重置操作计数
                Interlocked.Exchange(ref operationCount, 0);
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// 重置资源
        /// </summary>
        public override bool TryReset()
        {
            // 取消当前操作
            _cts.Cancel();

            // 清空队列
            while (_concurrentQueue.TryDequeue(out var operation))
            {
                try { operation.Release(); }
                catch { /* 忽略释放异常 */ }
            }

            // 重置状态
            Interlocked.Exchange(ref operationCount, 0);
            Interlocked.Exchange(ref _isExecuting, 0);
            CanOperate = false;

            return base.TryReset();
        }
    }
}