namespace ExtenderApp.Common.Networks.SNMP
{
    public enum SnmpPduType : byte
    {
        /// <summary>
        /// GetRequest：应用类+构造类型+标签号0 → 0x40(应用类) + 0x20(构造类型) + 0x00(标签号) = 0x60
        /// </summary>
        GetRequest = 0x00,

        /// <summary>
        /// GetNextRequest：应用类+构造类型+标签号1 → 0x40 + 0x20 + 0x01 = 0x61
        /// </summary>
        GetNextRequest = 0x01,

        /// <summary>
        /// GetResponse：应用类+构造类型+标签号2 → 0x40 + 0x20 + 0x02 = 0x62
        /// </summary>
        GetResponse = 0x02,

        /// <summary>
        /// SetRequest：应用类+构造类型+标签号3 → 0x40 + 0x20 + 0x03 = 0x63
        /// </summary>
        SetRequest = 0x03,

        /// <summary>
        /// TrapV1：应用类+构造类型+标签号4 → 0x40 + 0x20 + 0x04 = 0x64
        /// </summary>
        TrapV1 = 0x04,

        /// <summary>
        /// GetBulkRequest：应用类+构造类型+标签号5 → 0x40 + 0x20 + 0x05 = 0x65
        /// </summary>
        GetBulkRequest = 0x05,

        /// <summary>
        /// InformRequest：应用类+构造类型+标签号6 → 0x40 + 0x20 + 0x06 = 0x66
        /// </summary>
        InformRequest = 0x06,

        /// <summary>
        /// TrapV2：上下文特定类+构造类型+标签号0 → 0x80(上下文特定类) + 0x20(构造类型) + 0x00(标签号) = 0xA0
        /// （SNMPv2 Trap使用上下文特定类，区别于其他PDU）
        /// </summary>
        TrapV2 = 0x00,

        /// <summary>
        /// Report：应用类+构造类型+标签号8 → 0x40 + 0x20 + 0x08 = 0x68
        /// </summary>
        Report = 0x08
    }
}
