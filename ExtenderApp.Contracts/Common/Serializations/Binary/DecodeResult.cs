

namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 解码结果枚举
    /// </summary>
    public enum DecodeResult : byte
    {
        /// <summary>
        /// 解码成功
        /// </summary>
        Success,

        /// <summary>
        /// 令牌不匹配
        /// </summary>
        TokenMismatch,

        /// <summary>
        /// 缓冲区为空
        /// </summary>
        EmptyBuffer,

        /// <summary>
        /// 缓冲区不足
        /// </summary>
        InsufficientBuffer,
    }
}
