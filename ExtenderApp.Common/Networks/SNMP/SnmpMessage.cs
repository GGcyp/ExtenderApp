using ExtenderApp.Common.Encodings;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// 完整 SNMP 消息封装（version/community/pdu）
    /// </summary>
    public struct SnmpMessage : IDisposable
    {
        public SnmpVersionType Version { get; set; }
        public string Community { get; set; }
        public SnmpPdu Pdu { get; set; }

        public SnmpMessage(SnmpPdu pdu, SnmpVersionType version = SnmpVersionType.V2c, string community = "public")
        {
            Pdu = pdu;
            Version = version;
            Community = community;
        }

        public void Dispose()
        {
            Pdu.Dispose();
        }

        public void Encode(ref ByteBlock block)
        {
            ByteBlock valueBlock = new ByteBlock();
            BEREncoding.Encode(ref valueBlock, (int)Version);
            BEREncoding.Encode(ref valueBlock, Community);
            Pdu.Encode(ref valueBlock);
            BEREncoding.EncodeSequence(ref block, valueBlock);
            valueBlock.Dispose();
        }
    }
}