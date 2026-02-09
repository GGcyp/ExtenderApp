using ExtenderApp.Common.Encodings;
using ExtenderApp.Contracts;

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
    }
}
