using System.Buffers;
using System.Runtime.InteropServices;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.NetWorks
{
    /// <summary>
    /// 处理大数据包的分块和重组。
    /// </summary>
    public class PacketSegmenter
    {
        private readonly ILinker _linker; // 传输层
        private readonly Action<int, byte[], int, int> _callback; // 数据包接收完毕后的回调

        private byte[]? buffer; // 缓冲区
        private int typeCode; // 当前数据包类型码
        private int packetCount; // 数据包数量
        private int receivedCount; // 已接收的数据包数量
        private int totalLength; // 数据包总长度
        private int maxSegmenterLength; // 每个分块的最大大小

        public PacketSegmenter(ILinker linker, int maxSegmenterLength, Action<int, byte[], int, int> callback)
        {
            _linker = linker;
            this.maxSegmenterLength = maxSegmenterLength;
            _callback = callback;
            buffer = null;
        }

        public void Start()
        {
            _linker.Register<PacketSegmentHead>(ReceivePacketSegmentHead);
            _linker.Register<PacketSegmentDto>(ReceivePacketSegmentDto);
        }

        public bool SendBigPacket(int typeCode, byte[] bytes, int length)
        {
            if (length < maxSegmenterLength)
            {
                return false;
            }

            int count = length / maxSegmenterLength + (length % maxSegmenterLength > 0 ? 1 : 0);
            PacketSegmentHead head = new PacketSegmentHead(length, typeCode, count);
            _linker.Send(head);

            for (int i = 0; i < count - 1; i++)
            {
                PacketSegmentDto segmentDto = new PacketSegmentDto(i, maxSegmenterLength, new ReadOnlyMemory<byte>(bytes, i * maxSegmenterLength, maxSegmenterLength));
                _linker.Send(segmentDto);
            }

            int endIndex = count - 1;
            if (endIndex > 0)
            {
                int segmentLength = length % maxSegmenterLength;
                PacketSegmentDto segmentDto = new PacketSegmentDto(endIndex, segmentLength, new ReadOnlyMemory<byte>(bytes, endIndex * maxSegmenterLength, segmentLength));
                _linker.Send(segmentDto);
            }

            return true;
        }

        private void ReceivePacketSegmentHead(PacketSegmentHead segmentHead)
        {
            if (buffer != null)
                throw new Exception("上一个数据包还未接收完毕");

            buffer = ArrayPool<byte>.Shared.Rent(segmentHead.Length);
            typeCode = segmentHead.TypeCode;
            packetCount = segmentHead.Count;
            totalLength = segmentHead.Length;
        }

        private void ReceivePacketSegmentDto(PacketSegmentDto segmentDto)
        {
            segmentDto.Data.Span.CopyTo(buffer.AsSpan(segmentDto.SegmentIndex * maxSegmenterLength, segmentDto.Length));
            //Array.Copy(segmentDto.Data, 0, buffer, segmentDto.SegmentCount * maxSegmenterLength, segmentDto.Length);
            receivedCount++;
            if (MemoryMarshal.TryGetArray(segmentDto.Data, out var segment))
            {
                var sourceArray = segment.Array;
                if (sourceArray != null)
                    ArrayPool<byte>.Shared.Return(segment.Array!);
            }

            if (receivedCount != packetCount)
                return;

            _callback(typeCode, buffer, 0, buffer.Length);
            receivedCount = 0;
            packetCount = 0;
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = null;
        }
    }
}
