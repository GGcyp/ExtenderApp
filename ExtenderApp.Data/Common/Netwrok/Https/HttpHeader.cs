using System.Text;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 简单的请求头集合，支持单值或多值（基于 <see cref="ValueOrList<string>"/>）。
    /// 默认使用不区分大小写的头名比较（StringComparer.OrdinalIgnoreCase）。
    /// </summary>
    public class HttpHeader : Dictionary<string, ValueOrList<string>>
    {
        public HttpHeader() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// 添加一个头值（如果已存在则追加）。
        /// </summary>
        public void AddValue(string name, string value)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(value);

            if (!TryGetValue(name, out var list) || list is null)
            {
                list = new(1);
                this[name] = list;
            }
            list.Add(value);
        }

        /// <summary>
        /// 添加多个头值（按顺序追加）。
        /// </summary>
        public void AddValues(string name, IEnumerable<string> values)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(values);

            foreach (var v in values)
            {
                AddValue(name, v ?? string.Empty);
            }
        }

        /// <summary>
        /// 用单个值替换已有值（如果存在则删除旧值并只保留新值）。
        /// </summary>
        public void SetValue(string name, string value)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(value);

            var list = new ValueOrList<string>(1);
            list.Add(value);
            this[name] = list;
        }

        /// <summary>
        /// 尝试获取头的所有值（返回数组副本）。不存在时返回 false，values 为 Empty。
        /// </summary>
        public bool TryGetValues(string name, out ValueOrList<string> values)
        {
            ArgumentNullException.ThrowIfNull(name);

            if (TryGetValue(name, out var list) && list is not null && list.Count > 0)
            {
                values = list;
                return true;
            }
            values = ValueOrList<string>.Empty;
            return false;
        }

        /// <summary>
        /// 构建头部文本块到 StringBuilder。
        /// 如果 combineValues 为 true，则将同名多值合并为一行，值之间用 ", " 分隔；否则每个值单独一行（多行表示）。
        /// 不会在末尾追加额外的空行（调用方负责追加 "\r\n" 分隔请求头与主体）。
        /// </summary>
        public void BuildHeaderBlock(StringBuilder sb, bool combineValues = false)
        {
            ArgumentNullException.ThrowIfNull(sb);

            foreach (var kv in this)
            {
                if (kv.Value is null || kv.Value.Count == 0)
                {
                    sb.Append(kv.Key);
                    sb.Append(": ");
                    sb.Append("\r\n");
                    continue;
                }

                if (combineValues)
                {
                    var joined = string.Join(", ", kv.Value);
                    sb.Append(kv.Key);
                    sb.Append(": ");
                    sb.Append(joined);
                    sb.Append("\r\n");
                }
                else
                {
                    foreach (var v in kv.Value)
                    {
                        sb.Append(kv.Key);
                        sb.Append(": ");
                        sb.Append(v);
                        sb.Append("\r\n");
                    }
                }
            }
        }

        /// <summary>
        /// 将本集合的头追加到目标字典，每个值作为单独条目追加到目标的 List 中。
        /// </summary>
        public void ApplyTo(IDictionary<string, List<string>> target)
        {
            ArgumentNullException.ThrowIfNull(target);

            foreach (var kv in this)
            {
                if (string.IsNullOrEmpty(kv.Key))
                    continue;

                if (!target.TryGetValue(kv.Key, out var list))
                {
                    list = new List<string>();
                    target[kv.Key] = list;
                }

                if (kv.Value is null || kv.Value.Count == 0)
                {
                    list.Add(string.Empty);
                }
                else
                {
                    foreach (var v in kv.Value)
                    {
                        if (v is not null)
                            list.Add(v);
                    }
                }
            }
        }

        /// <summary>
        /// 删除指定头并返回是否存在。
        /// </summary>
        public new bool Remove(string name)
        {
            ArgumentNullException.ThrowIfNull(name);
            return base.Remove(name);
        }

        /// <summary>
        /// 判断是否存在指定头（名称不区分大小写）。
        /// </summary>
        public bool ContainsHeader(string name)
        {
            ArgumentNullException.ThrowIfNull(name);
            return ContainsKey(name);
        }
    }
}