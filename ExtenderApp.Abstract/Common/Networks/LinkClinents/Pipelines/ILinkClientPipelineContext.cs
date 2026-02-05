namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 管线上下文接口，用于在管线处理器之间传递状态和数据。
    /// </summary>
    public interface ILinkClientPipelineContext : IDisposable
    {
        /// <summary>
        /// 获取一个值，该值指示管线执行是否已被终止。
        /// </summary>
        bool IsAborted { get; }

        /// <summary>
        /// 终止当前管线的后续处理流程。
        /// </summary>
        void Abort();
    }
}