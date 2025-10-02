

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 格式相关的扩展方法和工具类。
    /// </summary>
    internal unsafe static class FFmpegFormatExpansion
    {
        /// <summary>
        /// 将以 null 结尾的 UTF-8 编码的字节指针转换为字符串
        /// </summary>
        /// <param name="ptr">指向以 null 结尾的 UTF-8 字节序列的指针</param>
        /// <returns>转换后的字符串，如果指针为 null 则返回空字符串</returns>
        /// <remarks>
        /// 此方法用于处理来自非托管代码的以 null 结尾的 UTF-8 字符串
        /// 由于使用了 unsafe 上下文，调用者需要确保指针的有效性
        /// </remarks>
        public static string PtrToString(byte* ptr)
        {
            // 处理空指针情况，直接返回空字符串
            if (ptr == null) return string.Empty;

            // 计算字符串长度（直到遇到 null 终止符）
            int len = 0;
            while (ptr[len] != 0) len++;

            // 使用 UTF-8 编码将字节序列转换为字符串
            return System.Text.Encoding.UTF8.GetString(ptr, len);
        }
    }
}
