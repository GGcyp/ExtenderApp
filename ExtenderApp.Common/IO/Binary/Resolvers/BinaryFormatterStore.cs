using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary
{
    internal class BinaryFormatterStore : ConcurrentDictionary<Type, BinaryFormatterDetails>, IBinaryFormatterStore
    {
        public void AddFormatter(Type type, Type typeFormatter, bool isVersionDataFormatter = false)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (typeFormatter == null)
                throw new ArgumentNullException(nameof(typeFormatter));

            if (TryGetValue(type, out BinaryFormatterDetails? details))
            {
                if (!typeFormatter.IsAssignableTo(typeof(IVersionDataFormatter)))
                {
                    throw new InvalidOperationException($"只有继承IVersionDataFormatter的转换器才能重复添加：{type.FullName} : {typeFormatter.FullName}");
                }

                if (details.FormatterTypes.Contains(typeFormatter))
                {
                    throw new Exception($"转换器类型已存在：{type.FullName} : {typeFormatter.FullName}");
                }
                details.FormatterTypes.Add(typeFormatter);
                return;
            }

            details = new BinaryFormatterDetails(type, isVersionDataFormatter);
            details.FormatterTypes.Add(typeFormatter);

            if (isVersionDataFormatter)
            {
                TryAdd(details.BinaryType, details);
                TryAdd(details.VersionDataBinaryType!, details);
            }
            else
            {
                TryAdd(type, details);
            }
        }

        public bool TryGetSingleFormatterType(Type type, out Type formatterType)
        {
            if (!TryGetValue(type, out var details))
            {
                throw new KeyNotFoundException($"未找到指定类型的格式化器 {type.FullName}");
            }

            if (details.IsVersionDataFormatter)
            {
                throw new InvalidCastException($"此类为版本格式化器 {type.FullName} 不能使用此函数");
            }

            formatterType = details.FormatterTypes[0];
            return true;
        }
    }
}
