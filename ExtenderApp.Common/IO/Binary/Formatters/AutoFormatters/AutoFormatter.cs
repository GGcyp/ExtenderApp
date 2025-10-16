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
    /// <remarks>
    /// 工作流程：
    /// 1) 在构造函数中创建统一的参数表达式并通过 <see cref="AutoMemberDetailsStore"/> 收集成员；
    /// 2) 基于收集的成员拼装表达式树，分别编译出 Serialize/Deserialize/GetLength 委托；
    /// 3) 对于引用类型，序列化/反序列化时会处理 Nil 标记；长度估算时如果为 null，返回 1（Nil 标记）。
    /// </remarks>
    public abstract class AutoFormatter<T> : ResolverFormatter<T>
    {
        /// <summary>
        /// 用于表达式树的共享参数：ref <see cref="ByteBuffer"/>。
        /// 所有生成的调用表达式都会共享此参数以确保签名一致。
        /// </summary>
        public readonly static ParameterExpression ByteBufferParameter = Expression.Parameter(typeof(ByteBuffer).MakeByRefType());

        /// <summary>
        /// 序列化委托签名。
        /// </summary>
        /// <param name="buffer">目标缓存（ref）。</param>
        /// <param name="value">要序列化的值。</param>
        public delegate void SerializeMethod(ref ByteBuffer buffer, T value);

        /// <summary>
        /// 反序列化委托签名。
        /// </summary>
        /// <param name="buffer">数据来源（ref）。</param>
        /// <returns>反序列化得到的值。</returns>
        public delegate T DeserializeMethod(ref ByteBuffer buffer);

        /// <summary>
        /// 获取序列化长度委托签名。
        /// </summary>
        /// <param name="value">目标值。</param>
        /// <returns>序列化所需字节数。</returns>
        private delegate long GetLengthMethod(T value);

        /// <summary>
        /// 序列化方法（在 <see cref="Init"/> 编译前调用将抛出异常）。
        /// </summary>
        private readonly SerializeMethod _serialize = static (ref ByteBuffer _, T _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法序列化:{0}", typeof(T).FullName));

        /// <summary>
        /// 反序列化方法（在 <see cref="Init"/> 编译前调用将抛出异常）。
        /// </summary>
        private readonly DeserializeMethod _deserialize = static (ref ByteBuffer _) =>
                throw new InvalidOperationException(string.Format("未调用Init,反序列化:{0}", typeof(T).FullName));

        /// <summary>
        /// 获取对象长度的方法（在 <see cref="Init"/> 编译前调用将抛出异常）。
        /// </summary>
        private readonly GetLengthMethod _getLength = static (T _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法获取序列化后长度:{0}", typeof(T).FullName));

        /// <summary>
        /// 指示 <typeparamref name="T"/> 是否为引用类型。
        /// 用于在序列化/反序列化/长度估算时处理 Nil 语义。
        /// </summary>
        private readonly bool IsClass;

        /// <summary>
        /// 默认预估长度（字节数）。为成员默认长度之和；若 <typeparamref name="T"/> 为引用类型，会额外包含 1 字节的空值标记。
        /// </summary>
        public override int DefaultLength { get; }

        /// <summary>
        /// 使用解析器构造并编译当前类型的序列化/反序列化/长度估算委托。
        /// </summary>
        /// <param name="resolver">格式化器解析器。</param>
        /// <exception cref="InvalidOperationException">当引用类型缺少公共无参构造函数时抛出。</exception>
        public AutoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            Type type = typeof(T);

            // 统一的 T 参数表达式（确保所有成员访问表达式共享同一 ParameterExpression）
            ParameterExpression parameter = Expression.Parameter(typeof(T));

            // 将统一参数传入 Store，确保成员表达式与后续 Lambda 使用相同的 ParameterExpression
            AutoMemberDetailsStore store = new(this, parameter);
            Init(store);
            List<AutoMemberDetails> list = store.memberDetails;
            int length = list.Count;
            Expression[] serializes = new Expression[length];
            MemberBinding[] deserializes = new MemberBinding[length];
            Expression[] getLengths = new Expression[length];

            for (int i = 0; i < length; i++)
            {
                var details = list[i];
                serializes[i] = details.Serialize;
                deserializes[i] = details.Deserialize;
                getLengths[i] = details.GetLength;
                DefaultLength += details.DefaultLength;
            }

            IsClass = type.IsClass;
            if (IsClass)
                DefaultLength = 1; // 引用类型包含 Nil 标记（注意：此处仅设置为 1，未叠加成员默认长度）

            // 编译 Serialize(ref ByteBuffer, T)
            var serializeBody = Expression.Block(serializes);
            var serializeLambda = Expression.Lambda<SerializeMethod>(serializeBody, ByteBufferParameter, parameter);
            _serialize = serializeLambda.Compile();

            // 编译 Deserialize(ref ByteBuffer)：值类型直接 new；引用类型查找公共无参构造
            NewExpression newExpr;
            if (typeof(T).IsValueType)
            {
                newExpr = Expression.New(type);
            }
            else
            {
                var ctor = typeof(T).GetConstructor(Type.EmptyTypes);
                if (ctor is null)
                    throw new InvalidOperationException(string.Format("类型 {0} 缺少公共无参构造函数，无法自动反序列化。", typeof(T).FullName));
                newExpr = Expression.New(ctor);
            }

            // 以 MemberInit 将成员绑定至新建对象
            Expression deserializeBody = Expression.MemberInit(newExpr, deserializes);
            var deserializeLambda = Expression.Lambda<DeserializeMethod>(deserializeBody, ByteBufferParameter);
            _deserialize = deserializeLambda.Compile();

            // 编译 GetLength(T) —— 非空场景下各成员长度之和
            Expression totalLen = getLengths[0];
            for (int i = 1; i < getLengths.Length; i++)
            {
                totalLen = Expression.Add(totalLen, getLengths[i]);
            }
            var getLengthLambda = Expression.Lambda<GetLengthMethod>(totalLen, parameter);
            _getLength = getLengthLambda.Compile();

        }

        /// <summary>
        /// 从 <see cref="ByteBuffer"/> 反序列化为 <typeparamref name="T"/>。
        /// </summary>
        /// <param name="buffer">数据来源（ref）。</param>
        /// <returns>反序列化得到的值；若读取到 Nil 且 <typeparamref name="T"/> 为引用类型则返回 null。</returns>
        public override T Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
                return default!;

            return _deserialize(ref buffer);
        }

        /// <summary>
        /// 将 <paramref name="value"/> 序列化写入 <see cref="ByteBuffer"/>。
        /// </summary>
        /// <param name="buffer">目标缓存（ref）。</param>
        /// <param name="value">要序列化的值。</param>
        public override void Serialize(ref ByteBuffer buffer, T value)
        {
            if (IsClass && value is null)
            {
                WriteNil(ref buffer);
                return;
            }

            _serialize(ref buffer, value);
        }

        /// <summary>
        /// 估算（或精确计算）序列化 <paramref name="value"/> 需要的字节数。
        /// 引用类型或可空值类型为 null 时返回 1（空值标记）。
        /// </summary>
        /// <param name="value">目标值。</param>
        /// <returns>所需字节数。</returns>
        public override long GetLength(T value)
        {
            if (IsClass && value is null)
                return 1;
            return _getLength(value);
        }

        /// <summary>
        /// 基于提供的属性与字段集合，构建并编译序列化/反序列化/长度委托。
        /// </summary>
        /// <param name="store">成员收集器。</param>
        protected abstract void Init(AutoMemberDetailsStore store);

        /// <summary>
        /// 创建针对某成员的序列化调用表达式：formatter.Serialize(ref ByteBuffer, member)。
        /// </summary>
        /// <param name="member">成员访问表达式。</param>
        /// <param name="serializeMethodInfo">序列化方法信息。</param>
        /// <param name="formatter">成员类型对应的格式化器实例。</param>
        /// <returns>方法调用表达式。</returns>
        private MethodCallExpression CreateSerializeExpression(MemberExpression member, MethodInfo serializeMethodInfo, IBinaryFormatter formatter)
        {
            var instance = Expression.Convert(Expression.Constant(formatter), serializeMethodInfo.DeclaringType!);
            return Expression.Call(instance, serializeMethodInfo, ByteBufferParameter, member);
        }

        /// <summary>
        /// 创建针对某成员的反序列化绑定表达式：member = formatter.Deserialize(ref ByteBuffer)。
        /// </summary>
        /// <param name="memberInfo">目标成员信息。</param>
        /// <param name="deserializeMethodInfo">反序列化方法信息。</param>
        /// <param name="formatter">成员类型对应的格式化器实例。</param>
        /// <returns>成员赋值绑定。</returns>
        private MemberAssignment CreateDeserializeExpression(MemberInfo memberInfo, MethodInfo deserializeMethodInfo, IBinaryFormatter formatter)
        {
            var instance = Expression.Convert(Expression.Constant(formatter), deserializeMethodInfo.DeclaringType!);
            var call = Expression.Call(instance, deserializeMethodInfo, ByteBufferParameter);

            var targetType = memberInfo is PropertyInfo p ? p.PropertyType : ((FieldInfo)memberInfo).FieldType;
            Expression value = call;
            if (call.Type != targetType)
                value = Expression.Convert(call, targetType);

            return Expression.Bind(memberInfo, value);
        }

        /// <summary>
        /// 创建针对某成员的 GetLength 调用表达式：formatter.GetLength(member)。
        /// </summary>
        /// <param name="member">成员访问表达式。</param>
        /// <param name="getLengthMethodInfo">估算长度方法信息。</param>
        /// <param name="formatter">成员类型对应的格式化器实例。</param>
        /// <returns>方法调用表达式。</returns>
        private MethodCallExpression CreateGetLengthExpression(MemberExpression member, MethodInfo getLengthMethodInfo, IBinaryFormatter formatter)
        {
            var instance = Expression.Convert(Expression.Constant(formatter), getLengthMethodInfo.DeclaringType!);
            return Expression.Call(instance, getLengthMethodInfo, member);
        }

        /// <summary>
        /// 为属性创建成员信息（序列化/反序列化/长度估算表达式及默认长度）。
        /// </summary>
        /// <param name="parameter">统一的对象参数表达式。</param>
        /// <param name="propertyInfo">属性信息。</param>
        /// <returns>成员描述信息。</returns>
        private AutoMemberDetails CreateAutoMemberInfo(Expression parameter, PropertyInfo propertyInfo)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            ArgumentNullException.ThrowIfNull(propertyInfo);

            MemberExpression member = Expression.Property(parameter, propertyInfo);

            var formatter = GetFormatter(propertyInfo.PropertyType);
            var info = formatter.MethodInfoDetails;
            MethodCallExpression serialize = CreateSerializeExpression(member, info.Serialize, formatter);
            MemberBinding deserialize = CreateDeserializeExpression(propertyInfo, info.Deserialize, formatter);
            MethodCallExpression getLength = CreateGetLengthExpression(member, info.GetLength, formatter);

            return new AutoMemberDetails(serialize, deserialize, getLength, propertyInfo.Name, formatter.DefaultLength);
        }

        /// <summary>
        /// 为字段创建成员信息（序列化/反序列化/长度估算表达式及默认长度）。
        /// </summary>
        /// <param name="parameter">统一的对象参数表达式。</param>
        /// <param name="fieldInfo">字段信息。</param>
        /// <returns>成员描述信息。</returns>
        private AutoMemberDetails CreateAutoMemberInfo(Expression parameter, FieldInfo fieldInfo)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            ArgumentNullException.ThrowIfNull(fieldInfo);

            MemberExpression member = Expression.Field(parameter, fieldInfo);

            var formatter = GetFormatter(fieldInfo.FieldType);
            var info = formatter.MethodInfoDetails;
            MethodCallExpression serialize = CreateSerializeExpression(member, info.Serialize, formatter);
            MemberBinding deserialize = CreateDeserializeExpression(fieldInfo, info.Deserialize, formatter);
            MethodCallExpression getLength = CreateGetLengthExpression(member, info.GetLength, formatter);
            return new AutoMemberDetails(serialize, deserialize, getLength, fieldInfo.Name, formatter.DefaultLength);
        }

        /// <summary>
        /// 成员收集器：用于在构造过程中声明需要参与序列化/反序列化/长度估算的属性或字段。
        /// 持有统一的 <see cref="ParameterExpression"/> 以避免表达式树中的参数不一致问题。
        /// </summary>
        protected ref struct AutoMemberDetailsStore
        {
            private const string PropertyInfoString = nameof(PropertyInfo);
            private const string FieldInfoString = nameof(FieldInfo);

            AutoFormatter<T> autoFormatter;
            ParameterExpression parameter;
            /// <summary>
            /// 已收集的成员详情列表（按添加顺序生成序列化/反序列化/长度表达式）。
            /// </summary>
            public List<AutoMemberDetails> memberDetails;

            /// <summary>
            /// 使用给定格式化器与参数表达式创建成员收集器。
            /// </summary>
            /// <param name="autoFormatter">目标自动格式化器。</param>
            /// <param name="parameter">统一的对象参数表达式。</param>
            public AutoMemberDetailsStore(AutoFormatter<T> autoFormatter, ParameterExpression parameter)
            {
                this.autoFormatter = autoFormatter;
                this.parameter = parameter;
                memberDetails = new();
            }

            /// <summary>
            /// 添加一个属性或字段（通过成员访问表达式选择）。
            /// </summary>
            /// <typeparam name="TMember">成员信息类型：<see cref="PropertyInfo"/> 或 <see cref="FieldInfo"/>。</typeparam>
            /// <param name="selector">成员访问表达式，如 x =&gt; x.Property 或 x =&gt; x.Field。</param>
            /// <returns>当前收集器（支持链式调用）。</returns>
            /// <exception cref="InvalidOperationException">当表达式不是成员访问时抛出。</exception>
            /// <exception cref="ArgumentException">当表达式不是属性或字段访问时抛出。</exception>
            public AutoMemberDetailsStore Add<TMember>(Expression<Func<T, TMember>> selector)
                where TMember : MemberInfo
            {
                ArgumentNullException.ThrowIfNull(selector, nameof(selector));

                Expression body = selector.Body;

                if (body is UnaryExpression u && u.NodeType == ExpressionType.Convert)
                    body = u.Operand;

                if (body is not MemberExpression m)
                    throw new InvalidOperationException("传入的必须是属性或字段");

                AutoMemberDetails details = default;
                string infoName = string.Empty;
                switch (m.Member)
                {
                    case PropertyInfo pi:
                        details = autoFormatter.CreateAutoMemberInfo(parameter, pi);
                        infoName = PropertyInfoString;
                        break;

                    case FieldInfo fi:
                        details = autoFormatter.CreateAutoMemberInfo(parameter, fi);
                        infoName = FieldInfoString;
                        break;

                    default:
                        throw new ArgumentException("selector 必须为属性或字段访问表达式，例如 x => x.Property", nameof(selector));
                }

                // 重复检查（基于成员名）
                CheckDuplicate(details.Name, PropertyInfoString);

                memberDetails.Add(details);

                return this;
            }

            /// <summary>
            /// 添加一个属性（通过反射信息）。
            /// </summary>
            /// <param name="propertyInfo">属性信息。</param>
            /// <returns>当前收集器。</returns>
            public AutoMemberDetailsStore Add(PropertyInfo propertyInfo)
            {
                ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

                AutoMemberDetails details = autoFormatter.CreateAutoMemberInfo(parameter, propertyInfo);

                CheckDuplicate(details.Name, PropertyInfoString);

                memberDetails.Add(details);

                return this;
            }

            /// <summary>
            /// 添加一个字段（通过反射信息）。
            /// </summary>
            /// <param name="fieldInfo">字段信息。</param>
            /// <returns>当前收集器。</returns>
            public AutoMemberDetailsStore Add(FieldInfo fieldInfo)
            {
                ArgumentNullException.ThrowIfNull(fieldInfo, nameof(fieldInfo));

                AutoMemberDetails details = autoFormatter.CreateAutoMemberInfo(parameter, fieldInfo);

                CheckDuplicate(details.Name, FieldInfoString);

                memberDetails.Add(details);

                return this;
            }

            /// <summary>
            /// 检查是否重复添加同名成员。
            /// </summary>
            /// <param name="name">成员名称。</param>
            /// <param name="infoName">成员类型名提示（仅用于错误信息）。</param>
            /// <exception cref="Exception">当发现重复成员时抛出。</exception>
            private void CheckDuplicate(string name, string infoName)
            {
                for (int i = 0; i < memberDetails.Count; i++)
                {
                    var item = memberDetails[i];
                    if (item.Name == name)
                    {
                        throw new Exception(string.Format("重复添加{0}名称为：{1} : 在第{2}个输入", infoName, name, i));
                    }
                }
            }
        }

        /// <summary>
        /// 单个成员（属性/字段）的表达式与元数据聚合。
        /// </summary>
        protected readonly struct AutoMemberDetails
        {
            /// <summary>
            /// 针对该成员的序列化调用表达式。
            /// </summary>
            public Expression Serialize { get; }

            /// <summary>
            /// 针对该成员的反序列化成员绑定。
            /// </summary>
            public MemberBinding Deserialize { get; }

            /// <summary>
            /// 针对该成员的长度估算调用表达式。
            /// </summary>
            public Expression GetLength { get; }

            /// <summary>
            /// 成员名称（用于重复校验与诊断）。
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// 成员的默认预估长度（由成员类型的格式化器提供）。
            /// </summary>
            public int DefaultLength { get; }

            /// <summary>
            /// 使用给定的表达式与元信息构造成员详情。
            /// </summary>
            /// <param name="serialize">序列化表达式。</param>
            /// <param name="deserialize">反序列化绑定。</param>
            /// <param name="getLength">长度估算表达式。</param>
            /// <param name="name">成员名称。</param>
            /// <param name="defaultLength">成员默认预估长度。</param>
            public AutoMemberDetails(Expression serialize, MemberBinding deserialize, Expression getLength, string name, int defaultLength)
            {
                Serialize = serialize;
                Deserialize = deserialize;
                GetLength = getLength;
                Name = name;
                DefaultLength = defaultLength;
            }
        }
    }
}