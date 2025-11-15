using ExtenderApp.Common.Encodings;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// 表示一个 SNMP PDU（Protocol Data Unit），如 GetRequest/GetResponse/SetRequest 等。
    /// 该类型为值类型（readonly struct），承载 PDU 的元信息与 varbind 列表（变量绑定序列）。
    /// </summary>
    /// <remarks>
    /// - PDU 中的 varbind 列表使用 <see cref="ValueOrList{T}"/> 以减少短期分配开销；
    ///   在构造/解析 PDU 时可直接向 <see cref="VarBinds"/> 添加项。
    /// - 因为为 readonly struct，实例成员在构造时应一次性初始化；复制该结构体会拷贝 <see cref="ValueOrList{T}"/> 的引用语义，
    ///   使用时应注意不要对共享底层资源重复释放（如 <see cref="SnmpValue"/> 内部持有的缓冲需由单一所有者管理）。
    /// </remarks>
    public readonly struct SnmpPdu : IDisposable
    {
        private static Lazy<Random> RandomLazy = new();

        /// <summary>
        /// PDU 类型（例如 <see cref="SnmpPduType.GetRequest"/>、<see cref="SnmpPduType.GetResponse"/> 等）。
        /// </summary>
        public SnmpPduType PduType { get; }

        /// <summary>
        /// 请求标识（request-id），用于请求与响应的匹配。
        /// </summary>
        public int RequestId { get; }

        /// <summary>
        /// 协议级别的错误状态（error-status），适用于 v1/v2c。
        /// 默认值为 <see cref="SnmpErrorStatus.NoError"/>（如果使用构造函数未显式设置则由构造函数初始化）。
        /// </summary>
        public SnmpErrorStatus ErrorStatus { get; }

        /// <summary>
        /// 错误索引（error-index），指示出错的 varbind（从 1 开始），0 表示无特定索引。
        /// </summary>
        public int ErrorIndex { get; }

        /// <summary>
        /// 变量绑定序列（VarBind 列表），顺序重要。通常包含若干个 <see cref="SnmpVarBind"/> 项。
        /// 该容器为可变集合类型（<see cref="ValueOrList{T}"/>），便于在解析或构造 PDU 时就地追加元素而尽量减少堆分配。
        /// </summary>
        public ValueOrList<SnmpVarBind> VarBinds { get; }

        public bool IsEmpty => VarBinds == null;

        public SnmpPdu(SnmpErrorStatus errorStatus, int errorIndex, int requestId) : this()
        {
            PduType = SnmpPduType.Report;
            RequestId = requestId;
            ErrorStatus = errorStatus;
            ErrorIndex = errorIndex;
        }

        public SnmpPdu(SnmpPduType pduType) : this(pduType, RandomLazy.Value.Next(1, int.MaxValue))
        {

        }

        /// <summary>
        /// 使用指定 PDU 类型与 requestId 创建 PDU 实例。
        /// 构造后 <see cref="VarBinds"/> 已可用于追加 VarBind。
        /// </summary>
        /// <param name="pduType">PDU 类型。</param>
        /// <param name="requestId">请求标识。</param>
        public SnmpPdu(SnmpPduType pduType, int requestId) : this()
        {
            PduType = pduType;
            RequestId = requestId;
            ErrorStatus = SnmpErrorStatus.NoError;
            ErrorIndex = 0;
        }

        /// <summary>
        /// 参数less 构造函数：初始化内部 VarBinds 容器。
        /// </summary>
        /// <remarks>
        /// - 保留默认构造以便在某些序列化/解析场景或数组初始化时使用。
        /// - 若需设置其它字段（PduType/RequestId/…），请使用带参构造函数或直接初始化属性（视场景而定）。
        /// </remarks>
        public SnmpPdu()
        {
            VarBinds = new();
        }

        public void Dispose()
        {
            if (VarBinds.Count > 0)
            {
                foreach (var vb in VarBinds)
                {
                    vb.Value.Dispose();
                }
            }
        }

        internal void Encode(ref ByteBlock block)
        {
            ByteBlock valueBlock = new();
            BEREncoding.Encode(ref valueBlock, RequestId);
            BEREncoding.Encode(ref valueBlock, (int)ErrorStatus);
            BEREncoding.Encode(ref valueBlock, ErrorIndex);

            for (var i = 0; i < VarBinds.Count; i++)
            {
                VarBinds[i].Encode(ref valueBlock);
            }

            BEREncoding.EncodeSequence(ref block, valueBlock);
            valueBlock.Dispose();
        }
    }
}
