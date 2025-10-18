namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示通用结果的状态码。用于统一描述一次操作的最终状态。
    /// </summary>
    public enum ResultCode : byte
    {
        /// <summary>
        /// 默认值（未设置/未开始）。不代表具体结果，仅用于初始化。
        /// </summary>
        Default = 0,

        /// <summary>
        /// 操作成功完成。
        /// </summary>
        Success,

        /// <summary>
        /// 业务或可预期的错误，例如参数无效、权限不足、校验失败等。
        /// </summary>
        Error,

        /// <summary>
        /// 执行过程中发生异常导致失败（通常伴随异常对象）。
        /// </summary>
        Exception,

        /// <summary>
        /// 一般性失败，非异常引起，但未达到预期结果。
        /// </summary>
        Failed,

        /// <summary>
        /// 超时未完成（Timeout）。
        /// </summary>
        Overtime,

        /// <summary>
        /// 操作被取消（通常由调用方通过取消令牌触发）。
        /// </summary>
        Canceled,

        /// <summary>
        /// 相关对象或上下文已释放（Disposed），无法执行或继续操作。
        /// </summary>
        Disposed,
    }
}
