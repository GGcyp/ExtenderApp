using System.Text;

namespace ExtenderApp.Data
{
    /// <summary>
    /// HTTP 参数集合
    /// </summary>
    public class HttpParams : Dictionary<string, string>
    {
        /// <summary>
        /// 将参数以查询字符串格式追加到 sb（key 和 value 使用 Uri.EscapeDataString，空格为 %20）。
        /// 结果示例：&a=1&b=two%20words
        /// </summary>
        /// <param name="sb">目标 StringBuilder（会在每个键值对前追加 '&'）</param>
        public void BuildQuery(StringBuilder sb)
        {
            ArgumentNullException.ThrowIfNull(sb);

            static string Encode(string? s)
            {
                if (string.IsNullOrEmpty(s)) return string.Empty;
                return Uri.EscapeDataString(s);
            }

            foreach (var kv in this)
            {
                sb.Append('&');
                sb.Append(Encode(kv.Key));
                sb.Append('=');
                sb.Append(Encode(kv.Value));
            }
        }

        /// <summary>
        /// 将参数以 application/x-www-form-urlencoded（表单）格式追加到 sb（空格编码为 '+'）。
        /// 结果示例：&a=1&b=two+words
        /// </summary>
        /// <param name="sb">目标 StringBuilder（会在每个键值对前追加 '&'）</param>
        public void BuildFormUrlEncoded(StringBuilder sb)
        {
            ArgumentNullException.ThrowIfNull(sb);

            static string EncodeForForm(string? s)
            {
                if (string.IsNullOrEmpty(s)) return string.Empty;
                // 与 FormUrlEncodedContent 的实现一致：先百分号转义（UTF-8），再把 %20 替换为 '+'
                return Uri.EscapeDataString(s).Replace("%20", "+");
            }

            foreach (var kv in this)
            {
                sb.Append('&');
                sb.Append(EncodeForForm(kv.Key));
                sb.Append('=');
                sb.Append(EncodeForForm(kv.Value));
            }
        }

        /// <summary>
        /// 便捷方法：返回不带前导 '?' 或 '&' 的查询字符串（用于 URL）。
        /// 示例： "a=1&b=two%20words"
        /// </summary>
        public string ToQueryString()
        {
            if (Count == 0) return string.Empty;
            var sb = new StringBuilder();
            BuildQuery(sb);
            return sb.ToString();
        }

        /// <summary>
        /// 便捷方法：返回 application/x-www-form-urlencoded 格式的字符串（用于请求体）。
        /// 空格编码为 '+'。
        /// 示例： "a=1&b=two+words"
        /// </summary>
        public string ToFormUrlEncodedString()
        {
            if (Count == 0) return string.Empty;
            var sb = new StringBuilder();
            BuildFormUrlEncoded(sb);
            return sb.ToString();
        }
    }
}