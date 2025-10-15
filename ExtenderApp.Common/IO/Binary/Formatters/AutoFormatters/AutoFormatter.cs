using System.Linq.Expressions;
using System.Reflection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 通过表达式树为类型 <typeparamref name="T"/> 自动生成序列化/反序列化/长度估算委托的通用格式化器。
    /// 使用方式：派生类在构造时收集需要处理的属性/字段并调用 <see cref="Init"/> 完成委托编译。
    /// </summary>
    public class AutoFormatter<T> : IBinaryFormatter<T>
    {
        /// <summary>
        /// 序列化委托签名。
        /// </summary>
        public delegate void SerializeMethod(ref ByteBuffer buffer, T value);

        /// <summary>
        /// 反序列化委托签名。
        /// </summary>
        public delegate T DeserializeMethod(ref ByteBuffer buffer);

        /// <summary>
        /// 获取序列化长度委托签名。
        /// </summary>
        private delegate long GetLengthMethod(T value);

        /// <summary>
        /// 默认对象存储，用于获取成员的格式化器。
        /// </summary>
        private readonly DefaultObjectStore _store;

        /// <summary>
        /// 序列化方法（在 <see cref="Init"/> 编译前调用将抛出异常）。
        /// </summary>
        private SerializeMethod serializeMethod = static (ref ByteBuffer _, T _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法序列化:{0}",typeof(T).FullName));

        /// <summary>
        /// 反序列化方法（在 <see cref="Init"/> 编译前调用将抛出异常）。
        /// </summary>
        private DeserializeMethod deserializeMethod = static (ref ByteBuffer _) =>
                throw new InvalidOperationException(string.Format("未调用Init,反序列化:{0}", typeof(T).FullName));

        /// <summary>
        /// 获取对象长度的方法（在 <see cref="Init"/> 编译前调用将抛出异常）。
        /// </summary>
        private GetLengthMethod getLengthMethod = static (T _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法获取序列化后长度:{0}", typeof(T).FullName));

        /// <summary>
        /// 默认预估长度（字节数）。为成员默认长度之和；若 <typeparamref name="T"/> 为引用类型，会额外包含 1 字节的空值标记。
        /// </summary>
        public int DefaultLength { get; private set; }

        public AutoFormatter(DefaultObjectStore store)
        {
            _store = store;
        }

        /// <summary>
        /// 从 <see cref="ByteBuffer"/> 反序列化为 <typeparamref name="T"/>。
        /// </summary>
        public T Deserialize(ref ByteBuffer buffer)
        {
            return deserializeMethod(ref buffer);
        }

        /// <summary>
        /// 将 <paramref name="value"/> 序列化写入 <see cref="ByteBuffer"/>。
        /// </summary>
        public void Serialize(ref ByteBuffer buffer, T value)
        {
            serializeMethod(ref buffer, value);
        }

        /// <summary>
        /// 估算（或精确计算）序列化 <paramref name="value"/> 需要的字节数。
        /// 引用类型或可空值类型为 null 时返回 1（空值标记）。
        /// </summary>
        public long GetLength(T value)
        {
            if (value is null)
                return 1;
            return getLengthMethod(value);
        }

        /// <summary>
        /// 基于提供的属性与字段集合，构建并编译序列化/反序列化/长度委托。
        /// 注意：
        /// - <paramref name="properties"/> 应仅包含可读写的公共属性；
        /// - <paramref name="fieldInfos"/> 应仅包含公共实例字段；
        /// - <paramref name="allLength"/> 应为上述成员总数，用于预分配列表容量。
        /// </summary>
        /// <param name="properties">要序列化/反序列化的属性集合，可为 null。</param>
        /// <param name="fieldInfos">要序列化/反序列化的字段集合，可为 null。</param>
        /// <param name="allLength">成员总数（用于容量预估）。</param>
        /// <exception cref="ArgumentException">当成员集合均为空或成员总数为 0 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当引用类型缺少公共无参构造函数时抛出。</exception>
        protected void Init(IEnumerable<PropertyInfo>? properties, IEnumerable<FieldInfo>? fieldInfos, int allLength)
        {
            // 成员校验（明确括号，避免歧义）
            if ((properties == null && fieldInfos == null) || allLength == 0)
                throw new ArgumentException("要生成序列化/反序列化的属性或字段列表不能为空。");

            Type type = typeof(T);
            var valueParameter = Expression.Parameter(type, "value");

            // 为表达式树准备容器
            List<Expression> serializesList = new(allLength);
            List<MemberBinding> deserializesList = new(allLength);
            Expression getLengthExpression = Expression.Constant((long)0);

            // 处理属性
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    // 由调用方保证属性可读写，这里直接生成表达式
                    MemberExpression member = Expression.Property(valueParameter, property);

                    _store.GetFormatterMethodInfo(property.PropertyType, out var serializeMethodInfo, out var deserializeMethodInfo, out var getLengthMethodInfo, out var formatter);
                    serializesList.Add(_store.CreateSerializeExpression(member, serializeMethodInfo, formatter));
                    deserializesList.Add(_store.CreateDeserializeExpression(property, deserializeMethodInfo, formatter));
                    _store.AppendGetLengthExpression(member, getLengthMethodInfo, formatter, ref getLengthExpression);
                    DefaultLength += formatter.DefaultLength;
                }
            }

            // 处理字段
            if (fieldInfos != null)
            {
                foreach (FieldInfo field in fieldInfos)
                {
                    MemberExpression member = Expression.Field(valueParameter, field);

                    _store.GetFormatterMethodInfo(field.FieldType, out var serializeMethodInfo, out var deserializeMethodInfo, out var getLengthMethodInfo, out var formatter);
                    serializesList.Add(_store.CreateSerializeExpression(member, serializeMethodInfo, formatter));
                    deserializesList.Add(_store.CreateDeserializeExpression(field, deserializeMethodInfo, formatter));
                    _store.AppendGetLengthExpression(member, getLengthMethodInfo, formatter, ref getLengthExpression);
                    DefaultLength += formatter.DefaultLength;
                }
            }

            // 引用类型需有公共无参构造函数以便 MemberInit
            if (type.IsClass && type.GetConstructor(Type.EmptyTypes) is null)
                throw new InvalidOperationException($"类型 {type.FullName} 缺少公共无参构造函数，无法进行反序列化。");

            // 获取序列化委托
            var block = Expression.Block(serializesList);
            var serializeLambda = Expression.Lambda<SerializeMethod>(block, _store.serializeParameter, valueParameter);
            serializeMethod = serializeLambda.Compile();

            // 获取反序列化委托（new + MemberInit）
            var newObject = Expression.New(type);
            var memberInit = Expression.MemberInit(newObject, deserializesList);
            var deserializeLambda = Expression.Lambda<DeserializeMethod>(memberInit, _store.deserializeParameter);
            deserializeMethod = deserializeLambda.Compile();

            // 获取“获取长度”委托
            var getLengthLambda = Expression.Lambda<GetLengthMethod>(getLengthExpression, valueParameter);
            getLengthMethod = getLengthLambda.Compile();

            // 为引用类型额外计入 1 字节空值标记（不是覆盖，而是累加）
            if (type.IsClass)
                DefaultLength += 1;
        }
    }
}