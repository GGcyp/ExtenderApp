using System.Linq.Expressions;
using System.Reflection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 默认的对象格式化器类，继承自 <see cref="ResolverFormatter{T}"/> 类。
    /// </summary>
    /// <typeparam name="T">要格式化的对象的类型。</typeparam>
    internal class DefaultObjectFormatter<T> : IBinaryFormatter<T>
    {
        private delegate void SerializeMethod(ref ByteBuffer buffer, T value);
        private delegate T DeserializeMethod(ref ByteBuffer buffer);

        /// <summary>
        /// 获取对象长度的方法的委托类型。
        /// </summary>
        /// <param name="value">要获取长度的对象。</param>
        /// <returns>对象的长度。</returns>
        internal delegate long GetLengthMethod(T value);

        /// <summary>
        /// 序列化方法。
        /// </summary>
        private readonly SerializeMethod _serializeMethod;

        /// <summary>
        /// 反序列化方法。
        /// </summary>
        private readonly DeserializeMethod _deserializeMethod;

        /// <summary>
        /// 获取对象长度的方法。
        /// </summary>
        private readonly GetLengthMethod _getLengthMethod;

        private readonly int _length;
        public int DefaultLength => _length;

        public DefaultObjectFormatter(DefaultObjectStore store)
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);

            var valueParameter = Expression.Parameter(type, "value");

            //为每个属性创建系列化和反序列化委托
            var serializes = new Expression[properties.Length];
            var deserializes = new MemberBinding[properties.Length];
            Expression getLengthExpression = Expression.Constant((long)0);
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                MemberExpression member = Expression.Property(valueParameter, property);
                store.GetFormatterMethodInfo(property.PropertyType, out var serializeMethodInfo, out var deserializeMethodInfo, out var getLengthMethodInfo, out var formatter);
                serializes[i] = store.CreatePropertySerializeExpression(member, serializeMethodInfo, formatter);
                deserializes[i] = store.CreatePropertyDeserializeExpression(property, deserializeMethodInfo, formatter);
                getLengthExpression = store.CreateGetLengthExpression(member, getLengthMethodInfo, formatter, getLengthExpression);
                _length += formatter.DefaultLength;
            }

            //获取序列化委托
            var block = Expression.Block(serializes);
            var serialzeLambda = Expression.Lambda<SerializeMethod>(block, store.serializeParameter, valueParameter);
            _serializeMethod = serialzeLambda.Compile();

            //获取反序列化委托
            var newObject = Expression.New(type);
            var memberInit = Expression.MemberInit(newObject, deserializes);
            var deserializeLambda = Expression.Lambda<DeserializeMethod>(memberInit, store.deserializeParameter);
            _deserializeMethod = deserializeLambda.Compile();

            //获取获取长度委托
            var getLengthLambda = Expression.Lambda<GetLengthMethod>(getLengthExpression, valueParameter);
            _getLengthMethod = getLengthLambda.Compile();
        }

        public T Deserialize(ref ByteBuffer buffer)
        {
            return _deserializeMethod(ref buffer);
        }

        public void Serialize(ref ByteBuffer buffer, T value)
        {
            _serializeMethod(ref buffer, value);
        }

        public long GetLength(T value)
        {
            if (value == null)
                return 1;
            return _getLengthMethod.Invoke(value);
        }
    }
}
