

using System.Security.Cryptography;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 提供与 <see cref="SplitterDto"/> 相关的扩展方法。
    /// </summary>
    public static class SplitterDtoExtensions
    {
        /// <summary>
        /// 判断当前 <see cref="SplitterDto"/> 实例的 MD5 校验和是否与存储的 MD5 值一致。
        /// </summary>
        /// <param name="dto">当前的 <see cref="SplitterDto"/> 实例。</param>
        /// <returns>如果 MD5 校验和一致则返回 true，否则返回 false。</returns>
        public static bool CompliantMD5(this SplitterDto dto)
        {
            if (string.IsNullOrEmpty(dto.MD5) || dto.Bytes == null)
                return false;

            var currentBytesMD5 = MD5Handle.GetMD5Hash(dto.Bytes);
            return currentBytesMD5 == dto.MD5;
        }
    }
}
