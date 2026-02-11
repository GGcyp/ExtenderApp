using System.Linq.Expressions;
using System.Reflection;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 通过表达式树为类型 <typeparamref name="T"/> 自动生成序列化/反序列化/长度估算委托的通用格式化器。 使用方式：派生类在构造时收集需要处理的属性/字段并调用 <see cref="Init"/> 完成委托编译。
    /// </summary>
    /// <remarks>
    /// 工作流程：
    /// 1) 在构造函数中创建统一的参数表达式并通过 <see cref="AutoMemberDetailsStore"/> 收集成员；
    /// 2) 基于收集的成员拼装表达式树，分别编译出 Serialize/Deserialize/GetLength 委托；
    /// 3) 对于引用类型，序列化/反序列化时会处理 Nil 标记；长度估算时如果为 null，返回 1（Nil 标记）。
    /// </remarks>
    public abstract class AutoFormatter<T> : ResolverFormatter<T>
    {
        #region ParameterExpressions

        /// <summary>
        /// 用于表达式树的共享参数： <see cref="AbstractBuffer{byte}"/>。
        /// </summary>
        public static readonly ParameterExpression BufferParameter = Expression.Parameter(typeof(AbstractBuffer<byte>), "buffer");

        /// <summary>
        /// 用于表达式树的共享参数：ref <see cref="SpanWriter{byte}"/>。
        /// </summary>
        public static readonly ParameterExpression SpanWriterParameter = Expression.Parameter(typeof(SpanWriter<byte>).MakeByRefType(), "writer");

        /// <summary>
        /// 用于表达式树的共享参数： <see cref="AbstractBufferReader{byte}"/>。
        /// </summary>
        public static readonly ParameterExpression BufferReaderParameter = Expression.Parameter(typeof(AbstractBufferReader<byte>), "reader");

        /// <summary>
        /// 用于表达式树的共享参数：ref <see cref="SpanReader{byte}"/>。
        /// </summary>
        public static readonly ParameterExpression SpanReaderParameter = Expression.Parameter(typeof(SpanReader<byte>).MakeByRefType(), "reader");

        #endregion ParameterExpressions

        #region Delegates

        public delegate void SerializeBufferMethod(AbstractBuffer<byte> buffer, T value);

        public delegate void SerializeSpanMethod(ref SpanWriter<byte> writer, T value);

        public delegate T DeserializeBufferMethod(AbstractBufferReader<byte> reader);

        public delegate T DeserializeSpanMethod(ref SpanReader<byte> reader);

        private delegate long GetLengthMethod(T value);

        #endregion Delegates

        private readonly SerializeBufferMethod _serializeBuffer = static (AbstractBuffer<byte> _, T _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法序列化:{0}", typeof(T).FullName));

        private readonly SerializeSpanMethod _serializeSpan = static (ref SpanWriter<byte> _, T _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法序列化:{0}", typeof(T).FullName));

        private readonly DeserializeBufferMethod _deserializeBuffer = static (AbstractBufferReader<byte> _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法反序列化:{0}", typeof(T).FullName));

        private readonly DeserializeSpanMethod _deserializeSpan = static (ref SpanReader<byte> _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法反序列化:{0}", typeof(T).FullName));

        private readonly GetLengthMethod _getLength = static (T _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法获取序列化后长度:{0}", typeof(T).FullName));

        /// <summary>
        /// 指示 <typeparamref name="T"/> 是否为引用类型。用于在序列化/反序列化/长度估算时处理 Nil 语义。
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
        public AutoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            Type type = typeof(T);
            ParameterExpression parameter = Expression.Parameter(typeof(T), "value");

            AutoMemberDetailsStore store = new(this, parameter);
            Init(store);

            List<AutoMemberDetails> list = store.memberDetails;
            int length = list.Count;

            Expression[] serializesBuffer = new Expression[length];
            Expression[] serializesSpan = new Expression[length];
            MemberBinding[] deserializesBuffer = new MemberBinding[length];
            MemberBinding[] deserializesSpan = new MemberBinding[length];
            Expression[] getLengths = new Expression[length];

            for (int i = 0; i < length; i++)
            {
                var details = list[i];
                serializesBuffer[i] = details.SerializeBuffer;
                serializesSpan[i] = details.SerializeSpan;
                deserializesBuffer[i] = details.DeserializeBuffer;
                deserializesSpan[i] = details.DeserializeSpan;
                getLengths[i] = details.GetLength;
                DefaultLength += details.DefaultLength;
            }

            IsClass = type.IsClass;
            if (IsClass)
                DefaultLength = NilLength;

            // 编译 Serialize 方法
            _serializeBuffer = Expression.Lambda<SerializeBufferMethod>(Expression.Block(serializesBuffer), BufferParameter, parameter).Compile();
            _serializeSpan = Expression.Lambda<SerializeSpanMethod>(Expression.Block(serializesSpan), SpanWriterParameter, parameter).Compile();

            // 编译 Deserialize 方法
            NewExpression newExpr = type.IsValueType
                ? Expression.New(type)
                : Expression.New(type.GetConstructor(Type.EmptyTypes) ?? throw new InvalidOperationException(string.Format("类型 {0} 缺少公共无参构造函数。", type.FullName)));

            _deserializeBuffer = Expression.Lambda<DeserializeBufferMethod>(Expression.MemberInit(newExpr, deserializesBuffer), BufferReaderParameter).Compile();
            _deserializeSpan = Expression.Lambda<DeserializeSpanMethod>(Expression.MemberInit(newExpr, deserializesSpan), SpanReaderParameter).Compile();

            // 编译 GetLength 方法
            Expression totalLen = getLengths.Length > 0 ? getLengths[0] : Expression.Constant(0L);
            for (int i = 1; i < getLengths.Length; i++)
            {
                totalLen = Expression.Add(totalLen, getLengths[i]);
            }
            _getLength = Expression.Lambda<GetLengthMethod>(totalLen, parameter).Compile();
        }

        public override void Serialize(AbstractBuffer<byte> buffer, T value)
        {
            if (IsClass && value is null)
            {
                WriteNil(buffer);
                return;
            }
            _serializeBuffer(buffer, value);
        }

        public override void Serialize(ref SpanWriter<byte> writer, T value)
        {
            if (IsClass && value is null)
            {
                WriteNil(ref writer);
                return;
            }
            _serializeSpan(ref writer, value);
        }

        public override T Deserialize(AbstractBufferReader<byte> reader)
        {
            if (IsClass && TryReadNil(reader))
                return default!;
            return _deserializeBuffer(reader);
        }

        public override T Deserialize(ref SpanReader<byte> reader)
        {
            if (IsClass && TryReadNil(ref reader))
                return default!;
            return _deserializeSpan(ref reader);
        }

        public override long GetLength(T value)
        {
            if (IsClass && value is null)
                return NilLength;
            return _getLength(value);
        }

        protected abstract void Init(AutoMemberDetailsStore store);

        private MethodCallExpression CreateSerializeExpression(MemberExpression member, MethodInfo method, ParameterExpression param, IBinaryFormatter formatter)
        {
            var instance = Expression.Convert(Expression.Constant(formatter), method.DeclaringType!);
            return Expression.Call(instance, method, param, member);
        }

        private MemberAssignment CreateDeserializeExpression(MemberInfo memberInfo, MethodInfo method, ParameterExpression param, IBinaryFormatter formatter)
        {
            var instance = Expression.Convert(Expression.Constant(formatter), method.DeclaringType!);
            var call = Expression.Call(instance, method, param);
            var targetType = memberInfo is PropertyInfo p ? p.PropertyType : ((FieldInfo)memberInfo).FieldType;
            return Expression.Bind(memberInfo, call.Type != targetType ? Expression.Convert(call, targetType) : call);
        }

        private MethodCallExpression CreateGetLengthExpression(MemberExpression member, MethodInfo method, IBinaryFormatter formatter)
        {
            var instance = Expression.Convert(Expression.Constant(formatter), method.DeclaringType!);
            return Expression.Call(instance, method, member);
        }

        private AutoMemberDetails CreateAutoMemberInfo(Expression parameter, MemberInfo info)
        {
            Type memberType = info is PropertyInfo p ? p.PropertyType : ((FieldInfo)info).FieldType;
            MemberExpression member = Expression.MakeMemberAccess(parameter, info);
            var formatter = GetFormatter(memberType);
            var md = formatter.MethodInfoDetails;

            return new AutoMemberDetails(
                CreateSerializeExpression(member, md.SerializeBuffer, BufferParameter, formatter),
                CreateSerializeExpression(member, md.SerializeSpan, SpanWriterParameter, formatter),
                CreateDeserializeExpression(info, md.DeserializeBuffer, BufferReaderParameter, formatter),
                CreateDeserializeExpression(info, md.DeserializeSpan, SpanReaderParameter, formatter),
                CreateGetLengthExpression(member, md.GetLength, formatter),
                info.Name,
                formatter.DefaultLength);
        }

        protected struct AutoMemberDetailsStore
        {
            private AutoFormatter<T> autoFormatter;
            private ParameterExpression parameter;
            public List<AutoMemberDetails> memberDetails;

            public AutoMemberDetailsStore(AutoFormatter<T> autoFormatter, ParameterExpression parameter)
            {
                this.autoFormatter = autoFormatter;
                this.parameter = parameter;
                memberDetails = new();
            }

            public AutoMemberDetailsStore Add<TMember>(Expression<Func<T, TMember>> selector)
            {
                Expression body = selector.Body;
                if (body is UnaryExpression u && u.NodeType == ExpressionType.Convert) body = u.Operand;
                if (body is not MemberExpression m) throw new InvalidOperationException("必须是属性或字段");
                return Add(m.Member);
            }

            public AutoMemberDetailsStore Add(PropertyInfo propertyInfo) => Add((MemberInfo)propertyInfo);

            public AutoMemberDetailsStore Add(FieldInfo fieldInfo) => Add((MemberInfo)fieldInfo);

            private AutoMemberDetailsStore Add(MemberInfo info)
            {
                var details = autoFormatter.CreateAutoMemberInfo(parameter, info);
                for (int i = 0; i < memberDetails.Count; i++)
                    if (memberDetails[i].Name == details.Name)
                        throw new Exception($"重复添加成员：{details.Name}");
                memberDetails.Add(details);
                return this;
            }
        }

        protected readonly struct AutoMemberDetails
        {
            public Expression SerializeBuffer { get; }
            public Expression SerializeSpan { get; }
            public MemberBinding DeserializeBuffer { get; }
            public MemberBinding DeserializeSpan { get; }
            public Expression GetLength { get; }
            public string Name { get; }
            public int DefaultLength { get; }

            public AutoMemberDetails(Expression serBuf, Expression serSpan, MemberBinding desBuf, MemberBinding desSpan, Expression getLen, string name, int defLen)
            {
                SerializeBuffer = serBuf;
                SerializeSpan = serSpan;
                DeserializeBuffer = desBuf;
                DeserializeSpan = desSpan;
                GetLength = getLen;
                Name = name;
                DefaultLength = defLen;
            }
        }
    }
}