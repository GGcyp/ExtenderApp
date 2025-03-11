

namespace ExtenderApp.Common.NetWorks
{
    /// <summary>
    /// 数据包段头结构体
    /// </summary>
    internal struct PacketSegmentHead
    {
        /// <summary>
        /// 数据包长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 数据包类型码
        /// </summary>
        public int TypeCode { get; set; }

        /// <summary>
        /// 数据包段数量
        /// </summary>
        public int Count { get; set; }

        public PacketSegmentHead(int length, int typeCode, int count)
        {
            Length = length;
            TypeCode = typeCode;
            Count = count;
        }
    }
}
