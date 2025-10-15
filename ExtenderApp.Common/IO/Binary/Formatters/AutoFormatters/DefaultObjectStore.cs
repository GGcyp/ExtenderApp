using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 默认的对象存储与工厂，负责：
    /// 1) 通过 <see cref="IBinaryFormatterResolver"/> 获取指定类型的二进制格式化器；
    /// 2) 反射并缓存格式化器上的 Serialize/Deserialize/GetLength 方法信息；
    /// 3) 为表达式树生成常用的调用/绑定节点。
    /// 线程安全：内部对方法信息的构建采用并发字典 + 二次检查锁，避免重复构建。
    /// </summary>
    public class DefaultObjectStore
    {
        /// <summary>
        /// 二进制格式化解析器。
        /// </summary>
        private readonly IBinaryFormatterResolver _resolver;

        /// <summary>
        /// 格式化方法缓存字典（按类型缓存）：
        /// (1) 序列化方法；
        /// (2) 反序列化方法；
        /// (3) 实例序列化长度方法；
        /// (4) 对应的格式化器实例。
        /// </summary>
        private readonly ConcurrentDictionary<Type, (MethodInfo, MethodInfo, MethodInfo, IBinaryFormatter)> _formatterMethodDict;

        /// <summary>
        /// 解析器中获取格式化器的泛型方法定义（通常为 <see cref="IBinaryFormatterResolver.GetFormatter{T}"/>）。
        /// 注意：当前实现通过 <c>typeof(IBinaryFormatterResolver).GetMethods().First()</c> 取得，需确保解析器接口中
        /// 该泛型方法顺序稳定且匹配预期。
        /// </summary>
        private readonly MethodInfo _getFormatterMethodInfo;

        /// <summary>
        /// 复用的“序列化方法”参数类型数组：ref ByteBuffer + T。
        /// 通过复用数组减少临时分配；在锁内写入第 2 个元素为具体类型。
        /// </summary>
        private readonly Type[] _serializeTypes;

        /// <summary>
        /// 复用的“反序列化方法”参数类型数组：ref ByteBuffer。
        /// </summary>
        private readonly Type[] _deserializeTypes;

        /// <summary>
        /// 复用的“获取长度方法”参数类型数组：T。
        /// 通过复用数组减少临时分配；在锁内写入第 1 个元素为具体类型。
        /// </summary>
        private readonly Type[] _getLengthTypes;

        /// <summary>
        /// 写入器参数表达式（ref ByteBuffer），用于生成调用 Serialize 的表达式树参数。
        /// </summary>
        public ParameterExpression serializeParameter { get; }

        /// <summary>
        /// 读取器参数表达式（ref ByteBuffer），用于生成调用 Deserialize 的表达式树参数。
        /// </summary>
        public ParameterExpression deserializeParameter { get; }

        /// <summary>
        /// 初始化 <see cref="DefaultObjectStore"/>。
        /// </summary>
        /// <param name="resolver">二进制格式化解析器，用于按需解析各类型的 <see cref="IBinaryFormatter{T}"/>。</param>
        public DefaultObjectStore(IBinaryFormatterResolver resolver)
        {
            _formatterMethodDict = new();
            _resolver = resolver;
            _getFormatterMethodInfo = typeof(IBinaryFormatterResolver).GetMethods().First();
            _serializeTypes = new Type[2] { typeof(ByteBuffer).MakeByRefType(), null };
            _deserializeTypes = new Type[1] { typeof(ByteBuffer).MakeByRefType() };
            _getLengthTypes = new Type[1] { null };

            serializeParameter = Expression.Parameter(typeof(ByteBuffer).MakeByRefType(), "serialize");
            deserializeParameter = Expression.Parameter(typeof(ByteBuffer).MakeByRefType(), "deserialize");
        }

        /// <summary>
        /// 获取并缓存指定 <paramref name="type"/> 对应格式化器的关键方法信息与格式化器实例。
        /// 优先从缓存返回；若未命中则通过解析器创建，并反射获取目标方法信息后写入缓存。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <param name="serializeMethodInfo">输出：Serialize 方法。</param>
        /// <param name="deserializeMethodInfo">输出：Deserialize 方法。</param>
        /// <param name="getLengthMethodInfo">输出：GetLength 方法。</param>
        /// <param name="formatter">输出：格式化器实例（<see cref="IBinaryFormatter"/>）。</param>
        /// <remarks>
        /// 线程安全：方法内部使用并发字典的 TryGetValue + 外部锁的二次检查，避免重复构建与竞态。
        /// 反射性能：通过复用参数类型数组降低临时分配。
        /// </remarks>
        /// <exception cref="TargetInvocationException">解析器内部抛出异常时可能包装于此异常类型。</exception>
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
        /// 创建调用 <paramref name="formatter"/>.Serialize(ref buffer, value.Member) 的方法调用表达式。
        /// </summary>
        /// <param name="member">要序列化的成员访问表达式（value.Property/Field）。</param>
        /// <param name="serializeMethodInfo">格式化器上的 Serialize 方法。</param>
        /// <param name="formatter">格式化器实例。</param>
        /// <returns>可直接加入表达式块的调用表达式。</returns>
        public MethodCallExpression CreateSerializeExpression(MemberExpression member, MethodInfo serializeMethodInfo, IBinaryFormatter formatter)
        {
            // 创建序列化方法调用表达式。
            MethodCallExpression serializeMethodCall = Expression.Call(
                Expression.Constant(formatter),
                serializeMethodInfo,
                serializeParameter,
                member
            );
            return serializeMethodCall;
        }

        /// <summary>
        /// 创建将 <paramref name="memberInfo"/> 绑定为 <paramref name="formatter"/>.Deserialize(ref buffer) 结果的成员赋值表达式。
        /// 该表达式通常用于 <see cref="MemberInitExpression"/> 的成员初始化。
        /// </summary>
        /// <param name="memberInfo">要绑定的属性或字段。</param>
        /// <param name="deserializeMethodInfo">格式化器上的 Deserialize 方法。</param>
        /// <param name="formatter">格式化器实例。</param>
        /// <returns>用于 MemberInit 的成员绑定表达式。</returns>
        public MemberAssignment CreateDeserializeExpression(MemberInfo memberInfo, MethodInfo deserializeMethodInfo, IBinaryFormatter formatter)
        {
            // 创建反序列化方法调用表达式。
            MethodCallExpression deserializeMethodCall = Expression.Call(
                Expression.Constant(formatter),
                deserializeMethodInfo,
                deserializeParameter
            );
            // 创建属性/字段绑定表达式。
            var member = Expression.Bind(memberInfo, deserializeMethodCall);
            return member;
        }

        /// <summary>
        /// 将“获取长度”的方法调用累加到传入的 <paramref name="binaryExpression"/> 中。
        /// 生成形式为：binaryExpression + formatter.GetLength(value.Member)。
        /// </summary>
        /// <param name="member">成员访问表达式（value.Property/Field）。</param>
        /// <param name="getLengthMethodInfo">格式化器上的 GetLength 方法。</param>
        /// <param name="formatter">格式化器实例。</param>
        /// <param name="binaryExpression">用于累加的表达式引用（形如 long total = 0）。</param>
        public void AppendGetLengthExpression(MemberExpression member, MethodInfo getLengthMethodInfo, IBinaryFormatter formatter, ref Expression binaryExpression)
        {
            // 创建获取长度方法调用表达式。
            MethodCallExpression getLengthMethodCall = Expression.Call(
                Expression.Constant(formatter),
                getLengthMethodInfo,
                member
            );
            // 将获取长度表达式添加到二进制表达式中。
            binaryExpression = Expression.Add(binaryExpression, getLengthMethodCall);
        }
    }
}
