
using ExtenderApp.Common.Encodings;

namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// 通用的 SNMP 值类型标识（对应 ASN.1/BER 的 Tag）。
    /// 注：枚举值使用的是包含类别位的 Tag 值（例如 Application 类的 IpAddress = 0x40）。
    /// 在编码/解码时应注意：Tag 字节还包含 Class 和 Constructed 标志 (高位两位为 Class，第6位为 Constructed)。
    /// </summary>
    public enum SnmpDataType : byte
    {
        /// <summary>
        /// ASN.1 INTEGER (Tag = 0x02)。
        /// 用于表示有符号整数（例如 SNMP 的 Integer、error-status、request-id 等）。
        /// 解码时需要按最短补码形式恢复为有符号值。
        /// </summary>
        Integer = BEREncoding.IntegerTag,

        /// <summary>
        /// ASN.1 OCTET STRING (Tag = 0x04)。
        /// 任意字节序列，通常可视为文本（ASCII/UTF-8）或二进制数据（MAC、二进制 blob 等）。
        /// </summary>
        OctetString = BEREncoding.OctetStringTag,

        /// <summary>
        /// ASN.1 NULL (Tag = 0x05)。
        /// 表示空值（常见于 GET 请求中用作占位，或表示被管理对象不存在/无值）。
        /// </summary>
        Null = BEREncoding.NullTag,

        /// <summary>
        /// ASN.1 OBJECT IDENTIFIER (Tag = 0x06)。
        /// 表示 OID（点分数字表示），用于 VarBind 的对象标识部分或对象值（如 snmpTrapOID）。
        /// </summary>
        ObjectIdentifier = BEREncoding.ObjectIdentifierTag,

        /// <summary>
        /// ASN.1 SEQUENCE / SEQUENCE OF (Tag Number = 0x10)。
        /// 作为构造类型使用（外层 Tag 字节通常为 0x30，表示 Universal + Constructed + TagNumber=16）。
        /// 在 SNMP 中用于封装 PDU 的组成部分和 varbind 列表等。
        /// </summary>
        Sequence = BEREncoding.SequenceTag,

        /// <summary>
        /// Application class: IpAddress (Tag = 0x40)。
        /// 表示 IPv4 地址，通常以 4 个字节编码（例如 192.0.2.1）。
        /// 注意：这是 Application 类标签，不是 Universal。
        /// </summary>
        IpAddress = 0x40, // application

        /// <summary>
        /// Application class: Counter32 (Tag = 0x41)。
        /// 无符号 32 位计数器（递增），常用于接口字节计数等。
        /// </summary>
        Counter32 = 0x41,

        /// <summary>
        /// Application class: Gauge32 (Tag = 0x42)。
        /// 无符号 32 位的 Gauge/Scalar 值（可上下波动），例如当前资源使用量。
        /// </summary>
        Gauge32 = 0x42,

        /// <summary>
        /// Application class: TimeTicks (Tag = 0x43)。
        /// 表示自某个时间点以来的百毫秒(ticks)计数，常用于 sysUpTime 等。
        /// </summary>
        TimeTicks = 0x43,

        /// <summary>
        /// Application class: Opaque (Tag = 0x44)。
        /// 可由实现定义的二进制封装，通常用于扩展或厂商特定数据。
        /// </summary>
        Opaque = 0x44,

        /// <summary>
        /// Application class: Counter64 (Tag = 0x46)。
        /// 无符号 64 位计数器，用于高精度/大流量计数（ifHCInOctets/ifHCOutOctets）。
        /// </summary>
        Counter64 = 0x46
    }
}