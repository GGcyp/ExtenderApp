namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// SNMP 协议版本。
    /// 枚举值对应 SNMP 消息中编码的整数（例如 BER 中 version 字段）。
    /// 使用时应注意不同版本在安全模型、PDU 支持与通知机制上的差异：
    /// - V1/V2c 使用 community 字符串作为简单认证机制（明文），不推荐在不安全网络中使用；
    /// - V2c 增加了 GetBulk 等效率改进，但安全性仍依赖 community；
    /// - V3 引入USM（用户安全模型），支持鉴权与加密，适用于生产环境。
    /// </summary>
    public enum SnmpVersionType : int
    {
        /// <summary>
        /// SNMPv1（值 = 0）。
        /// - 最早的 SNMP 版本，使用 community 字符串进行明文“身份验证”；
        /// - 支持的 PDU 包括 GetRequest/GetNext/Set/Trap (v1 Trap) 等；
        /// - 不支持 GetBulk、Inform，也不提供强认证或加密；仅在兼容老设备时使用。
        /// </summary>
        V1 = 0,

        /// <summary>
        /// SNMPv2c（值 = 1）。
        /// - 基于 v1 的社区字符串模型（community），增加了协议效率（如 GetBulk）；
        /// - PDU 类型扩展（GetBulk、Inform 等），但安全性仍然依赖明文 community；
        /// - 常用于受限网络或对兼容性要求高但不需要强安全性的场景。
        /// </summary>
        V2c = 1,

        /// <summary>
        /// SNMPv3（值 = 3）。
        /// - 引入安全模型（USM）：支持用户鉴权（MD5/SHA）和报文加密（DES/AES 等）；
        /// - 提供对消息完整性、来源鉴别和机密性的支持，推荐在生产环境中使用；
        /// - PDU 功能包含 v2 的扩展，同时配合 v3 的安全与访问控制模型（VACM）。
        /// </summary>
        V3 = 3
    }
}
