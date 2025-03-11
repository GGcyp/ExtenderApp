
using System;

namespace ExtenderApp.Common.NetWorks
{
    /// <summary>
    /// 数据包分段传输对象
    /// </summary>
    internal struct PacketSegmentDto
    {
        /// <summary>
        /// 分段数量
        /// </summary>
        public int SegmentIndex { get; set; }

        /// <summary>
        /// 数据包长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 数据内容
        /// </summary>
        public ReadOnlyMemory<byte> Data { get; set; }

        public PacketSegmentDto(int segmentIndex, int length, ReadOnlyMemory<byte> data)
        {
            SegmentIndex = segmentIndex;
            Length = length;
            Data = data;
        }
    }
}
