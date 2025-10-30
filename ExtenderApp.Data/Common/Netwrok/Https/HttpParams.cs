using System.Text;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示 HTTP 参数集合（键和值均为字符串）。
    /// 继承自 <see cref="Dictionary{TKey, TValue}"/>，用于构造查询字符串或 application/x-www-form-urlencoded 表单内容。
    /// </summary>
    public class HttpParams : Dictionary<string, string>
    {
        /// <summary>
        /// 将当前参数集合按查询字符串格式追加到指定的 <see cref="StringBuilder"/>。
        /// </summary>
        /// <param name="sb">目标 <see cref="StringBuilder"/>，必须非空。</param>
        /// <param name="isAppend">
        /// 指示首个连接符的类型：
        /// - 若为 <c>true</c>，在开始处追加 '&'（适用于已经有其它查询内容，需要追加更多参数的场景）；
        /// - 若为 <c>false</c>，在开始处追加 '?'（适用于直接在路径后开始查询字符串的场景）。
        /// </param>
        /// <remarks>
        /// - 每个键和值都会使用 <see cref="Uri.EscapeDataString(string)"/> 进行百分号编码（空格编码为 %20）。
        /// - 输出模式示例（假设 isAppend==true）：&a=1&b=two%20words
        /// - 方法实现会在遍历所有键值对后移除末尾多余的分隔符（当前实现通过 sb.Remove 移除）。
        /// - 若当前集合为空，调用本方法仍会向 sb 追加 isAppend 指定的首字符，随后尝试移除末尾字符 —— 请在调用前确保集合非空以避免异常。
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="sb"/> 为 <c>null</c> 时抛出。</exception>
        public void BuildQuery(StringBuilder sb, bool isAppend = true)
        {
            ArgumentNullException.ThrowIfNull(sb);

            static string Encode(string? s)
            {
                if (string.IsNullOrEmpty(s)) return string.Empty;
                return Uri.EscapeDataString(s);
            }

            if (isAppend)
            {
                sb.Append('&');
            }
            else
            {
                sb.Append('?');
            }

            foreach (var kv in this)
            {
                sb.Append(Encode(kv.Key));
                sb.Append('=');
                sb.Append(Encode(kv.Value));
                sb.Append('&');
            }
            sb.Remove(sb.Length - 2, 1);// 移除最后一个多余的 '&'
        }

        /// <summary>
        /// 将当前参数集合以 application/x-www-form-urlencoded（表单）格式追加到指定的 <see cref="StringBuilder"/>。
        /// </summary>
        /// <param name="sb">目标 <see cref="StringBuilder"/>，必须非空。</param>
        /// <param name="isAppend">
        /// 指示首个连接符的类型：
        /// - 若为 <c>true</c>，在开始处追加 '&'（适用于已经有其它表单/查询内容，需要追加更多参数的场景）；
        /// - 若为 <c>false</c>，在开始处追加 '?'（适用于直接在路径后开始查询字符串的场景，但用于表单时一般传 <c>true</c>）。
        /// </param>
        /// <remarks>
        /// - 对键和值的编码规则：先调用 <see cref="Uri.EscapeDataString(string)"/>（UTF-8 百分号转义），
        ///   然后把 "%20" 替换为 '+'，以符合 application/x-www-form-urlencoded 的空格编码习惯。
        /// - 输出示例（假设 isAppend==true）：&a=1&b=two+words
        /// - 方法实现会在遍历所有键值对后移除末尾多余的分隔符（当前实现通过 sb.Remove 移除）。
        /// - 若当前集合为空，调用本方法仍会向 sb 追加 isAppend 指定的首字符，随后尝试移除末尾字符 —— 请在调用前确保集合非空以避免异常。
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="sb"/> 为 <c>null</c> 时抛出。</exception>
        public void BuildFormUrlEncoded(StringBuilder sb, bool isAppend = true)
        {
            ArgumentNullException.ThrowIfNull(sb);

            static string EncodeForForm(string? s)
            {
                if (string.IsNullOrEmpty(s)) return string.Empty;
                // 与 FormUrlEncodedContent 的实现一致：先百分号转义（UTF-8），再把 %20 替换为 '+'
                return Uri.EscapeDataString(s).Replace("%20", "+");
            }

            if (isAppend)
            {
                sb.Append('&');
            }
            else
            {
                sb.Append('?');
            }

            foreach (var kv in this)
            {
                sb.Append('&');
                sb.Append(EncodeForForm(kv.Key));
                sb.Append('=');
                sb.Append(EncodeForForm(kv.Value));
            }
            sb.Remove(sb.Length - 2, 1);// 移除最后一个多余的 '&'
        }

        /// <summary>
        /// 返回当前参数集合对应的查询字符串（不额外添加前导 '?'，但注意：当前实现的 <see cref="BuildQuery(StringBuilder,bool)"/>
        /// 默认会先追加一个 '&'，因此调用本方法得到的字符串可能以 '&' 开头——调用方若需要去掉前导符请自行 Trim）。
        /// </summary>
        /// <returns>形如 "a=1&amp;b=two%20words" 的字符串（可能以 '&' 开头，集合为空时返回空字符串）。</returns>
        public string ToQueryString()
        {
            if (Count == 0) return string.Empty;
            var sb = new StringBuilder();
            BuildQuery(sb);
            return sb.ToString();
        }

        /// <summary>
        /// 返回当前参数集合对应的 application/x-www-form-urlencoded 字符串（用于请求体）。
        /// </summary>
        /// <returns>形如 "a=1&amp;b=two+words" 的字符串（可能以 '&' 开头，集合为空时返回空字符串）。</returns>
        public string ToFormUrlEncodedString()
        {
            if (Count == 0) return string.Empty;
            var sb = new StringBuilder();
            BuildFormUrlEncoded(sb);
            return sb.ToString();
        }
    }
}