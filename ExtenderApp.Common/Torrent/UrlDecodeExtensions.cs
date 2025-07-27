

namespace ExtenderApp.Common.Torrent
{
    /// <summary>
    /// UrlDecodeExtensions 类提供 URL 解码功能。
    /// </summary>
    public static class UrlDecodeExtensions
    {
        /// <summary>
        /// 将 URL 编码的字符串解码为普通字符串。
        /// </summary>
        /// <param name="value">待解码的 URL 编码字符串。</param>
        /// <returns>解码后的字符串。如果输入为 null，则返回 null。</returns>
        public static string? UrlDecode(this string? value)
        {
            if (value == null)
                return null;
            // Decode the URL-encoded string
            return System.Web.HttpUtility.UrlDecode(value);
        }
    }
}
