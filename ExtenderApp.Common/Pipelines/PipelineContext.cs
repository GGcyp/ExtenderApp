using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Pipelines
{
    /// <summary>
    /// 管线上下文，提供管线执行状态和错误信息。
    /// </summary>
    public abstract class PipelineContext : DisposableObject, IPipelineContext
    {
        public bool IsTerminated { get; set; }
        public Exception? Error { get; set; }

        public void Reset()
        {
            IsTerminated = false;
            Error = null;
            ProtectedReset();
        }

        protected abstract void ProtectedReset();
    }
}
