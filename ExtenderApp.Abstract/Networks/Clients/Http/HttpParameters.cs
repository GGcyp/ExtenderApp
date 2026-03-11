using System.Text;
using ExtenderApp.Abstract.Options;
using ExtenderApp.Buffer;

namespace ExtenderApp.Abstract.Networks
{
    /// <summary>
    /// 表示 HTTP 参数集合（键和值均为字符串）。
    /// 继承自 <see cref="OptionsObject"/>, 用于构造查询字符串或 application/x-www-form-urlencoded 表单内容。
    /// </summary>
    public class HttpParameters : OptionsObject
    {
        /// <summary>
        /// 构造键=值 的参数对字符串，不包含前导 '?' 或 '&'，参数之间以 '&' 分隔。
        /// </summary>
        /// <returns>例如: "a=1&b=two%20words"，若无参数返回空字符串。</returns>
        private void BuildPairsString(StringBuilder sb, Func<string?, string?> encodeFunc)
        {
            bool first = true;
            foreach (var (identifier, optionValue) in RegisteredOptionsIdentifier)
            {
                string key = identifier.Name;
                string value = optionValue.ValueToString();
                if (!first)
                    sb.Append('&');
                first = false;

                sb.Append(encodeFunc(key));
                sb.Append('=');
                sb.Append(encodeFunc(value));
            }
        }

        /// <summary>
        /// 将当前参数集合按查询字符串格式追加到指定的 <see cref="StringBuilder"/>。
        /// </summary>
        /// <param name="sb">目标 <see cref="StringBuilder"/>，必须非空。</param>
        /// <param name="isAppend">
        /// 指示首个连接符的类型：
        /// - 若为 <c>true</c>，在开始处追加 '&'（适用于已经有其它查询内容，需要追加更多参数的场景）；
        /// - 若为 <c>false</c>，在开始处追加 '?'（适用于直接在路径后开始查询字符串的场景）。
        /// </param>
        public void BuildQuery(StringBuilder sb, bool isAppend = true)
        {
            ArgumentNullException.ThrowIfNull(sb);

            static string Encode(string? s)
            {
                if (string.IsNullOrEmpty(s)) return string.Empty;
                return Uri.EscapeDataString(s);
            }

            sb.Append(isAppend ? '&' : '?');
            BuildPairsString(sb, Encode);
        }

        /// <summary>
        /// 将当前参数集合以 application/x-www-form-urlencoded（表单）格式追加到指定的 <see cref="StringBuilder"/>。
        /// </summary>
        /// <param name="sb">目标 <see cref="StringBuilder"/>，必须非空。</param>
        /// <param name="isAppend">指示首个连接符是 '&' 还是 '?'（参见 <see cref="BuildQuery"/>）。</param>
        public void BuildFormUrlEncoded(StringBuilder sb, bool isAppend = true)
        {
            ArgumentNullException.ThrowIfNull(sb);

            static string EncodeForForm(string? s)
            {
                if (string.IsNullOrEmpty(s)) return string.Empty;
                // 与 FormUrlEncodedContent 的实现一致：先百分号转义（UTF-8），再把 %20 替换为 '+'
                return Uri.EscapeDataString(s).Replace("%20", "+");
            }

            sb.Append(isAppend ? '&' : '?');
            BuildPairsString(sb, EncodeForForm);
        }

        /// <summary>
        /// 返回当前参数集合对应的查询字符串（不包含前导 '?'）。
        /// </summary>
        /// <returns>形如 "a=1&amp;b=two%20words" 的字符串（集合为空时返回空字符串）。</returns>
        public string ToQueryString()
        {
            if (OptionCount == 0)
                return string.Empty;

            StringBuilder sb = new();
            BuildPairsString(sb, static s => string.IsNullOrEmpty(s) ? string.Empty : Uri.EscapeDataString(s));
            return sb.ToString();
        }

        /// <summary>
        /// 返回当前参数集合对应的 application/x-www-form-urlencoded 字符串（不包含前导 '?'）。
        /// </summary>
        /// <returns>形如 "a=1&amp;b=two+words" 的字符串（集合为空时返回空字符串）。</returns>
        public string ToFormUrlEncodedString()
        {
            if (OptionCount == 0)
                return string.Empty;

            StringBuilder sb = new();
            BuildPairsString(sb, static s => string.IsNullOrEmpty(s) ? string.Empty : Uri.EscapeDataString(s).Replace("%20", "+"));
            return sb.ToString();
        }

        /// <summary>
        /// 将当前参数集合按查询字符串或表单格式写入指定的缓冲区。
        /// </summary>
        /// <param name="buffer">目标缓冲区，必须非空。</param>
        /// <param name="form">
        /// 指示格式类型：
        /// - 若为 <c>true</c>，则按表单格式写入（application/x-www-form-urlencoded）；
        /// - 若为 <c>false</c>，则按查询字符串格式写入。
        /// </param>
        /// <param name="encoding">
        /// 可选的编码器。
        /// - 若为 <c>null</c>，则使用 UTF-8 编码；
        /// - 否则，使用指定的编码器。
        /// </param>
        public void WriteQueryToBuffer(AbstractBuffer<byte> buffer, bool form = false, Encoding? encoding = null)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            encoding ??= Encoding.UTF8;

            if (OptionCount == 0)
                return;

            bool first = true;
            foreach (var (identifier, optionValue) in RegisteredOptionsIdentifier)
            {
                string key = identifier.Name ?? string.Empty;
                string value = optionValue?.ValueToString() ?? string.Empty;

                // Percent-encode (UTF-8 semantics) then for form replace %20 with '+'
                string encodedKey = Uri.EscapeDataString(key);
                string encodedValue = Uri.EscapeDataString(value);
                if (form)
                {
                    encodedKey = encodedKey.Replace("%20", "+");
                    encodedValue = encodedValue.Replace("%20", "+");
                }

                if (!first)
                {
                    // write '&'
                    buffer.Write((byte)'&');
                }
                first = false;

                // write key bytes
                if (!string.IsNullOrEmpty(encodedKey))
                {
                    buffer.Write(encodedKey, encoding);
                }

                // write '='
                buffer.Write((byte)'=');

                // write value bytes
                if (!string.IsNullOrEmpty(encodedValue))
                {
                    buffer.Write(encodedValue, encoding);
                }
            }
        }
    }
}