using System.Threading.Tasks.Sources;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 封装一个文件传输请求及其决策过程。
    /// 允许事件订阅者接受或拒绝该请求，并可通过 Task 异步等待决策结果。
    /// </summary>
    public class FileRequestDecision : DisposableObject, IValueTaskSource<Result<BitFieldData>>
    {
        /// <summary>
        /// 结构化的手动重置值任务源核心，用于管理异步操作的状态和结果。
        /// </summary>
        private ManualResetValueTaskSourceCore<Result<BitFieldData>> vts;

        /// <summary>
        /// 获取原始的文件传输请求数据。
        /// </summary>
        public FileDtoRequest Request { get; }

        /// <summary>
        /// 文件传输决策的位字段数据。
        /// </summary>
        public BitFieldData FileDecision { get; }

        /// <summary>
        /// 获取此操作的版本，用于 ValueTask 验证。
        /// </summary>
        public short Version => vts.Version;

        /// <summary>
        /// 初始化 <see cref="FileRequestDecision"/> 的新实例。
        /// </summary>
        /// <param name="request">收到的文件传输请求。</param>
        public FileRequestDecision(FileDtoRequest request)
        {
            Request = request;
            FileDecision = new(request.FileDtos.Count);
        }

        /// <summary>
        /// 同意文件传输请求。
        /// </summary>
        public void Accept()
        {
            if (FileDecision.AllFalse)
            {
                vts.SetResult(Result.Unsuccess<Result<BitFieldData>>("未选择任何文件"));
            }
            else
            {
                vts.SetResult(Result.Success(FileDecision));
            }
        }

        /// <summary>
        /// 拒绝文件传输请求。
        /// </summary>
        public void Reject()
        {
            vts.SetResult(Result.Unsuccess<Result<BitFieldData>>());
        }

        /// <summary>
        /// 获取操作的结果。此方法是 IValueTaskSource<TResult> 接口实现的一部分。
        /// </summary>
        /// <param name="token">用于验证操作的令牌。</param>
        /// <returns>操作的结果。</returns>
        public Result<BitFieldData> GetResult(short token)
        {
            return vts.GetResult(token);
        }

        /// <summary>
        /// 获取操作的当前状态。此方法是 IValueTaskSource 接口实现的一部分。
        /// </summary>
        /// <param name="token">用于验证操作的令牌。</param>
        /// <returns>操作的当前状态。</returns>
        public ValueTaskSourceStatus GetStatus(short token)
        {
            return vts.GetStatus(token);
        }

        /// <summary>
        /// 当操作完成时，安排要调用的延续操作。此方法是 IValueTaskSource 接口实现的一部分。
        /// </summary>
        /// <param name="continuation">要调用的延续操作。</param>
        /// <param name="state">要传递给延续操作的状态对象。</param>
        /// <param name="token">用于验证操作的令牌。</param>
        /// <param name="flags">控制延续操作行为的标志。</param>
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            vts.OnCompleted(continuation, state, token, flags);
        }

        public ValueTask<Result<BitFieldData>> GetResult()
        {
            return new ValueTask<Result<BitFieldData>>(this, Version);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            FileDecision.Dispose();
        }
    }
}