using ExtenderApp.Common.Encodings;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// 表示一个 VarBind（OID + Value），对应 SNMP 中的单个变量绑定项。
    /// </summary>
    public struct SnmpVarBind : IDisposable
    {
        /// <summary>
        /// VarBind 的对象标识（OID），使用 <see cref="SnmpOid"/> 表示（点分字符串与可选友好名）。
        /// </summary>
        public SnmpOid Oid { get; set; }

        /// <summary>
        /// 变量的值封装（根据 <see cref="SnmpValue.Type"/> 表示实际 ASN.1/BER 类型）。
        /// </summary>
        public SnmpValue Value { get; set; }

        public bool IsEmpty => string.IsNullOrEmpty(Oid);

        /// <summary>
        /// 默认构造函数，创建一个空的 VarBind（调用方应随后设置 <see cref="Oid"/> 与 <see cref="Value"/>）。
        /// </summary>
        public SnmpVarBind()
        {
        }

        public SnmpVarBind(string oid) : this(new SnmpOid(oid), SnmpValue.Empty)
        {
        }

        /// <summary>
        /// 使用 OID 字符串与值创建 VarBind。
        /// </summary>
        /// <param name="oid">点分 OID 字符串（例如 "1.3.6.1.2.1.1.1.0"）。</param>
        /// <param name="value">对应的 <see cref="SnmpValue"/> 值。</param>
        public SnmpVarBind(string oid, SnmpValue value) : this(new SnmpOid(oid), value)
        {
        }

        public SnmpVarBind(SnmpOid oid) : this(oid, SnmpValue.Empty)
        {

        }

        /// <summary>
        /// 使用 <see cref="SnmpOid"/> 与 <see cref="SnmpValue"/> 创建 VarBind 实例。
        /// </summary>
        /// <param name="oid">预构造的 <see cref="SnmpOid"/> 实例。</param>
        /// <param name="value">对应的 <see cref="SnmpValue"/> 值。</param>
        public SnmpVarBind(SnmpOid oid, SnmpValue value)
        {
            Oid = oid;
            Value = value;
        }

        public void Dispose()
        {
            Value.Dispose();
        }

        /// <summary>
        /// 尝试从 <see cref="ByteBlock"/> 的当前位置解析并构造一个 VarBind（OID + Value）。
        /// </summary>
        /// <param name="block">
        /// 待解析的字节块（以引用方式传入）。当方法返回 <c>true</c> 时，块的读取位置已推进并消费对应的 SEQUENCE TLV（Tag+Length+Value）。
        /// 若方法返回 <c>false</c>，表示当前位置不包含一个完整的 VarBind（通常不会推进或仅在确认存在 SEQUENCE 且长度合理后推进到内容起始处）。
        /// </param>
        /// <param name="bind">解析成功时输出解析得到的 <see cref="SnmpVarBind"/> 实例；失败时为默认值。</param>
        /// <returns>
        /// 成功解析并构造 VarBind 返回 <c>true</c>；当前位置不是 VarBind（或解析前置检查失败）则返回 <c>false</c>。
        /// </returns>
        /// <exception cref="System.IO.InvalidDataException">
        /// 若检测到 SEQUENCE 存在但其长度或内部编码不完整（例如 OID 或 Value 的 base-128 编码不合法或数据不足），方法会抛出该异常。
        /// </exception>
        /// <remarks>
        /// - 方法流程：先通过 <see cref="BEREncoding.TryDecodeSequence(ref ByteBlock)"/> 确认并进入 SEQUENCE 内容，随后依次调用
        ///   <see cref="SnmpOid.TryDecode(ref ByteBlock, out SnmpOid)"/> 与 <see cref="SnmpValue.TryDecode(ref ByteBlock, out SnmpValue)"/>。
        /// - 若任一子项解析失败且该子项已被确认为存在但格式错误，会抛出异常；若子项根本不存在（非 VarBind 情形），方法返回 <c>false</c>。
        /// </remarks>
        internal static bool TryDecode(ref ByteBlock block, out SnmpVarBind bind)
        {
            bind = default;
            if (block.IsEmpty)
                return false;

            // 尝试解析 Sequence
            if (!BEREncoding.TryDecodeSequence(ref block))
                return false;

            // 解析 OID
            if (!SnmpOid.TryDecode(ref block, out var oid))
                return false;

            // 解析 Value
            if (!SnmpValue.TryDecode(ref block, out var value))
                return false;

            bind = new(oid, value);
            return true;
        }
    }
}
