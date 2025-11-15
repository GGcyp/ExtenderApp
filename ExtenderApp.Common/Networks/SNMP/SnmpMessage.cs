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

        /// <summary>
        /// 尝试从 <see cref="ByteBlock"/> 的当前位置解析一个 SNMP 消息（version/community/pdu）。
        /// </summary>
        /// <param name="block">
        /// 包含待解析数据的字节块引用。方法在成功解析后会推进该块的读取位置以消费对应的 SEQUENCE TLV（Tag+Length+Value）。
        /// </param>
        /// <param name="snmpMessage">解析成功时输出对应的 <see cref="SnmpMessage"/> 实例；失败或不为 SNMP 消息时为默认值。</param>
        /// <returns>
        /// 若当前位置为完整且有效的 SNMP Message TLV，则返回 <c>true</c> 并推进原始块的读取位置；否则返回 <c>false</c>（通常不推进读取位置）。
        /// </returns>
        /// <exception cref="ArgumentException">当传入的 <paramref name="block"/> 为空（无已写入数据）时抛出。</exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// 当检测到 Message TLV 存在但内部长度或子项（version/community/pdu）不完整或格式非法时抛出。
        /// </exception>
        public static bool TryDecode(ref ByteBlock block, out SnmpMessage snmpMessage)
        {
            if (block.IsEmpty)
                throw new ArgumentException("当前要解码字节块为空", nameof(block));

            snmpMessage = default;
            if (!BEREncoding.TryDecodeSequence(ref block))
                return false;

            SnmpVersionType versionType = (SnmpVersionType)BEREncoding.DecodeInteger(ref block);
            string community = BEREncoding.DecodeUtf8String(ref block);
            if (!SnmpPdu.TryDecode(ref block, out var pdu))
                return false;

            snmpMessage = new SnmpMessage(pdu, versionType, community);
            return true;
        }
    }
}