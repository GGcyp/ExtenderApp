using System.Reflection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatter
{
    internal class DefaultObjectFormatter<T> : ResolverFormatter<T> where T : class, new()
    {
        private static MethodInfo serializeMethod = typeof(IBinaryFormatter<>).GetMethod("Serialize", BindingFlags.Instance | BindingFlags.NonPublic)!;
        private static MethodInfo deserializeMethod = typeof(IBinaryFormatter<>).GetMethod("Deserialize", BindingFlags.Instance | BindingFlags.NonPublic)!;
        private static MethodInfo getMethod = typeof(ResolverFormatter<>).GetMethod("GetFormatter", BindingFlags.Instance | BindingFlags.NonPublic)!;

        public override int Count
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _formatters.Count; i++)
                {
                    count += _formatters[i].Item1.Count;
                }
                return count;
            }
        }

        private readonly List<(IBinaryFormatter, PropertyInfo, MethodInfo, MethodInfo)> _formatters;

        private readonly object[] _values;

        public DefaultObjectFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            _formatters = new(properties.Length);
            _values = new object[2];

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                var formatter = GetFormatterForType(getMethod, property.PropertyType);

                if (formatter == null)
                {
                    throw new InvalidOperationException($"不能找到对应的二进制转换器 {property.PropertyType}");
                }

                MethodInfo serialize = serializeMethod.MakeGenericMethod(property.PropertyType);
                MethodInfo deserialize = deserializeMethod.MakeGenericMethod(property.PropertyType);
                _formatters.Add((formatter, property, serialize, deserialize));
            }
        }

        private IBinaryFormatter? GetFormatterForType(MethodInfo method, Type type)
        {
            MethodInfo genericMethod = method.MakeGenericMethod(type)!;
            return (IBinaryFormatter)genericMethod.Invoke(this, null);
        }

        public override T Deserialize(ref ExtenderBinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, T value)
        {
            for (int i = 0; i < _formatters.Count; i++)
            {
                var formatter = _formatters[i];
                object? propertyValue = formatter.Item2.GetValue(value);
                //formatter.Item3.Invoke(formatter.Item1, new object[] { ref writer, propertyValue });
            }
        }
    }
}
