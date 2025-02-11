using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.StreamOperates
{
    /// <summary>
    /// 泛型流操作类，实现了IResettable和IDisposable接口
    /// </summary>
    /// <typeparam name="TValue">泛型类型参数</typeparam>
    public class StreamOperate : DisposableObject, IResettable
    {
        /// <summary>
        /// 私有并发队列，用于存储流操作对象
        /// </summary>
        private readonly ConcurrentQueue<IStreamOperation> _queue;

        /// <summary>
        /// 获取或设置释放操作
        /// </summary>
        /// <value>
        /// 一个执行释放操作的委托，该委托接受一个 <see cref="IStreamOperation"/> 类型的参数
        /// </value>
        public Action<IStreamOperation> ReleaseAtion { get; set; }

        /// <summary>
        /// 受保护的流对象
        /// </summary>
        protected Stream? stream;

        /// <summary>
        /// 受保护的文件操作信息对象
        /// </summary>
        protected FileOperateInfo OperateInfo;

        /// <summary>
        /// 获取本地文件信息
        /// </summary>
        /// <returns>返回本地文件信息对象</returns>
        protected LocalFileInfo LocalFileInformation => OperateInfo.LocalFileInfo;

        /// <summary>
        /// 私有变量，跟踪队列中的操作数量
        /// </summary>
        private volatile int operationCount;

        /// <summary>
        /// 私有变量，表示当前是否正在执行操作
        /// </summary>
        private volatile bool isExecuting;
        /// <summary>
        /// 获取当前是否正在执行操作
        /// </summary>
        /// <returns>如果正在执行操作，则返回true；否则返回false</returns>
        public bool IsExecuting => isExecuting;

        /// <summary>
        /// 构造函数，初始化并发队列
        /// </summary>
        public StreamOperate()
        {
            _queue = new();
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="info">文件操作信息对象</param>
        internal void OpenFile(FileOperateInfo info)
        {
            this.OperateInfo = info;
            stream = info.OpenFile();
            CheckFileOperateInfo();
        }

        /// <summary>
        /// 检查文件操作信息
        /// </summary>
        protected virtual void CheckFileOperateInfo()
        {

        }

        /// <summary>
        /// 设置流操作
        /// </summary>
        /// <param name="operation">流操作对象</param>
        public void SetOperation(IStreamOperation operation)
        {
            stream.ArgumentNull(nameof(stream));
            operation.ArgumentNull(nameof(operation));
            _queue.Enqueue(operation);
            Interlocked.Increment(ref operationCount);

            lock (_queue)
            {
                if (!isExecuting)
                {
                    isExecuting = true;
                    Task.Run(Run);
                }
            }
        }

        /// <summary>
        /// 私有方法，执行流操作
        /// </summary>
        private void Run()
        {
            try
            {
                BeforeExecute();
                Execute();
                AfterExecute();
            }
            finally
            {
                lock (_queue)
                {
                    if (operationCount > 0)
                    {
                        Task.Run(Run);
                        isExecuting = true;
                    }
                    else
                    {
                        isExecuting = false;
                    }
                }
            }
        }

        /// <summary>
        /// 受保护虚方法，在执行操作前调用
        /// </summary>
        protected virtual void BeforeExecute()
        {

        }

        /// <summary>
        /// 私有方法，执行队列中的流操作
        /// </summary>
        private void Execute()
        {
            stream.ArgumentNull(nameof(stream));

            while (_queue.Count > 0)
            {
                if (!_queue.TryDequeue(out var operation))
                {
                    ErrorUtil.Operation("在取出文件处理操作时出现错误");
                    break;
                }

                lock (stream)
                {
                    operation.Execute(stream);
                    Interlocked.Decrement(ref operationCount);
                    ProtectedExcute();
                    ReleaseAtion.Invoke(operation);
                }
            }
        }

        /// <summary>
        /// 受保护的执行方法。
        /// </summary>
        protected virtual void ProtectedExcute()
        {

        }

        /// <summary>
        /// 受保护虚方法，在执行操作后调用
        /// </summary>
        protected virtual void AfterExecute()
        {

        }

        /// <summary>
        /// 实现IDisposable接口，释放资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            TryReset();
        }

        /// <summary>
        /// 实现IResettable接口，尝试重置对象状态
        /// </summary>
        /// <returns>是否重置成功</returns>
        public virtual bool TryReset()
        {
            if (_queue.Count > 0)
                ErrorUtil.Operation("队列中还有操作未执行完毕");

            stream?.Dispose();
            stream = null;
            return true;
        }
    }
}
