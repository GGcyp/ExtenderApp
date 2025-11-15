using ExtenderApp.Common.Encodings;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// 表示一个完整的 SNMP 消息（包含 version、community、pdu）。
    /// 该类型为值类型，包装 PDU 并负责在需要时释放其资源。
    /// </summary>
    public struct SnmpMessage : IDisposable
    {
        /// <summary>
        /// SNMP 协议版本（例如 V1/V2c/V3 等）。
        /// </summary>
        public SnmpVersionType Version { get; set; }

        /// <summary>
        /// SNMP community 字符串（通常为 "public" 或自定义团体名）。
        /// </summary>
        public string Community { get; set; }

        /// <summary>
        /// 包含变量绑定列表和 PDU 元信息的 PDU 实例。
        /// </summary>
        public SnmpPdu Pdu { get; set; }

        /// <summary>
        /// 使用指定 PDU 创建一个 SNMP 消息实例。
        /// </summary>
        /// <param name="pdu">要封装的 PDU。</param>
        /// <param name="version">SNMP 版本，默认 <see cref="SnmpVersionType.V2c"/>。</param>
        /// <param name="community">community 字符串，默认 "public"。</param>
        public SnmpMessage(SnmpPdu pdu, SnmpVersionType version = SnmpVersionType.V2c, string community = "public")
        {
            Pdu = pdu;
            Version = version;
            Community = community;
        }

        /// <summary>
        /// 释放 SNMP 消息持有的可释放资源（转发到内部 PDU 的释放实现）。
        /// </summary>
        public void Dispose()
        {
            Pdu.Dispose();
        }
    }
}