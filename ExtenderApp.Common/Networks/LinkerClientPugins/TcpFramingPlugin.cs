using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    public sealed class TcpFramingPlugin : IClientSendPlugin, IClientReceivePlugin
    {
        private static byte[] DefaultMagic = { 0x54, 0x43, 0x50, 0x21 }; // "TCP!"

        private readonly IByteBufferFactory _bufferFactory;
        private readonly IBinaryFormatter<LinkHeader> _headerFormatter;
        private readonly byte[]? _magic;
        private readonly int _magicLen;
        private ByteBlock receiveCacheBlock;
        private int reamingLength;

        public TcpFramingPlugin(IBinaryFormatter<LinkHeader> headerFormatter, IByteBufferFactory bufferFactory) : this(headerFormatter, bufferFactory, (int)Utility.KilobytesToBytes(16), DefaultMagic)
        {
        }

        public TcpFramingPlugin(IBinaryFormatter<LinkHeader> headerFormatter, IByteBufferFactory bufferFactory, int cacheLength, byte[]? magic = null)
        {
            _bufferFactory = bufferFactory;
            _headerFormatter = headerFormatter;
            _magic = magic;
            _magicLen = magic?.Length ?? 0;
            receiveCacheBlock = new(cacheLength);
            reamingLength = 0;
        }

        public void ReceiveOperateContext(LinkerClientContext context)
        {
            // 将传入的数据追加到缓存（仅使用可读部分）
            if (context.MessageBlock.Remaining > 0)
            {
                receiveCacheBlock.Write(context.MessageBlock);
            }
            context.MessageBlock.Dispose();
            context.MessageBlock = default;

            // 逐步解析：Magic -> Header -> Body
            while (true)
            {
                var available = (int)receiveCacheBlock.Remaining;
                if (available <= 0)
                {
                    break;
                }

                // 对齐 Magic（若启用）
                if (_magicLen > 0)
                {
                    if (available < _magicLen)
                    {
                        break;
                    }

                    var unread = receiveCacheBlock.UnreadSpan;
                    if (!unread.StartsWith(_magic))
                    {
                        int idx = IndexOf(unread, _magic);
                        if (idx < 0)
                        {
                            // 保留末尾 _magicLen - 1 字节以应对跨包匹配
                            int keep = _magicLen - 1;
                            if (available > keep)
                            {
                                receiveCacheBlock.ReadAdvance(available - keep);
                            }
                            break;
                        }
                        else if (idx > 0)
                        {
                            receiveCacheBlock.ReadAdvance(idx);
                            available = (int)receiveCacheBlock.Remaining;
                            if (available < _magicLen)
                            {
                                break;
                            }
                            unread = receiveCacheBlock.UnreadSpan;
                        }
                    }

                    // 跳过 Magic
                    receiveCacheBlock.ReadAdvance(_magicLen);
                }

                // 确保头部足够（假设 DefaultLength 为头部实际长度或下界）
                if (receiveCacheBlock.Remaining < _headerFormatter.DefaultLength)
                {
                    // 不足一个完整头部，等待更多数据
                    if (_magicLen > 0)
                    {
                        // 回退到 Magic 前状态：下次再从 Magic 之后继续
                        receiveCacheBlock.Rewind(_magicLen);
                    }
                    break;
                }

                // 使用只读视图尝试反序列化头部，计算被消费的头部字节数
                var peek = new ByteBuffer(receiveCacheBlock.UnreadMemory);
                LinkHeader header;
                long consumedHeader;
                try
                {
                    header = _headerFormatter.Deserialize(ref peek);
                    consumedHeader = peek.Consumed;
                }
                catch
                {
                    // 数据尚不完整或格式不正确：等待更多数据
                    if (_magicLen > 0)
                    {
                        receiveCacheBlock.Rewind(_magicLen);
                    }
                    break;
                }

                if (consumedHeader <= 0 || consumedHeader > int.MaxValue)
                {
                    // 头部异常，放弃本次解析
                    break;
                }

                // 确保负载足够
                long needed = consumedHeader + header.DataLength;
                if (receiveCacheBlock.Remaining < needed)
                {
                    // 回退 Magic（若有），等待更多字节
                    if (_magicLen > 0)
                    {
                        receiveCacheBlock.Rewind(_magicLen);
                    }
                    break;
                }

                // 前进读指针跳过头部
                receiveCacheBlock.ReadAdvance((int)consumedHeader);

                // 复制负载为新的 MessageBlock
                var payload = new ByteBlock(header.DataLength);
                payload.Write(receiveCacheBlock.UnreadSpan.Slice(0, header.DataLength));
                receiveCacheBlock.ReadAdvance(header.DataLength);

                // 输出到 context：得到一帧完整的消息
                context.LinkHeader = header;
                context.MessageBlock = payload;

                // 仅产出一帧，其他留待下次调用
                break;
            }

            // 若缓冲区已读空，重置以避免无限增长
            if (receiveCacheBlock.Remaining == 0 && receiveCacheBlock.Length > 0)
            {
                receiveCacheBlock.Reset();
            }
        }

        public void SendOperateContext(LinkerClientContext context)
        {
            var buffer = _bufferFactory.Create();
            _headerFormatter.Serialize(ref buffer, context.LinkHeader);
            ByteBlock block = new ByteBlock((int)(buffer.Length + context.MessageBlock.Remaining + _magicLen));

            ReadOnlyMemory<byte> magicMemory = _magic ?? Array.Empty<byte>();
            block.Write(magicMemory);
            block.Write(buffer);
            block.Write(context.MessageBlock);
            context.MessageBlock.Dispose();
            context.MessageBlock = block;
        }

        private static int IndexOf(ReadOnlySpan<byte> span, ReadOnlySpan<byte> pattern)
        {
            if (pattern.Length == 0) return 0;
            if (span.Length < pattern.Length) return -1;

            int last = span.Length - pattern.Length;
            for (int i = 0; i <= last; i++)
            {
                if (span[i] == pattern[0] && span.Slice(i, pattern.Length).SequenceEqual(pattern))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
