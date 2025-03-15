using System.Linq.Expressions;
using System.Reflection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    /// <summary>
    /// 默认的对象格式化器类，继承自 <see cref="ResolverFormatter{T}"/> 类。
    /// </summary>
    /// <typeparam name="T">要格式化的对象的类型。</typeparam>
    internal class DefaultObjectFormatter<T> : ResolverFormatter<T>
    {
        /// <summary>
        /// 序列化方法的委托类型。
        /// </summary>
        /// <param name="writer">一个 <see cref="ExtenderBinaryWriter"/> 类型的引用，用于写入序列化后的数据。</param>
        /// <param name="value">要序列化的对象。</param>
        internal delegate void SerializeMethod(ref ExtenderBinaryWriter writer, T value);

        /// <summary>
        /// 反序列化方法的委托类型。
        /// </summary>
        /// <param name="writer">一个 <see cref="ExtenderBinaryReader"/> 类型的引用，用于读取反序列化后的数据。</param>
        /// <returns>反序列化后的对象。</returns>
        internal delegate T DeserializeMethod(ref ExtenderBinaryReader writer);

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
        public override int Length => _length;

        public DefaultObjectFormatter(DefaultObjectStore store, IBinaryFormatterResolver resolver) : base(resolver)
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
                _length += formatter.Length;
            }

            //获取序列化委托
            var block = Expression.Block(serializes);
            var serialzeLambda = Expression.Lambda<SerializeMethod>(block, store.WriterParameter, valueParameter);
            _serializeMethod = serialzeLambda.Compile();

            //获取反序列化委托
            var newObject = Expression.New(type);
            var memberInit = Expression.MemberInit(newObject, deserializes);
            var deserializeLambda = Expression.Lambda<DeserializeMethod>(memberInit, store.ReaderParameter);
            _deserializeMethod = deserializeLambda.Compile();

            //获取获取长度委托
            var getLengthLambda = Expression.Lambda<GetLengthMethod>(getLengthExpression, valueParameter);
            _getLengthMethod = getLengthLambda.Compile();
        }

        public override T Deserialize(ref ExtenderBinaryReader reader)
        {
            return _deserializeMethod(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, T value)
        {
            _serializeMethod(ref writer, value);
        }

        public override long GetLength(T value)
        {
            return _getLengthMethod.Invoke(value);
        }
    }
}
