namespace ExtenderApp.Data
{
    /// <summary>
    /// 管线上下文，提供管线执行状态和错误信息。
    /// </summary>
    public abstract class PipelineContext : DisposableObject
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
