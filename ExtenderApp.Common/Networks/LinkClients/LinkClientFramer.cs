using System.Buffers.Binary;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 链路客户端的帧化器实现。
    /// /// 负责：
    /// - 将接收到的字节流按协议解析为 Frame（TryBERDecodeSequence）；
    /// - 根据消息类型与负载长度构造可写帧头以供发送（EncodeSequence）。
    /// 实现遵循 ILinkClientFramer 的契约（接口中已经包含了对外方法的详细文档），
    /// 此类管理接收缓存以处理半包/粘包场景，并在 Dispose 时释放缓存资源。
    /// </summary>
    public class LinkClientFramer : DisposableObject, ILinkClientFramer
    {
        /// <summary>
        /// 默认 TCP 魔数（ASCII: "TCP!"）。
        /// 用于帧的对齐与快速判定。
        /// </summary>
        public static readonly ReadOnlyMemory<byte> TcpMagic = new byte[] { 0x54, 0x43, 0x50, 0x21 }; // "TCP!"

        /// <summary>
        /// 默认 UDP 魔数（ASCII: "UDP!"）。
        /// </summary>
        public static readonly ReadOnlyMemory<byte> UdpMagic = new byte[] { 0x55, 0x44, 0x50, 0x21 }; // "UDP!"

        // 私有接收缓存：用于在解析过程中累积不完整的数据（处理半包/粘包）。
        private ByteBlock receiveCacheBlock;

        // 初始或建议的缓存长度（用于重置/构造时参考）。
        private int cacheLength;

        public ReadOnlyMemory<byte> Magic { get; private set; }

        /// <summary>
        /// 构造一个 LinkClientFramer 实例。
        /// </summary>
        /// <param name="cacheLength">接收缓存的初始容量（字节）。建议根据预期吞吐量设置合理值以减少扩容。</param>
        /// <param name="magic">初始魔数字节序列；传入 null 则使用 <see cref="TcpMagic"/> 作为默认值。</param>
        public LinkClientFramer(int cacheLength, ReadOnlySpan<byte> magic)
        {
            receiveCacheBlock = new(cacheLength);
            this.cacheLength = cacheLength;
            SetMagic(magic);
        }

        public void SetMagic(ReadOnlySpan<byte> magic)
        {
            if (magic.IsEmpty || magic.Length <= 0)
                throw new ArgumentNullException(nameof(magic));
            byte[] magicArray = new byte[magic.Length];
            magic.CopyTo(magicArray);
            Magic = magicArray;
        }

        public void Decode(ref ByteBuffer originalMessage, out PooledFrameList framedList)
        {
            framedList = new PooledFrameList();
            if (originalMessage.Remaining == 0 && receiveCacheBlock.Remaining == 0)
                return;

            // 格式: [magic?][messageType:int32 BE][length:int32 BE][payload:length]
            int magicLen = Magic.Length;
            int intLength = sizeof(int);
            int headerLen = magicLen + intLength * 2; // 4 bytes type + 4 bytes length

            // 如果有新的输入数据，先把它追加到 receiveCacheBlock（缓存）并将 originalMessage 全部消费
            if (originalMessage.Remaining > 0)
            {
                receiveCacheBlock.Write(originalMessage);
            }
            Span<byte> tmp = stackalloc byte[4];
            while (true)
            {
                // 头部不足
                if (receiveCacheBlock.Remaining < headerLen)
                    break;

                ByteBuffer peek = new(receiveCacheBlock);
                // 查找/匹配 magic（若配置）
                bool magicMatched = true;
                ReadOnlySpan<byte> magicSpan = Magic.Span;
                if (magicLen > 0)
                {
                    for (int i = 0; i < magicLen; i++)
                    {
                        if (!peek.TryRead(out byte b))
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
                        peek.ReadAdvance(1);
                        if (peek.End)
                        {
                            break;
                        }
                        continue;
                    }
                }

                // 从 peek 读取 messageType 与 length（peek 已在 magic 之后）
                for (int i = 0; i < intLength; i++)
                {
                    if (!peek.TryRead(out byte b))
                    {
                        magicMatched = false;
                        break;
                    }
                    tmp[i] = b;
                }
                if (!magicMatched) break;
                int messageType = BinaryPrimitives.ReadInt32BigEndian(tmp);

                for (int i = 0; i < intLength; i++)
                {
                    if (!peek.TryRead(out byte b))
                    {
                        magicMatched = false;
                        break;
                    }
                    tmp[i] = b;
                }
                if (!magicMatched) break;
                int length = BinaryPrimitives.ReadInt32BigEndian(tmp);

                if (length < 0)
                    throw new ArgumentOutOfRangeException(nameof(length), "帧长度非法（负值）。");
                else if (length > receiveCacheBlock.Capacity)
                {
                    receiveCacheBlock.Ensure(length);
                }

                // 如果剩余数据不足以包含完整 payload，则等候更多数据
                if (peek.Remaining < length || peek.Remaining == 0)
                    break;

                // 确认有完整帧：先消费 header（magic + type + length）再读取 payload
                receiveCacheBlock.ReadAdvance(headerLen);

                // 将 payloadSeq 拷贝到新的 ByteBlock（由 Frame 接管并在最终释放）
                var payload = new ByteBlock(receiveCacheBlock.UnreadSpan.Slice(0, length));
                receiveCacheBlock.ReadAdvance(length);

                // 将帧加入集合（集合接管 payload 的释放责任）
                framedList.Add(new Frame(messageType, payload));
                // 继续循环以尝试解析后续帧
            }

            if (receiveCacheBlock.Remaining == 0)
            {
                receiveCacheBlock.Dispose();
                receiveCacheBlock = new ByteBlock(cacheLength);
            }
            else
            {
                receiveCacheBlock.Compact();
            }
        }

        public void Encode(int messageType, int length, out ByteBuffer framedMessage)
        {
            // 构造帧头并返回一个可写 ByteBuffer，写入者可以在其后写入 payload
            int magicLen = Magic.Length;
            int intLength = sizeof(int);
            int headerLen = magicLen + intLength * 2; // messageType(4) + length(4)

            // 创建可写缓冲
            framedMessage = ByteBuffer.CreateBuffer();
            Span<byte> span = framedMessage.GetSpan(headerLen);

            Magic.Span.CopyTo(span.Slice(0, magicLen));

            BinaryPrimitives.WriteInt32BigEndian(span.Slice(magicLen, intLength), messageType);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(magicLen + intLength, intLength), length);

            framedMessage.WriteAdvance(headerLen);
        }

        /// <summary>
        /// 释放托管资源（主要是接收缓存），并清理内部状态引用。
        /// </summary>
        /// <param name="disposing">指示是否由 Dispose 调用。</param>
        protected override void Dispose(bool disposing)
        {
            receiveCacheBlock.Dispose();
            Magic = null!;
        }
    }
}
