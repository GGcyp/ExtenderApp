using System.Text;
using ExtenderApp.Abstract.Options;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract.Networks
{
    /// <summary>
    /// 表示 HTTP 头集合的类型安全容器。
    /// - 在构造时预先注册常用的头部选项标识，以便后续直接设置/读取而无需按次注册。
    /// </summary>
    public class HttpHeader : OptionsObject
    {
        private const string ColonString = ": ";

        /// <summary>
        /// 将当前 HttpHeader 中已注册的可见选项序列化为 HTTP 头部文本（每行以 CRLF 结尾），并追加到提供的 StringBuilder 中。
        /// - 对于 <see cref="DateTimeOffset"/> 值使用 RFC1123 格式化（"r");
        /// - 对于多值项（ <see cref="ValueOrList{string}"/>) 若 <paramref name="combineValues"/> 为 <c>false</c> 则每个值单独写一行（适用于 Set-Cookie）；否则使用逗号合并。
        /// </summary>
        /// <param name="headers">HttpHeader 实例。</param>
        /// <param name="sb">目标 StringBuilder。</param>
        /// <param name="combineValues">指示是否把多值头合并为单行（用 ", ")；若为 false 则针对集合类型写多行。</param>
        public void BuildHeaderBlock(StringBuilder sb, bool combineValues = true)
        {
            if (sb is null) throw new ArgumentNullException(nameof(sb));

            foreach (var (identifier, optionValue) in RegisteredOptionsIdentifier)
            {
                if (identifier is null || optionValue is null)
                    continue;

                string name = identifier.Name;
                if (string.IsNullOrEmpty(name))
                    continue;

                // Special handling for typed values
                switch (optionValue)
                {
                    case OptionValue<DateTimeOffset> dt:
                        var dto = dt.Value;
                        sb.Append(name);
                        sb.Append(ColonString);
                        sb.Append(dto.ToString("r")); // RFC1123
                        sb.Append(HttpConstants.CRLF);
                        break;

                    case OptionValue<ValueOrList<string>> multi:
                        ValueOrList<string> list = multi.Value;
                        if (list.Count == 0)
                            break;

                        if (!combineValues && ReferenceEquals(identifier, HttpHeaderOptions.SetCookieIdentifier))
                        {
                            // Set-Cookie must be on separate lines, write each entry
                            foreach (var v in list)
                            {
                                sb.Append(name);
                                sb.Append(ColonString);
                                sb.Append(v ?? string.Empty);
                                sb.Append(HttpConstants.CRLF);
                            }
                        }
                        else if (!combineValues)
                        {
                            // write each value on its own line
                            foreach (var v in list)
                            {
                                sb.Append(name);
                                sb.Append(ColonString);
                                sb.Append(v ?? string.Empty);
                                sb.Append(HttpConstants.CRLF);
                            }
                        }
                        else
                        {
                            sb.Append(name);
                            sb.Append(ColonString);
                            bool first = true;
                            foreach (var v in multi.Value)
                            {
                                if (!first)
                                    sb.Append(',');

                                sb.Append(v ?? string.Empty);
                                first = false;
                            }
                            sb.Append(HttpConstants.CRLF);
                        }
                        break;

                    default:
                        var text = optionValue.ValueToString();
                        if (string.IsNullOrEmpty(text))
                            break;
                        sb.Append(name);
                        sb.Append(ColonString);
                        sb.Append(text);
                        sb.Append(HttpConstants.CRLF);
                        break;
                }
            }
        }

        /// <summary>
        /// 将当前 HttpHeader 中已注册的可见选项序列化为 HTTP 头部字节并写入到指定的缓冲区（避免中间字符串分配）。
        /// - 对于 <see cref="DateTimeOffset"/> 值使用 RFC1123 格式化（"r");
        /// - 对于多值项（ <see cref="ValueOrList{string}"/>) 若 <paramref name="combineValues"/> 为 <c>false</c> 则每个值单独写一行（适用于 Set-Cookie）；否则使用逗号合并。
        /// </summary>
        /// <param name="buffer">目标字节缓冲。</param>
        /// <param name="combineValues">指示是否把多值头合并为单行（用 ", ")；若为 false 则针对集合类型写多行。</param>
        /// <param name="encoding">写入时使用的编码（默认 ASCII）。</param>
        public void BuildHeaderBlock(AbstractBuffer<byte> buffer, bool combineValues = true, Encoding? encoding = null)
        {
            if (buffer is null) throw new ArgumentNullException(nameof(buffer));
            encoding ??= Encoding.ASCII;

            foreach (var (identifier, optionValue) in RegisteredOptionsIdentifier)
            {
                if (identifier is null || optionValue is null)
                    continue;

                string name = identifier.Name;
                if (string.IsNullOrEmpty(name))
                    continue;

                switch (optionValue)
                {
                    case OptionValue<DateTimeOffset> dt:
                        var dto = dt.Value;
                        buffer.Write(name, encoding);
                        buffer.Write(ColonString, encoding);
                        buffer.Write(dto.ToString("r"), encoding);
                        buffer.Write(HttpConstants.NextLine, encoding);
                        break;

                    case OptionValue<ValueOrList<string>> multi:
                        ValueOrList<string> list = multi.Value;
                        if (list.Count == 0)
                            break;

                        if (ReferenceEquals(identifier, HttpHeaderOptions.SetCookieIdentifier))
                        {
                            foreach (var v in list)
                            {
                                buffer.Write(name, encoding);
                                buffer.Write(ColonString, encoding);
                                buffer.Write(v ?? string.Empty, encoding);
                                buffer.Write(HttpConstants.NextLine, encoding);
                            }
                        }
                        else if (!combineValues)
                        {
                            foreach (var v in list)
                            {
                                buffer.Write(name, encoding);
                                buffer.Write(ColonString, encoding);
                                buffer.Write(v ?? string.Empty, encoding);
                                buffer.Write(HttpConstants.NextLine, encoding);
                            }
                        }
                        else
                        {
                            buffer.Write(name, encoding);
                            buffer.Write(ColonString, encoding);
                            bool first = true;
                            foreach (var v in list)
                            {
                                if (!first)
                                    buffer.Write(",", encoding);

                                buffer.Write(v ?? string.Empty, encoding);
                                first = false;
                            }
                            buffer.Write(HttpConstants.NextLine, encoding);
                        }
                        break;

                    default:
                        var text = optionValue.ValueToString();
                        if (string.IsNullOrEmpty(text))
                            break;
                        buffer.Write(name, encoding);
                        buffer.Write(ColonString, encoding);
                        buffer.Write(text, encoding);
                        buffer.Write(HttpConstants.NextLine, encoding);
                        break;
                }
            }
        }

        public void ApplyOption(string name, string value)
        {
            if (!HttpHeaderOptions.TryGetOptionIdentifier(name, out var optId) || optId is null)
                return;

            switch (optId)
            {
                case OptionIdentifier<string> stringIdentifier:
                    if (!TrySetOptionValue(stringIdentifier, value))
                        RegisterOption(stringIdentifier, value);
                    break;

                case OptionIdentifier<int> intIdentifier:
                    if (int.TryParse(value, out var iv))
                    {
                        if (!TrySetOptionValue(intIdentifier, iv))
                            RegisterOption(intIdentifier, iv);
                    }
                    break;

                case OptionIdentifier<long> longIdentifier:
                    if (long.TryParse(value, out var lv))
                    {
                        if (!TrySetOptionValue(longIdentifier, lv))
                            RegisterOption(longIdentifier, lv);
                    }
                    break;

                case OptionIdentifier<DateTimeOffset> dtoIdentifier:
                    if (DateTimeOffset.TryParse(value, out var dto))
                    {
                        if (!TrySetOptionValue(dtoIdentifier, dto))
                            RegisterOption(dtoIdentifier, dto);
                    }
                    break;

                case OptionIdentifier<ValueOrList<string>> voListId:
                    var trimmed = value?.Trim() ?? string.Empty;

                    if (ReferenceEquals(voListId, HttpHeaderOptions.SetCookieIdentifier))
                    {
                        // always treat entire value as one cookie entry
                        if (TryGetOptionValue(voListId, out var existing))
                            existing.Add(trimmed);
                        else
                            RegisterOption(voListId, new ValueOrList<string> { trimmed });
                    }
                    else
                    {
                        var items = trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (TryGetOptionValue(voListId, out var existing))
                        {
                            foreach (var it in items) existing.Add(it);
                        }
                        else
                        {
                            var list = new ValueOrList<string>();
                            list.AddRange(items);
                            RegisterOption(voListId, list);
                        }
                    }
                    break;
            }
        }
    }
}