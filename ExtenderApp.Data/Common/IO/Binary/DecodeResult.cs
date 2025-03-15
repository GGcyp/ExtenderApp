using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 解码结果枚举
    /// </summary>
    public enum DecodeResult
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
