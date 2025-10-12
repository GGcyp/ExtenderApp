using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 默认的对象存储类
    /// </summary>
    internal class DefaultObjectStore
    {
        /// <summary>
        /// 二进制格式化解析器
        /// </summary>
        private readonly IBinaryFormatterResolver _resolver;

        /// <summary>
        /// 格式化方法字典
        /// 1、序列化方法
        /// 2、反序列化方法
        /// 3、实力序列化后长度方法
        /// 4、格式化器
        /// </summary>
        private readonly ConcurrentDictionary<Type, (MethodInfo, MethodInfo, MethodInfo, IBinaryFormatter)> _formatterMethodDict;

        /// <summary>
        /// 获取格式化器的方法信息
        /// </summary>
        private readonly MethodInfo _getFormatterMethodInfo;

        /// <summary>
        /// 存储序列化类型的数组
        /// </summary>
        private readonly Type[] _serializeTypes;

        /// <summary>
        /// 私有只读字段，用于存储反序列化类型的数组。
        /// </summary>
        private readonly Type[] _deserializeTypes;

        /// <summary>
        /// 私有只读字段，用于存储获取长度类型的数组。
        /// </summary>
        private readonly Type[] _getLengthTypes;

        /// <summary>
        /// 写入器参数表达式
        /// </summary>
        public ParameterExpression WriterParameter { get; }

        /// <summary>
        /// 读取器参数表达式
        /// </summary>
        public ParameterExpression ReaderParameter { get; }

        /// <summary>
        /// 初始化 DefaultObjectStore 类的新实例
        /// </summary>
        /// <param name="resolver">二进制格式化解析器</param>
        public DefaultObjectStore(IBinaryFormatterResolver resolver)
        {
            _formatterMethodDict = new();
            _resolver = resolver;
            _getFormatterMethodInfo = typeof(IBinaryFormatterResolver).GetMethods().First();
            _serializeTypes = new Type[2] { typeof(ExtenderBinaryWriter).MakeByRefType(), null };
            _deserializeTypes = new Type[1] { typeof(ExtenderBinaryReader).MakeByRefType() };
            _getLengthTypes = new Type[1] { null };

            WriterParameter = Expression.Parameter(typeof(ExtenderBinaryWriter).MakeByRefType(), "writer");
            ReaderParameter = Expression.Parameter(typeof(ExtenderBinaryReader).MakeByRefType(), "Reader");
        }

        /// <summary>
        /// 获取格式化器的方法信息和格式化器实例。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <param name="serializeMethodInfo">序列化方法信息。</param>
        /// <param name="deserializeMethodInfo">反序列化方法信息。</param>
        /// <param name="getLengthMethodInfo">获取长度方法信息。</param>
        /// <param name="formatter">格式化器实例。</param>
        public void GetFormatterMethodInfo(Type type, out MethodInfo serializeMethodInfo, out MethodInfo deserializeMethodInfo, out MethodInfo getLengthMethodInfo, out IBinaryFormatter formatter)
        {
            // 如果字典中已经存在目标类型的方法信息，则直接返回。
            if (_formatterMethodDict.TryGetValue(type, out var item))
            {
                serializeMethodInfo = item.Item1;
                deserializeMethodInfo = item.Item2;
                getLengthMethodInfo = item.Item3;
                formatter = item.Item4;
                return;
            }

            // 加锁以避免线程安全问题。
            lock (_formatterMethodDict)
            {
                // 再次检查字典中是否已经存在目标类型的方法信息。
                if (_formatterMethodDict.TryGetValue(type, out item))
                {
                    serializeMethodInfo = item.Item1;
                    deserializeMethodInfo = item.Item2;
                    getLengthMethodInfo = item.Item3;
                    formatter = item.Item4;
                    return;
                }

                // 获取目标类型的格式化器方法信息。
                var getMethodInfo = _getFormatterMethodInfo.MakeGenericMethod(type);
                formatter = (IBinaryFormatter)getMethodInfo.Invoke(_resolver, null)!;

                // 获取目标类型的格式化器类型。
                Type formatterType = typeof(IBinaryFormatter<>).MakeGenericType(type);

                // 获取序列化方法的参数类型。
                _serializeTypes[1] = type;
                // 获取获取长度方法的参数类型。
                _getLengthTypes[0] = type;

                // 获取序列化方法信息。
                serializeMethodInfo = formatterType.GetRuntimeMethod("Serialize", _serializeTypes);

                // 获取反序列化方法信息。
                deserializeMethodInfo = formatterType.GetRuntimeMethod("Deserialize", _deserializeTypes);

                // 获取获取长度方法信息。
                getLengthMethodInfo = formatterType.GetRuntimeMethod("GetLength", _getLengthTypes);

                // 将方法信息和格式化器实例添加到字典中。
                _formatterMethodDict[type] = (serializeMethodInfo, deserializeMethodInfo, getLengthMethodInfo, formatter);
            }
        }

        /// <summary>
        /// 创建属性序列化表达式。
        /// </summary>
        /// <param name="member">成员表达式。</param>
        /// <param name="serializeMethodInfo">序列化方法信息。</param>
        /// <param name="formatter">格式化器实例。</param>
        /// <returns>序列化表达式。</returns>
        public MethodCallExpression CreatePropertySerializeExpression(MemberExpression member, MethodInfo serializeMethodInfo, IBinaryFormatter formatter)
        {
            // 创建序列化方法调用表达式。
            MethodCallExpression serializeMethodCall = Expression.Call(
                Expression.Constant(formatter),
                serializeMethodInfo,
                WriterParameter,
                member
            );
            return serializeMethodCall;
        }

        /// <summary>
        /// 创建属性反序列化表达式。
        /// </summary>
        /// <param name="property">属性信息。</param>
        /// <param name="deserializeMethodInfo">反序列化方法信息。</param>
        /// <param name="formatter">格式化器实例。</param>
        /// <returns>反序列化表达式。</returns>
        public MemberAssignment CreatePropertyDeserializeExpression(PropertyInfo property, MethodInfo deserializeMethodInfo, IBinaryFormatter formatter)
        {
            // 创建反序列化方法调用表达式。
            MethodCallExpression deserializeMethodCall = Expression.Call(
                Expression.Constant(formatter),
                deserializeMethodInfo,
                ReaderParameter
            );
            // 创建属性绑定表达式。
            var member = Expression.Bind(property, deserializeMethodCall);
            return member;
        }

        /// <summary>
        /// 创建获取长度表达式。
        /// </summary>
        /// <param name="member">成员表达式。</param>
        /// <param name="getLengthMethodInfo">获取长度方法信息。</param>
        /// <param name="formatter">格式化器实例。</param>
        /// <param name="binaryExpression">二进制表达式。</param>
        /// <returns>获取长度表达式。</returns>
        public Expression CreateGetLengthExpression(MemberExpression member, MethodInfo getLengthMethodInfo, IBinaryFormatter formatter, Expression binaryExpression)
        {
            // 创建获取长度方法调用表达式。
            MethodCallExpression getLengthMethodCall = Expression.Call(
                Expression.Constant(formatter),
                getLengthMethodInfo,
                member
            );
            // 将获取长度表达式添加到二进制表达式中。
            binaryExpression = Expression.Add(binaryExpression, getLengthMethodCall);
            return binaryExpression;
        }
    }
}
