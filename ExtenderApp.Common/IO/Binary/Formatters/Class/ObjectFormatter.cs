using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatters.Class
{
    /// <summary>
    /// 内部类 ObjectFormatter，继承自 ExtenderFormatter<object> 类。
    /// 用于格式化对象类型的数据。
    /// 如果没有指定类型，则写入空数据。
    /// </summary>
    internal class ObjectFormatter : ExtenderFormatter<object>
    {
        private readonly ConcurrentDictionary<Type, SerializeMethod> _serializeMethods = new();
        private readonly MethodInfo getMethod = typeof(IBinaryFormatterResolver).GetMethod("GetFormatter")!;

        private delegate void SerializeMethod(object formatter, ref ExtenderBinaryWriter writer, object value);
        public override int DefaultLength => 0;
        private readonly IBinaryFormatterStore _store;
        private readonly BinaryFormatterCreator _creator;
        private readonly IBinaryFormatterResolver _resolver;

        public ObjectFormatter(IBinaryFormatterStore store, BinaryFormatterCreator creator, IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
            _store = store;
            _creator = creator;
            _resolver = resolver;
        }

        public override object Deserialize(ref ExtenderBinaryReader reader)
        {
            _binaryReaderConvert.ReadMapHeader(ref reader);
            return null;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, object value)
        {
            if (value is null)
            {
                _binaryWriterConvert.WriteNil(ref writer);
                return;
            }

            Type type = value.GetType();
            if (type == typeof(object))
            {
                _binaryWriterConvert.WriteNil(ref writer);
                return;
            }

            // 如果没有找到对应的格式化器，则创建一个新的格式化器
            if (!_store.TryGetSingleFormatterType(type, out Type formatterType))
            {
                formatterType = _creator.CreatFormatter(type);
                if (formatterType == null)
                {
                    _binaryWriterConvert.WriteNil(ref writer);
                    return;
                }
            }

            if(!_serializeMethods.TryGetValue(type, out SerializeMethod serializeMethod))
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                formatterType = typeof(IBinaryFormatter<>).MakeGenericType(type);
                ParameterExpression param0 = Expression.Parameter(typeof(object), "formatter");
                ParameterExpression param1 = Expression.Parameter(typeof(ExtenderBinaryWriter).MakeByRefType(), "writer");
                ParameterExpression param2 = Expression.Parameter(typeof(object), "value");
                MethodInfo serializeMethodInfo = formatterType.GetRuntimeMethod("Serialize", new[] { typeof(ExtenderBinaryWriter).MakeByRefType(), type })!;
                MethodCallExpression body = Expression.Call(
                    Expression.Convert(param0, formatterType),
                    serializeMethodInfo,
                    param1,
                    typeInfo.IsValueType ? Expression.Unbox(param2, type) : Expression.Convert(param2, type));

                serializeMethod = Expression.Lambda<SerializeMethod>(body, param0, param1, param2).Compile();
                _serializeMethods.GetOrAdd(type, _ => serializeMethod);
            }

            var formatter = getMethod.MakeGenericMethod(type).Invoke(_resolver, null);
            if(formatter == null)
            {
                _binaryWriterConvert.WriteNil(ref writer);
                return;
            }

            serializeMethod(formatter, ref writer, value);
        }
    }
}
