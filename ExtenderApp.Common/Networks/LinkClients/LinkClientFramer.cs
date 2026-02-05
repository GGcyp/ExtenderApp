using System.Buffers;
using System.Buffers.Binary;
using System.Threading.Channels;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 链路客户端的帧化器实现。 负责：
    /// - 将接收到的字节流按协议解析为 Frame（TryBERDecodeSequence）；
    /// - 根据消息类型与负载长度构造可写帧头以供发送（EncodeSequence）。 实现遵循 ILinkClientFramer 的契约（接口中已经包含了对外方法的详细文档）， 此类管理接收缓存以处理半包/粘包场景，并在 Dispose 时释放缓存资源。
    /// </summary>
    public class LinkClientFramer : DisposableObject
    {
        private const int intLength = sizeof(int);

        /// <summary>
        /// 默认 TCP 魔数（ASCII: "TCP!"）。 用于帧的对齐与快速判定。
        /// </summary>
        public static readonly ReadOnlyMemory<byte> TcpMagic = new byte[] { 0x54, 0x43, 0x50, 0x21 }; // "TCP!"

        /// <summary>
        /// 默认 UDP 魔数（ASCII: "UDP!"）。
        /// </summary>
        public static readonly ReadOnlyMemory<byte> UdpMagic = new byte[] { 0x55, 0x44, 0x50, 0x21 }; // "UDP!"

        // 私有接收缓存：用于在解析过程中累积不完整的数据（处理半包/粘包）。
        private readonly SequenceCache<byte> _receiveCache;

        private readonly Channel<FrameContext> _frameChannel;

        public ReadOnlyMemory<byte> Magic { get; private set; }

        /// <summary>
        /// 构造一个 LinkClientFramer 实例。
        /// </summary>
        /// <param name="cacheLength">接收缓存的初始容量（字节）。建议根据预期吞吐量设置合理值以减少扩容。</param>
        /// <param name="magic">初始魔数字节序列；传入 null 则使用 <see cref="TcpMagic"/> 作为默认值。</param>
        public LinkClientFramer(ReadOnlySpan<byte> magic)
        {
            _receiveCache = new();
            _frameChannel = Channel.CreateUnbounded<FrameContext>();
            SetMagic(magic);
        }

        public void SetMagic(ReadOnlySpan<byte> magic)
        {
            // Empty 表示禁用魔数检查（与接口注释对齐）
            if (magic.IsEmpty || magic.Length == 0)
            {
                Magic = ReadOnlyMemory<byte>.Empty;
                return;
            }

            byte[] magicArray = new byte[magic.Length];
            magic.CopyTo(magicArray);
            Magic = magicArray;
        }

        public void Decode(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty || span.Length <= 0)
                return;

            // 格式: [magic?][length:int32 BE][payload:length]
            int magicLen = Magic.Length;
            int headerLen = magicLen + intLength; //4 bytes length

            _receiveCache.Write(span);
            while (true)
            {
                // 头部不足
                if (_receiveCache.Remaining < headerLen)
                    break;

                SequenceReader<byte> reader = _receiveCache.GetSequenceReader();
                // 查找/匹配 magic（若配置）
                bool magicMatched = true;
                ReadOnlySpan<byte> magicSpan = Magic.Span;
                if (magicLen > 0)
                {
                    for (int i = 0; i < magicLen; i++)
                    {
                        if (!reader.TryRead(out byte b))
                        {
                            magicMatched = false;
                            break;
                        }
                        if (b != magicSpan[i])
                        {
                            magicMatched = false;
                            break;
                        }
                    }

                    if (!magicMatched)
                    {
                        // magic 未匹配：丢弃 1 字节（向后搜索），继续下一次尝试以寻找对齐的魔数字节序列
                        reader.Advance(1);
                        if (reader.End)
                        {
                            break;
                        }
                        continue;
                    }
                }

                if (TryReadInt32BigEndian(reader, out int length))
                {
                    break;
                }
                if (length < 0)
                    throw new ArgumentOutOfRangeException(nameof(length), "帧长度非法（负值）。");

                // 如果剩余数据不足以包含完整 payload，则等候更多数据
                if (reader.Remaining < length || reader.Remaining == 0)
                    break;

                // 确认有完整帧：先消费 header（magic + type + length）再读取 payload
                ByteBlock payload = new(headerLen);
                _receiveCache.Read(payload.GetSpan(headerLen));

                // 将帧加入集合（集合接管 payload 的释放责任）
                _frameChannel.Writer.TryWrite(payload);
            }
        }

        public void Encode(ref FrameContext framedMessage)
        {
            ReadOnlyMemory<byte> messageSpan = framedMessage.UnreadMemory;
            int magicLen = Magic.Length;
            int headerLen = magicLen + intLength; //length(4)
            int length = messageSpan.Length;

            // 创建可写缓冲
            ByteBlock block = new(headerLen + messageSpan.Length);
            Span<byte> span = block.GetSpan(headerLen);

            Magic.Span.CopyTo(span.Slice(0, magicLen));

            BinaryPrimitives.WriteInt32BigEndian(span.Slice(magicLen + intLength, intLength), length);
            block.Write(messageSpan);
            framedMessage.WriteNextPayload(block);
        }

        public ValueTask<FrameContext> ReadFrameAsync(CancellationToken token = default)
        {
            return _frameChannel.Reader.ReadAsync(token);
        }

        private bool TryReadInt32BigEndian(SequenceReader<byte> reader, out int value)
        {
            Span<byte> buffer = stackalloc byte[4];
            for (int i = 0; i < intLength; i++)
            {
                if (!reader.TryRead(out byte b))
                {
                    value = 0;
                    return false;
                }
                buffer[i] = b;
            }
            value = BinaryPrimitives.ReadInt32BigEndian(buffer);
            return true;
        }

        protected override void DisposeManagedResources()
        {
            _receiveCache.Dispose();
        }
    }
}