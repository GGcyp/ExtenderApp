using System.Formats.Asn1;
using ExtenderApp.Common.Encodings;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// 表示一个 SNMP PDU（Protocol Data Unit），如
    /// GetRequest/GetResponse/SetRequest 等。
    /// 该类型为值类型（readonly struct），承载 PDU 的元信息与
    /// varbind 列表（变量绑定序列）。
    /// </summary>
    /// <remarks>
    /// - PDU 中的 varbind 列表使用 <see
    ///   cref="ValueOrList{T}"/> 以减少短期分配开销；
    /// 在构造/解析 PDU 时可直接向 <see cref="VarBinds"/> 添加项。
    /// - 因为为 readonly
    /// struct，实例成员在构造时应一次性初始化；复制该结构体会拷贝 <see
    /// cref="ValueOrList{T}"/> 的引用语义，
    /// 使用时应注意不要对共享底层资源重复释放（如 <see
    /// cref="SnmpValue"/> 内部持有的缓冲需由单一所有者管理）。
    /// </remarks>
    public readonly struct SnmpPdu : IDisposable
    {
        private static Lazy<Random> RandomLazy = new();

        /// <summary>
        /// PDU 类型（例如 <see
        /// cref="SnmpPduType.GetRequest"/>、 <see
        /// cref="SnmpPduType.GetResponse"/> 等）。
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
        /// 错误索引（error-index），指示出错的 varbind（从 1
        /// 开始），0 表示无特定索引。
        /// </summary>
        public int ErrorIndex { get; }

        /// <summary>
        /// 变量绑定序列（VarBind 列表），顺序重要。通常包含若干个 <see
        /// cref="SnmpVarBind"/> 项。 该容器为可变集合类型（
        /// <see cref="ValueOrList{T}"/>），便于在解析或构造
        /// PDU 时就地追加元素而尽量减少堆分配。
        /// </summary>
        public ValueOrList<SnmpVarBind> VarBinds { get; }

        public bool IsEmpty => VarBinds == null;

        public SnmpPdu(int requestId, SnmpErrorStatus errorStatus, int errorIndex) : this()
        {
            PduType = SnmpPduType.Report;
            RequestId = requestId;
            ErrorStatus = errorStatus;
            ErrorIndex = errorIndex;
            VarBinds = new();
        }

        public SnmpPdu(SnmpPduType pduType) : this(pduType, RandomLazy.Value.Next(1, int.MaxValue))
        {
        }

        /// <summary>
        /// 使用指定 PDU 类型与 requestId 创建 PDU 实例。 构造后
        /// <see cref="VarBinds"/> 已可用于追加 VarBind。
        /// </summary>
        /// <param name="pduType">PDU 类型。</param>
        /// <param name="requestId">请求标识。</param>
        public SnmpPdu(SnmpPduType pduType, int requestId) : this()
        {
            PduType = pduType;
            RequestId = requestId;
            ErrorStatus = SnmpErrorStatus.NoError;
            ErrorIndex = 0;
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

        /// <summary>
        /// 将当前 PDU 按 BER/ASN.1 的 SEQUENCE 格式编码并追加写入目标 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="block">目标字节块（以引用方式传入）。编码的完整 PDU TLV（外层 SEQUENCE）将追加到该块末尾。</param>
        /// <remarks>
        /// 实现要点：
        /// - 按顺序编码 request-id、error-status、error-index（使用 <see cref="BEREncoding.EncodeInteger(ref ByteBlock, long)"/>）；
        /// - 依次对 <see cref="VarBinds"/> 中的每个 <see cref="SnmpVarBind"/> 调用其 <see cref="SnmpVarBind.Encode(ref ByteBlock)"/> 写入；
        /// - 使用临时 <see cref="ByteBlock"/> 组装内层内容，最后通过 <see cref="BEREncoding.EncodeSequence(ref ByteBlock, ByteBlock)"/> 写出外层 SEQUENCE 并释放临时块；
        /// - 任何编码过程中抛出的异常会向上传播给调用方。
        /// </remarks>
        internal void Encode(ref ByteBlock block)
        {
            //ByteBlock valueBlock = new();
            //BEREncoding.EncodeInteger(ref valueBlock, RequestId);
            //BEREncoding.EncodeInteger(ref valueBlock, (int)ErrorStatus);
            //BEREncoding.EncodeInteger(ref valueBlock, ErrorIndex);

            //ByteBlock varBindsBlock = new();
            //if (VarBinds != null)
            //{
            //    for (var i = 0; i < VarBinds.Count; i++)
            //    {
            //        VarBinds[i].Encode(ref varBindsBlock);
            //    }
            //}
            //BEREncoding.EncodeSequence(ref valueBlock, varBindsBlock);

            //BEREncoding.EncodeTag(ref block, BEREncoding.ApplicationClass, true, (int)PduType);
            //BEREncoding.EncodeLength(ref block, valueBlock.Remaining);
            //block.Write(valueBlock);

            //valueBlock.Dispose();
            //varBindsBlock.Dispose();
        }

        /// <summary>
        /// 尝试从 <see cref="ByteBlock"/> 的当前位置解析并构造一个 PDU。
        /// </summary>
        /// <param name="block">
        /// 待解析的字节块（以引用方式传入）。当方法返回 <c>true</c> 时，块的读取位置已推进并消费对应的 PDU SEQUENCE TLV（Tag+Length+Value）。
        /// 若方法返回 <c>false</c>，表示当前位置不包含一个完整的 PDU（通常不会推进读取位置）。
        /// </param>
        /// <param name="snmpPdu">解析成功时输出解析得到的 <see cref="SnmpPdu"/> 实例；失败时为默认值。</param>
        /// <returns>成功解析并构造 PDU 返回 <c>true</c>；当前位置不是 PDU 或前置检查失败则返回 <c>false</c>。</returns>
        /// <exception cref="System.IO.InvalidDataException">
        /// 当检测到 SEQUENCE 存在但其长度或内部编码不完整，或整数解码/子项解码出现不合法数据时抛出。
        /// </exception>
        /// <remarks>
        /// - 解析流程：先通过 <see cref="BEREncoding.TryDecodeSequence(ref ByteBlock)"/> 进入 SEQUENCE 内容； 
        ///   随后依次解析 request-id、error-status、error-index（三者均通过 <see cref="BEREncoding.DecodeInteger(ref ByteBlock)"/> 获取）；
        /// - 根据解析到的 error-status 构造初始 <see cref="SnmpPdu"/>，然后循环尝试解析 VarBind（调用 <see cref="SnmpVarBind.TryDecode(ref ByteBlock, out SnmpVarBind)"/>）直至失败；
        /// - 若任一子项在确定存在后格式不合法，将抛出异常；若当前位置根本不是 PDU，则方法返回 <c>false</c>。
        /// </remarks>
        internal static bool TryDecode(ref ByteBlock block, out SnmpPdu snmpPdu)
        {
            snmpPdu = default;
            if (!BEREncoding.TryDecodeSequence(ref block))
                return false;

            int requesId = (int)BEREncoding.DecodeInteger(ref block);
            SnmpErrorStatus status = (SnmpErrorStatus)BEREncoding.DecodeInteger(ref block);
            int errorIndex = (int)BEREncoding.DecodeInteger(ref block);

            if (status != SnmpErrorStatus.NoError)
            {
                snmpPdu = new SnmpPdu(status, errorIndex, requesId);
            }
            else
            {
                snmpPdu = new(SnmpPduType.GetResponse, requesId);
            }


            if (!BEREncoding.TryDecodeSequence(ref block))
                return false;
            while (true)
            {
                if (SnmpVarBind.TryDecode(ref block, out SnmpVarBind varBind))
                {
                    snmpPdu.VarBinds.Add(varBind);
                }
                else
                {
                    break;
                }
            }

            return true;
        }
    }
}