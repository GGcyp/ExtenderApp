using ExtenderApp.Common.Encodings;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// 轻量的 OID 表示（包含标准 OID、推断的数据类型与可选友好名）。
    /// </summary>
    public readonly struct SnmpOid : IEquatable<SnmpOid>
    {
        #region 常用基础 OID

        /// <summary>
        /// sysDescr.0 - 系统描述
        /// </summary>
        public static readonly SnmpOid SysDescr = new("1.3.6.1.2.1.1.1.0", "sysDescr.0");

        /// <summary>
        /// sysObjectID.0 - 系统对象ID
        /// </summary>
        public static readonly SnmpOid SysObjectID = new("1.3.6.1.2.1.1.2.0", "sysObjectID.0");

        /// <summary>
        /// sysUpTime.0 - 系统运行时间（TimeTicks）
        /// </summary>
        public static readonly SnmpOid SysUpTime = new("1.3.6.1.2.1.1.3.0", "sysUpTime.0");

        /// <summary>
        /// sysContact.0 - 系统管理员
        /// </summary>
        public static readonly SnmpOid SysContact = new("1.3.6.1.2.1.1.4.0", "sysContact.0");

        /// <summary>
        /// sysName.0 - 系统名称
        /// </summary>
        public static readonly SnmpOid SysName = new("1.3.6.1.2.1.1.5.0", "sysName.0");

        /// <summary>
        /// sysLocation.0 - 系统位置
        /// </summary>
        public static readonly SnmpOid SysLocation = new("1.3.6.1.2.1.1.6.0", "sysLocation.0");

        /// <summary>
        /// sysServices.0 - 系统服务位掩码
        /// </summary>
        public static readonly SnmpOid SysServices = new("1.3.6.1.2.1.1.7.0", "sysServices.0");

        /// <summary>
        /// ifNumber.0 - 接口数量
        /// </summary>
        public static readonly SnmpOid IfNumber = new("1.3.6.1.2.1.2.1.0", "ifNumber.0");

        /// <summary>
        /// ifDescr - 接口描述（列基，后接索引）
        /// </summary>
        public static readonly SnmpOid IfDescr = new("1.3.6.1.2.1.2.2.1.2", "ifDescr");

        /// <summary>
        /// ifType - 接口类型（列基）
        /// </summary>
        public static readonly SnmpOid IfType = new("1.3.6.1.2.1.2.2.1.3", "ifType");

        /// <summary>
        /// ifMtu - 接口 MTU（列基）
        /// </summary>
        public static readonly SnmpOid IfMtu = new("1.3.6.1.2.1.2.2.1.4", "ifMtu");

        /// <summary>
        /// ifSpeed - 接口速率（列基，Gauge32）
        /// </summary>
        public static readonly SnmpOid IfSpeed = new("1.3.6.1.2.1.2.2.1.5", "ifSpeed");

        /// <summary>
        /// ifPhysAddress - 接口物理地址（MAC）（列基）
        /// </summary>
        public static readonly SnmpOid IfPhysAddress = new("1.3.6.1.2.1.2.2.1.6", "ifPhysAddress");

        /// <summary>
        /// ifAdminStatus - 接口管理状态（列基）
        /// </summary>
        public static readonly SnmpOid IfAdminStatus = new("1.3.6.1.2.1.2.2.1.7", "ifAdminStatus");

        /// <summary>
        /// ifOperStatus - 接口运行状态（列基）
        /// </summary>
        public static readonly SnmpOid IfOperStatus = new("1.3.6.1.2.1.2.2.1.8", "ifOperStatus");

        /// <summary>
        /// ipAdEntAddr - IP 地址表项（列基，后接地址索引）
        /// </summary>
        public static readonly SnmpOid IpAdEntAddr = new("1.3.6.1.2.1.4.20.1.1", "ipAdEntAddr");

        /// <summary>
        /// tcpConnState - TCP 连接状态（列基）
        /// </summary>
        public static readonly SnmpOid TcpConnState = new("1.3.6.1.2.1.6.13.1.1", "tcpConnState");

        /// <summary>
        /// ifHCInOctets - 高容量接口输入字节（Counter64）
        /// </summary>
        public static readonly SnmpOid IfHCInOctets = new("1.3.6.1.2.1.31.1.1.1.6", "ifHCInOctets");

        /// <summary>
        /// ifHCOutOctets - 高容量接口输出字节（Counter64）
        /// </summary>
        public static readonly SnmpOid IfHCOutOctets = new("1.3.6.1.2.1.31.1.1.1.10", "ifHCOutOctets");

        /// <summary>
        /// snmpTrapOID - Trap OID（用于通知中的 trap 对象标识）
        /// </summary>
        public static readonly SnmpOid SnmpTrapOID = new("1.3.6.1.6.3.1.1.4.1", "snmpTrapOID");

        /// <summary>
        /// enterprises - 企业 OID 基址（厂商私有，从这里扩展）
        /// </summary>
        public static readonly SnmpOid Enterprises = new("1.3.6.1.4.1", "enterprises");

        #endregion 常用基础 OID

        /// <summary>
        /// OID 的点分字符串表示（例如 "1.3.6.1.2.1.1.1.0"）。
        /// </summary>
        public string Oid { get; }

        /// <summary>
        /// 可选的友好名称（例如 "sysDescr.0"）。用于日志或 UI 展示，非必须。
        /// </summary>
        public string? FriendlyName { get; }

        /// <summary>
        /// 获取当前OID是否为空（即 <see cref="Oid"/> 为空或空字符串）。
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(Oid);

        /// <summary>
        /// 创建一个新的 <see cref="SnmpOid"/> 实例。
        /// </summary>
        /// <param name="id">点分 OID 字符串，不能为空或空白。</param>
        /// <param name="dataType">
        /// 可选的数据类型提示，默认 <see cref="SnmpDataType.OctetString"/>。
        /// </param>
        /// <param name="friendlyName">可选友好名，用于展示。</param>
        public SnmpOid(string id, string? friendlyName = null)
        {
            Oid = id ?? throw new ArgumentNullException(nameof(id));
            if (!CheckOid(Oid))
                throw new ArgumentException("OID 格式非法", nameof(id));
            FriendlyName = friendlyName;
        }

        /// <summary>
        /// 返回友好名（如果存在）否则返回原始 OID 字符串。
        /// </summary>
        /// <returns>友好名或点分 OID。</returns>
        public override string ToString() => FriendlyName ?? Oid;

        /// <summary>
        /// 验证 OID 字符串的基本语法：非空且仅由数字与 '.' 组成。
        /// </summary>
        /// <param name="oid">点分 OID 字符串，例如 <c>"1.3.6.1.2.1.1.1.0"</c>。</param>
        /// <returns>
        /// 如果格式满足要求则返回 <c>true</c>；否则返回 <c>false</c>。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="oid"/> 为空字符串时抛出。
        /// </exception>
        public static bool CheckOid(string oid)
        {
            ReadOnlySpan<char> s = oid.AsSpan();
            if (s.Length == 0) throw new ArgumentException("OID 不能为空");

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if ((c < '0' || c > '9') && c != '.')
                    return false;
            }
            return true;
        }

        public bool Equals(SnmpOid other)
        {
            if (other.IsEmpty)
                return false;

            return string.Equals(Oid, other.Oid, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator ==(SnmpOid left, SnmpOid right) => left.Equals(right);

        public static bool operator !=(SnmpOid left, SnmpOid right) => !left.Equals(right);

        public override bool Equals(object? obj)
        {
            return obj is SnmpOid && Equals((SnmpOid)obj);
        }

        public override int GetHashCode()
        {
            return Oid.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }

        public static implicit operator SnmpOid(string oidString) => new(oidString);

        public static implicit operator string(SnmpOid snmpOid) => snmpOid.Oid;

        public static implicit operator System.Security.Cryptography.Oid(SnmpOid snmpOid)
            => new System.Security.Cryptography.Oid(snmpOid.Oid, snmpOid.FriendlyName);

        public static implicit operator SnmpOid(System.Security.Cryptography.Oid oid)
            => new SnmpOid(oid.Value ?? throw new ArgumentNullException(nameof(oid.Value)), oid.FriendlyName);
    }
}