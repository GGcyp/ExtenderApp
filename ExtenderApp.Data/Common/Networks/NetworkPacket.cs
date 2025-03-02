

namespace ExtenderApp.Data
{
    /// <summary>
    /// 用于发送数据的结构体
    /// </summary>
    public struct NetworkPacket
    {
        /// <summary>
        /// 类型哈希码
        /// </summary>
        public int TypeCode { get; set; }

        /// <summary>
        /// 数据字节数组
        /// </summary>
        public byte[] Bytes { get; set; }

        public NetworkPacket(int typeCode, byte[] bytes)
        {
            TypeCode = typeCode;
            Bytes = bytes;
        }
    }
}
