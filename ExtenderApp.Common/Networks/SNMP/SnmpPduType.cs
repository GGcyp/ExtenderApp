namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// SNMP PDU 类型（常用子集）。
    /// 每个枚举值对应 PDU 的上下文特定 tag number（在 BER 中常作为 context-specific, constructed）。
    /// 备注：GetBulk 仅在 SNMPv2c/ v3 中可用；TrapV1 为 SNMPv1 专用；Report 多用于 SNMPv3 报告（安全/通信错误）。
    /// </summary>
    public enum SnmpPduType : byte
    {
        /// <summary>
        /// GetRequest (tag = 0)。
        /// Manager → Agent：请求一个或多个对象的当前值，Agent 返回 GetResponse。
        /// 常用于轮询读取单个或少量 OID。
        /// </summary>
        GetRequest = 0,

        /// <summary>
        /// GetNextRequest (tag = 1)。
        /// Manager → Agent：请求在 MIB 树中紧随指定 OID 之后的下一个对象（用于遍历/WALK）。
        /// 可用于逐步遍历表项，兼容早期 SNMP 版本。
        /// </summary>
        GetNextRequest = 1,

        /// <summary>
        /// GetResponse (tag = 2)。
        /// Agent → Manager：对 GetRequest/GetNext/Set 的应答，包含 request-id、error-status、error-index 和 varbind 列表。
        /// </summary>
        GetResponse = 2,

        /// <summary>
        /// SetRequest (tag = 3)。
        /// Manager → Agent：写入或修改 Agent 上指定对象的值（需具备写权限）。
        /// Agent 用 GetResponse 返回操作结果或错误码。
        /// </summary>
        SetRequest = 3,

        /// <summary>
        /// TrapV1 (tag = 4)。
        /// Agent → Manager：SNMPv1 的非确认异步通知（Trap），由 Agent 主动发送到管理站的 UDP 162 端口。
        /// 与 v2/v3 的 Trap/Inform 不完全兼容（字段不同）。
        /// </summary>
        TrapV1 = 4,

        /// <summary>
        /// GetBulkRequest (tag = 5)。
        /// Manager → Agent（SNMPv2c/v3）：用于高效一次性获取大量表格数据（bulk retrieval），由 GetNext 的扩展演变而来。
        /// 通过 non-repeaters 和 max-repetitions 控制返回量。
        /// </summary>
        GetBulkRequest = 5,

        /// <summary>
        /// InformRequest (tag = 6)。
        /// Agent → Manager（或 Manager ↔ Manager）：类似 Trap，但带确认机制（可靠传递）。
        /// 发送方会等待对方的 GetResponse 样式确认。
        /// </summary>
        InformRequest = 6,

        /// <summary>
        /// TrapV2 (tag = 7)。
        /// Agent → Manager：SNMPv2/v3 的非确认通知（SNMPv2-Trap），采用与 GetResponse 相同的 PDU 结构风格（但 tag 为 context-specific）。
        /// 与 InformRequest 的区别在于可靠性（Inform 会确认）。
        /// </summary>
        TrapV2 = 7,

        /// <summary>
        /// Report (tag = 8)。
        /// Manager/Agent：用于 SNMPv3 中传递错误/报告消息（例如安全或处理报告），可用于诊断与错误通知。
        /// </summary>
        Report = 8
    }
}
