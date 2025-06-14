using System.Buffers;
using System.Runtime.InteropServices;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 数据包分段器类,处理数据包分段
    /// </summary>
    public class PacketSegmenter
    {
        /// <summary>
        /// 传输层接口
        /// </summary>
        private readonly ILinker _linker;

        /// <summary>
        /// 数据包接收完毕后的回调
        /// </summary>
        private readonly Action<int, byte[], int, int> _callback;

        /// <summary>
        /// 获取分段长度的函数
        /// </summary>
        private readonly Func<int> _getSegmenterLength;

        /// <summary>
        /// 缓冲区
        /// </summary>
        private byte[]? buffer;

        /// <summary>
        /// 当前数据包类型码
        /// </summary>
        private int typeCode;

        /// <summary>
        /// 数据包数量
        /// </summary>
        private int packetCount;

        /// <summary>
        /// 已接收的数据包数量
        /// </summary>
        private int receivedCount;

        /// <summary>
        /// 分段的长度
        /// </summary>
        private int segmenterLength;

        /// <summary>
        /// 判断是否正在发送数据
        /// </summary>
        /// <returns>如果正在发送数据，则返回 true；否则返回 false</returns>
        public bool IsSending => isSending != 0;
        private volatile int isSending;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="linker">传输层接口</param>
        /// <param name="maxSegmenterLength">每个分块的最大大小</param>
        /// <param name="callback">数据包接收完毕后的回调</param>
        public PacketSegmenter(ILinker linker, Func<int> getSegmenterLength, Action<int, byte[], int, int> callback)
        {
            _linker = linker;
            _callback = callback;
            _getSegmenterLength = getSegmenterLength;
            buffer = null;
        }

        /// <summary>
        /// 启动数据包分段器
        /// </summary>
        public void Start()
        {
            _linker.Register<PacketSegmentHead>(ReceivePacketSegmentHead);
            _linker.Register<PacketSegmentDto>(ReceivePacketSegmentDto);
            segmenterLength = _getSegmenterLength.Invoke();
        }

        /// <summary>
        /// 发送大数据包
        /// </summary>
        /// <param name="typeCode">数据包类型码</param>
        /// <param name="bytes">数据包字节数据</param>
        /// <param name="length">数据包长度</param>
        /// <returns>发送成功返回true，否则返回false</returns>
        /// <exception cref="ArgumentException">当分段长度小于等于零时抛出异常</exception>
        public void SendBigPacket(int typeCode, byte[] bytes, int length)
        {
            if (length < segmenterLength)
            {
                return;
            }

            Interlocked.Exchange(ref isSending, 1);

            int count = length / segmenterLength + (length % segmenterLength > 0 ? 1 : 0);
            PacketSegmentHead head = new PacketSegmentHead(length, typeCode, count);
            _linker.Send(head);

            for (int i = 0; i < count - 1; i++)
            {
                PacketSegmentDto segmentDto = new PacketSegmentDto(i, segmenterLength, new ReadOnlyMemory<byte>(bytes, i * segmenterLength, segmenterLength));
                _linker.Send(segmentDto);
            }

            int endIndex = count - 1;
            if (endIndex > 0)
            {
                int segmentLength = length % segmenterLength;
                PacketSegmentDto segmentDto = new PacketSegmentDto(endIndex, segmentLength, new ReadOnlyMemory<byte>(bytes, endIndex * segmenterLength, segmentLength));
                _linker.Send(segmentDto);
            }
            Interlocked.Exchange(ref isSending, 0);
        }

        /// <summary>
        /// 异步发送大数据包
        /// </summary>
        /// <param name="typeCode">数据包类型码</param>
        /// <param name="bytes">数据包字节数据</param>
        /// <param name="length">数据包长度</param>
        /// <returns>发送成功返回true，否则返回false</returns>
        /// <exception cref="ArgumentException">当分段长度小于等于零时抛出异常</exception>
        public void SendBigPacketAsync(int typeCode, byte[] bytes, int length)
        {
            if (length < segmenterLength)
            {
                return;
            }
            Interlocked.Exchange(ref isSending, 1);
            int count = length / segmenterLength + (length % segmenterLength > 0 ? 1 : 0);
            PacketSegmentHead head = new PacketSegmentHead(length, typeCode, count);
            _linker.SendAsync(head);

            for (int i = 0; i < count - 1; i++)
            {
                PacketSegmentDto segmentDto = new PacketSegmentDto(i, segmenterLength, new ReadOnlyMemory<byte>(bytes, i * segmenterLength, segmenterLength));
                _linker.SendAsync(segmentDto);
            }

            int endIndex = count - 1;
            if (endIndex > 0)
            {
                int segmentLength = length % segmenterLength;
                PacketSegmentDto segmentDto = new PacketSegmentDto(endIndex, segmentLength, new ReadOnlyMemory<byte>(bytes, endIndex * segmenterLength, segmentLength));
                _linker.SendAsync(segmentDto);
            }
            Interlocked.Exchange(ref isSending, 0);
        }

        /// <summary>
        /// 接收数据包头
        /// </summary>
        /// <param name="segmentHead">数据包头</param>
        private void ReceivePacketSegmentHead(PacketSegmentHead segmentHead)
        {
            if (buffer != null)
                throw new Exception("上一个数据包还未接收完毕");

            buffer = ArrayPool<byte>.Shared.Rent(segmentHead.Length);
            typeCode = segmentHead.TypeCode;
            packetCount = segmentHead.Count;
        }

        /// <summary>
        /// 接收数据包数据
        /// </summary>
        /// <param name="segmentDto">数据包数据对象</param>
        private void ReceivePacketSegmentDto(PacketSegmentDto segmentDto)
        {
            segmentDto.Data.Span.CopyTo(buffer.AsSpan(segmentDto.SegmentIndex * segmenterLength, segmentDto.Length));
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
