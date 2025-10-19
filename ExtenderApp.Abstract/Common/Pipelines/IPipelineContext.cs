

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 管道上下文：承载数据和状态，在中间件间传递
    /// </summary>
    public interface IPipelineContext
    {
        /// <summary>
        /// 是否终止管道（中间件可设置为true，跳过后续处理）
        /// </summary>
        public bool IsTerminated { get; set; }

        /// <summary>
        /// 错误信息（中间件抛出异常时存储）
        /// </summary>
        public Exception? Error { get; set; }
    }
}
