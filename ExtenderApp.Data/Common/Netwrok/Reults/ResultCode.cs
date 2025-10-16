

namespace ExtenderApp.Data
{
    // <summary>
    /// 结果类型
    /// </summary>
    public enum ResultCode : byte
    {
        /// <summary>
        /// 默认，表示没有特定的结果状态
        /// </summary>
        Default = 0,

        /// <summary>
        /// 成功
        /// </summary>
        Success,

        /// <summary>
        /// 错误，程度较重的错误，但不影响系统的运行
        /// </summary>
        Error,

        /// <summary>
        /// 异常，程度较重的错误，可能是由于系统异常或其他不可恢复的原因导致的
        /// </summary>
        Exception,

        /// <summary>
        /// 失败，程度较轻的错误，可能是由于参数错误或其他可恢复的原因导致的
        /// </summary>
        Failure,

        /// <summary>
        /// 操作超时
        /// </summary>
        Overtime,

        /// <summary>
        /// 操作取消
        /// </summary>
        Canceled,

        /// <summary>
        /// 操作对象已被释放
        /// </summary>
        Disposed,
    }
}
