using ExtenderApp.Common.Encodings;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// 轻量的 OID 表示（包含标准 OID、推断的数据类型与可选友好名）。
    /// </summary>
    public readonly struct SnmpOid
    {
        #region 常用基础 OID

        /// <summary>
        /// sysDescr.0 - 系统描述
        /// </summary>
        public static readonly SnmpOid SysDescr = new("1.3.6.1.2.1.1.1.0", SnmpDataType.OctetString, "sysDescr.0");

        /// <summary>
        /// sysObjectID.0 - 系统对象ID
        /// </summary>
        public static readonly SnmpOid SysObjectID = new("1.3.6.1.2.1.1.2.0", SnmpDataType.ObjectIdentifier, "sysObjectID.0");

        /// <summary>
        /// sysUpTime.0 - 系统运行时间（TimeTicks）
        /// </summary>
        public static readonly SnmpOid SysUpTime = new("1.3.6.1.2.1.1.3.0", SnmpDataType.TimeTicks, "sysUpTime.0");

        /// <summary>
        /// sysContact.0 - 系统管理员
        /// </summary>
        public static readonly SnmpOid SysContact = new("1.3.6.1.2.1.1.4.0", SnmpDataType.OctetString, "sysContact.0");

        /// <summary>
        /// sysName.0 - 系统名称
        /// </summary>
        public static readonly SnmpOid SysName = new("1.3.6.1.2.1.1.5.0", SnmpDataType.OctetString, "sysName.0");

        /// <summary>
        /// sysLocation.0 - 系统位置
        /// </summary>
        public static readonly SnmpOid SysLocation = new("1.3.6.1.2.1.1.6.0", SnmpDataType.OctetString, "sysLocation.0");

        /// <summary>
        /// sysServices.0 - 系统服务位掩码
        /// </summary>
        public static readonly SnmpOid SysServices = new("1.3.6.1.2.1.1.7.0", SnmpDataType.Integer, "sysServices.0");

        /// <summary>
        /// ifNumber.0 - 接口数量
        /// </summary>
        public static readonly SnmpOid IfNumber = new("1.3.6.1.2.1.2.1.0", SnmpDataType.Integer, "ifNumber.0");

        /// <summary>
        /// ifDescr - 接口描述（列基，后接索引）
        /// </summary>
        public static readonly SnmpOid IfDescr = new("1.3.6.1.2.1.2.2.1.2", SnmpDataType.OctetString, "ifDescr");

        /// <summary>
        /// ifType - 接口类型（列基）
        /// </summary>
        public static readonly SnmpOid IfType = new("1.3.6.1.2.1.2.2.1.3", SnmpDataType.Integer, "ifType");

        /// <summary>
        /// ifMtu - 接口 MTU（列基）
        /// </summary>
        public static readonly SnmpOid IfMtu = new("1.3.6.1.2.1.2.2.1.4", SnmpDataType.Integer, "ifMtu");

        /// <summary>
        /// ifSpeed - 接口速率（列基，Gauge32）
        /// </summary>
        public static readonly SnmpOid IfSpeed = new("1.3.6.1.2.1.2.2.1.5", SnmpDataType.Gauge32, "ifSpeed");

        /// <summary>
        /// ifPhysAddress - 接口物理地址（MAC）（列基）
        /// </summary>
        public static readonly SnmpOid IfPhysAddress = new("1.3.6.1.2.1.2.2.1.6", SnmpDataType.OctetString, "ifPhysAddress");

        /// <summary>
        /// ifAdminStatus - 接口管理状态（列基）
        /// </summary>
        public static readonly SnmpOid IfAdminStatus = new("1.3.6.1.2.1.2.2.1.7", SnmpDataType.Integer, "ifAdminStatus");

        /// <summary>
        /// ifOperStatus - 接口运行状态（列基）
        /// </summary>
        public static readonly SnmpOid IfOperStatus = new("1.3.6.1.2.1.2.2.1.8", SnmpDataType.Integer, "ifOperStatus");

        /// <summary>
        /// ipAdEntAddr - IP 地址表项（列基，后接地址索引）
        /// </summary>
        public static readonly SnmpOid IpAdEntAddr = new("1.3.6.1.2.1.4.20.1.1", SnmpDataType.IpAddress, "ipAdEntAddr");

        /// <summary>
        /// tcpConnState - TCP 连接状态（列基）
        /// </summary>
        public static readonly SnmpOid TcpConnState = new("1.3.6.1.2.1.6.13.1.1", SnmpDataType.Integer, "tcpConnState");

        /// <summary>
        /// ifHCInOctets - 高容量接口输入字节（Counter64）
        /// </summary>
        public static readonly SnmpOid IfHCInOctets = new("1.3.6.1.2.1.31.1.1.1.6", SnmpDataType.Counter64, "ifHCInOctets");

        /// <summary>
        /// ifHCOutOctets - 高容量接口输出字节（Counter64）
        /// </summary>
        public static readonly SnmpOid IfHCOutOctets = new("1.3.6.1.2.1.31.1.1.1.10", SnmpDataType.Counter64, "ifHCOutOctets");

        /// <summary>
        /// snmpTrapOID - Trap OID（用于通知中的 trap 对象标识）
        /// </summary>
        public static readonly SnmpOid SnmpTrapOID = new("1.3.6.1.6.3.1.1.4.1", SnmpDataType.ObjectIdentifier, "snmpTrapOID");

        /// <summary>
        /// enterprises - 企业 OID 基址（厂商私有，从这里扩展）
        /// </summary>
        public static readonly SnmpOid Enterprises = new("1.3.6.1.4.1", SnmpDataType.ObjectIdentifier, "enterprises");

        #endregion 常用基础 OID

        /// <summary>
        /// OID 的点分字符串表示（例如 "1.3.6.1.2.1.1.1.0"）。
        /// </summary>
        public string Oid { get; }

        /// <summary>
        /// 根据经验推断或指定的 SNMP 数据类型（用于解析/格式化值时的提示）。
        /// 注意：具体值的实际 ASN.1 tag 以 Agent 返回为准，本字段仅作便捷提示。
        /// </summary>
        public SnmpDataType DataType { get; }

        /// <summary>
        /// 可选的友好名称（例如 "sysDescr.0"）。用于日志或 UI 展示，非必须。
        /// </summary>
        public string? FriendlyName { get; }

        /// <summary>
        /// 创建一个新的 <see cref="SnmpOid"/> 实例。
        /// </summary>
        /// <param name="id">点分 OID 字符串，不能为空或空白。</param>
        /// <param name="dataType">
        /// 可选的数据类型提示，默认 <see cref="SnmpDataType.OctetString"/>。
        /// </param>
        /// <param name="friendlyName">可选友好名，用于展示。</param>
        public SnmpOid(string id, SnmpDataType dataType = SnmpDataType.OctetString, string? friendlyName = null)
        {
            Oid = id ?? throw new ArgumentNullException(nameof(id));
            if (!CheckOid(Oid))
                throw new ArgumentException("OID 格式非法", nameof(id));
            DataType = dataType;
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
        /// <returns>如果格式满足要求则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="oid"/> 为空字符串时抛出。</exception>
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

        /// <summary>
        /// 将当前 OID 按 BER/ASN.1 的 OBJECT IDENTIFIER 编码并写入指定的 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="block">目标字节块，编码的 TLV（Tag + Length + Content）将追加写入该块。</param>
        /// <remarks>
        /// 编码实现说明（零堆分配）：
        /// - 使用 <see cref="ReadOnlySpan{T}"/> 对 <see cref="Oid"/> 进行两遍扫描（第一次计算 Content 长度，第二次写入实际 Content），避免临时分配；
        /// - 首个内容字节按规范合并为 <c>40 * first + second</c>；从第三个分量开始使用 base-128（7 位）分组编码，高位先写出，非最后字节设置 0x80；
        /// - base-128 编码的临时缓冲使用 <c>stackalloc</c>（长度 10 对应 64 位整数的最大 7-bit 分组数），保证不产生托管堆分配；
        /// - 长度字段使用项目中的 <see cref="BEREncoding.EncodeLength(ref ByteBlock, int)"/> 写入（支持短/长格式）；
        /// - 输入校验：遇到非数字字符、分量不足等情况会抛出 <see cref="ArgumentException"/>。
        /// </remarks>
        /// <exception cref="ArgumentException">OID 为空、含非法字符或分量少于 2 时抛出。</exception>
        internal void Encode(ref ByteBlock block)
        {
            ReadOnlySpan<char> s = Oid.AsSpan();
            if (s.Length == 0) throw new ArgumentException("OID 不能为空");

            // 1) 计算 components 数量并估算内容长度（不包含 Tag/Length）
            int pos = 0;
            int compIndex = 0;
            ulong first = 0, second = 0;
            int contentLen = 0;
            while (pos < s.Length)
            {
                ulong v = 0;
                while (pos < s.Length && s[pos] != '.')
                {
                    char c = s[pos++];
                    if (c < '0' || c > '9')
                        throw new ArgumentException("OID 含非法字符");

                    v = v * 10 + (ulong)(c - '0');
                }

                if (compIndex == 0) first = v;
                else if (compIndex == 1) second = v;
                else
                {
                    // 计算 base-128 所需字节数
                    if (v == 0) contentLen += 1;
                    else
                    {
                        ulong tv = v;
                        while (tv != 0)
                        {
                            contentLen++;
                            tv >>= 7;
                        }
                    }
                }
                compIndex++;
                if (pos < s.Length && s[pos] == '.') pos++;
            }

            if (compIndex < 2) throw new ArgumentException("OID 至少两个分量");

            // 第一个字节为 40*first + second
            contentLen += 1;

            // 2) 写 Tag (OBJECT IDENTIFIER = 0x06) 与 Length（使用 BEREncoding 的写入函数）
            block.Write((byte)SnmpDataType.ObjectIdentifier); // tag 0x06
            BEREncoding.EncodeLength(ref block, contentLen);

            // 3) 再次扫描并写入内容：先写合并的第一个字节，然后写其余分量的 base-128 编码
            block.Write((byte)(checked((int)(first * 40 + second))));

            pos = 0;
            compIndex = 0;
            Span<byte> tmp = stackalloc byte[10];
            while (pos < s.Length)
            {
                ulong v = 0;
                while (pos < s.Length && s[pos] != '.')
                {
                    v = v * 10 + (ulong)(s[pos++] - '0');
                }

                if (compIndex >= 2)
                {
                    int tlen = 0;
                    if (v == 0)
                    {
                        tmp[0] = 0;
                        tlen = 1;
                    }
                    else
                    {
                        while (v != 0)
                        {
                            tmp[tlen++] = (byte)(v & 0x7F);
                            v >>= 7;
                        }
                    }

                    for (int i = tlen - 1; i >= 0; i--)
                    {
                        byte b = tmp[i];
                        if (i != 0) b |= 0x80;
                        block.Write(b);
                    }
                }

                compIndex++;
                if (pos < s.Length && s[pos] == '.') pos++;
            }
        }

        public static implicit operator SnmpOid(string oidString) => new(oidString);

        public static implicit operator string(SnmpOid snmpOid) => snmpOid.Oid;

        public static implicit operator System.Security.Cryptography.Oid(SnmpOid snmpOid)
            => new System.Security.Cryptography.Oid(snmpOid.Oid, snmpOid.FriendlyName);

        public static implicit operator SnmpOid(System.Security.Cryptography.Oid oid)
            => new SnmpOid(oid.Value ?? throw new ArgumentNullException(nameof(oid.Value)), SnmpDataType.ObjectIdentifier, oid.FriendlyName);
    }
}