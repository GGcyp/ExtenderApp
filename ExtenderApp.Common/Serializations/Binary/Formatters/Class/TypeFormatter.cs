using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 类型格式化器类，继承自 <see cref="ResolverFormatter{T}"/>。
    /// </summary>
    internal class TypeFormatter : ResolverFormatter<Type>
    {
        protected readonly IBinaryFormatter<string> _string;
        public override int DefaultLength => _string.DefaultLength;
        public TypeFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _string = resolver.GetFormatter<string>();
        }

        public override Type Deserialize(ref ByteBuffer buffer)
        {
            var typeName = _string.Deserialize(ref buffer);
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }
            try
            {
                return Type.GetType(typeName, throwOnError: true);
            }
            catch (Exception)
            {
                // 如果类型无法解析，返回 null
                return null;
            }
        }

        public override void Serialize(ref ByteBuffer buffer, Type value)
        {
            _string.Serialize(ref buffer, value?.AssemblyQualifiedName ?? string.Empty);
        }

        public override long GetLength(Type value)
        {
            if (value == null)
            {
                return 1;
            }

            return _string.GetLength(value.AssemblyQualifiedName);
        }
    }
}
