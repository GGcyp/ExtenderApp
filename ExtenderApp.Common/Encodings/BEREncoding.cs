using System.Buffers.Binary;
using System.Formats.Asn1;
using System.Text;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Encodings
{
    /// <summary>
    /// 提供对基本 BER/ASN.1 编码与解码的静态工具方法。
    /// - 编码方法负责将常用 ASN.1 原语写入 <see cref="ByteBlock"/> ;
    /// - 解码方法负责从 <see cref="ByteBlock"/> 中读取并解析对应的 TLV。
    /// </summary>
    public static class BEREncoding
    {
        #region 编码方法

        /// <summary>
        /// 将有符号整数按 ASN.1 INTEGER 的最短补码形式编码并写入目标块（包含 Tag/Capacity/Value）。
        /// </summary>
        /// <param name="block">目标写入块。</param>
        /// <param name="value">要编码的整数值。</param>
        /// <remarks>
        /// 方法会自动去除冗余前导字节以生成最短编码（兼容 RFC 相关要求）。
        /// </remarks>
        public static void EncodeInteger(ref ByteBlock block, long value)
        {
            // 写标签：Universal, Primitive, INTEGER
            EncodeTag(ref block, Asn1Tag.Integer);

            // 将 long 写成 8 字节大端补码
            Span<byte> tmp = stackalloc byte[8];
            ulong u = unchecked((ulong)value);
            for (int i = 7; i >= 0; i--)
            {
                tmp[i] = (byte)(u & 0xFF);
                u >>= 8;
            }

            // 剔除冗余前缀，确保最短编码：
            int start = 0;
            if (value >= 0)
            {
                // 去掉多余的 0x00，但要保证下一个字节最高位为 0
                while (start < 7 && tmp[start] == 0x00 && (tmp[start + 1] & 0x80) == 0)
                    start++;
            }
            else
            {
                // 去掉多余的 0xFF，但要保证下一个字节最高位为 1
                while (start < 7 && tmp[start] == 0xFF && (tmp[start + 1] & 0x80) == 0x80)
                    start++;
            }

            int len = 8 - start;
            EncodeLength(ref block, len);
            block.Write(tmp.Slice(start, len));
        }

        /// <summary>
        /// 将指定字符串按 UTF-8 编码写为 ASN.1 UTF8String（包含 Tag/Capacity/Value）。
        /// </summary>
        /// <param name="block">目标写入块。</param>
        /// <param name="value">要编码的字符串（非空）。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="value"/> 为空或空字符串时抛出。</exception>
        public static void EncodeUtf8String(ref ByteBlock block, string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            // 使用 UTF8String 的 Universal tag
            EncodeTag(ref block, new Asn1Tag(TagClass.Universal, (int)UniversalTagNumber.UTF8String, false));

            Encoding encoding = Encoding.UTF8;
            int length = encoding.GetByteCount(value);
            EncodeLength(ref block, length);

            Span<byte> span = block.GetSpan(length);
            Encoding.UTF8.GetBytes(value, span);
            block.WriteAdvance(length);
        }

        /// <summary>
        /// 将 <see cref="ByteBuffer"/> 的剩余可读数据作为一个 SEQUENCE 的内容写入目标块（写入 Tag/Capacity/Value）。
        /// </summary>
        /// <param name="block">目标写入块。</param>
        /// <param name="buffer">包含要写入的序列内容的 <see cref="ByteBuffer"/> （其未读内容会被写入）。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为空时抛出。</exception>
        public static void EncodeSequence(ref ByteBlock block, ByteBuffer buffer)
        {
            if (buffer.IsEmpty)
                throw new ArgumentNullException(nameof(buffer));

            Encode(ref block, buffer, Asn1Tag.Sequence);
        }

        /// <summary>
        /// 将 <see cref="ByteBlock"/> 的未读数据作为一个 SEQUENCE 的内容写入目标块（写入 Tag/Capacity/Value）。
        /// </summary>
        /// <param name="block">目标写入块。</param>
        /// <param name="buffer">包含要写入的序列内容的 <see cref="ByteBlock"/>。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为空时抛出。</exception>
        public static void EncodeSequence(ref ByteBlock block, ByteBlock buffer)
        {
            if (buffer.IsEmpty)
                throw new ArgumentNullException(nameof(buffer));

            Encode(ref block, buffer, Asn1Tag.Sequence);
        }

        /// <summary>
        /// 将字符串按指定编码写成 OCTET STRING（包含 Tag/Capacity/Value）。
        /// </summary>
        /// <param name="block">目标写入块。</param>
        /// <param name="value">要编码的字符串。</param>
        /// <param name="encoding">可选的字符编码，默认 ASCII。</param>
        public static void EncodeOctetString(ref ByteBlock block, string value, Encoding? encoding = null)
        {
            ByteBlock stringBlock = new();
            stringBlock.Write(value, encoding ?? Encoding.ASCII);
            Encode(ref block, stringBlock, Asn1Tag.PrimitiveOctetString);
            stringBlock.Dispose();
        }

        /// <summary>
        /// 写入 ASN.1 NULL（Tag + 0 长度）。
        /// </summary>
        /// <param name="block">目标写入块。</param>
        public static void EncodeNull(ref ByteBlock block)
        {
            EncodeTag(ref block, Asn1Tag.Null);
            EncodeLength(ref block, 0);
        }

        /// <summary>
        /// 将布尔值按 ASN.1 BOOLEAN 编码并写入（Tag/Capacity/Value）。
        /// </summary>
        /// <param name="block">目标写入块。</param>
        /// <param name="value">布尔值，true 编码为 0xFF，false 为 0x00。</param>
        public static void EncodeBoolean(ref ByteBlock block, bool value)
        {
            byte valueByte = value ? (byte)0xFF : (byte)0x00;
            Encode(ref block, valueByte, Asn1Tag.Boolean);
        }

        /// <summary>
        /// 将单字节值以指定 <see cref="Asn1Tag"/> 编码并写入（Tag/Capacity/Value）。
        /// </summary>
        /// <param name="block">目标写入块。</param>
        /// <param name="value">要写入的单字节值。</param>
        /// <param name="tag">用于编码的 <see cref="Asn1Tag"/>。</param>
        public static void Encode(ref ByteBlock block, byte value, Asn1Tag tag)
        {
            EncodeTag(ref block, tag);
            EncodeLength(ref block, 1);
            block.Write(value);
        }

        /// <summary>
        /// 将 <see cref="ByteBuffer"/> 的剩余数据以指定 <see cref="Asn1Tag"/> 编码写入（适用于构造或原语标签）。
        /// </summary>
        /// <param name="block">目标写入块。</param>
        /// <param name="buffer">源缓冲，其未读数据将作为 Value 写入。</param>
        /// <param name="tag">目标 ASN.1 标签（如 Sequence、Application/ContextSpecific 等）。</param>
        public static void Encode(ref ByteBlock block, ByteBuffer buffer, Asn1Tag tag)
        {
            EncodeTag(ref block, tag);
            EncodeLength(ref block, (int)buffer.Remaining);
            block.Write(buffer);
        }

        /// <summary>
        /// 将 <see cref="ByteBlock"/> 的可读数据以指定 <see cref="Asn1Tag"/> 编码写入（适用于已构造的临时块）。
        /// </summary>
        /// <param name="block">目标写入块。</param>
        /// <param name="buffer">源 <see cref="ByteBlock"/>。</param>
        /// <param name="tag">目标 ASN.1 标签。</param>
        public static void Encode(ref ByteBlock block, ByteBlock buffer, Asn1Tag tag)
        {
            EncodeTag(ref block, tag);
            EncodeLength(ref block, buffer.Remaining);
            block.Write(buffer);
        }

        /// <summary>
        /// 编码 ASN.1 Tag 并写入目标块。
        /// 支持单字节 tag（tag number <= 31）与多字节高 tag 编码（tag number &gt; 31）。
        /// </summary>
        /// <param name="block">目标写入块。</param>
        /// <param name="tag">要编码的 <see cref="Asn1Tag"/>。</param>
        public static void EncodeTag(ref ByteBlock block, Asn1Tag tag)
        {
            // 先映射 TagClass 到 BER 的高两位掩码
            byte classBits = tag.TagClass.ToByte();

            byte firstByte = classBits;
            // 设置构造类型标志（第6位）
            if (tag.IsConstructed)
                firstByte |= 0x20;

            int tagNumber = tag.TagValue;

            // 标签号处理：<=31用单字节，>31用多字节
            if (tagNumber <= 31)
            {
                firstByte |= (byte)tagNumber;
                block.Write(firstByte);
            }
            else
            {
                firstByte |= 0x1F; // 标记为多字节模式
                block.Write(firstByte);

                // 多字节编码（每个字节最高位为1，最后一个为0）
                ByteBlock numberBlock = new();
                while (tagNumber > 0)
                {
                    numberBlock.Write((byte)(tagNumber & 0x7F));// 取低7位
                    tagNumber >>= 7;
                }

                // 反转字节并设置最高位（除最后一个）
                var numberSpan = numberBlock.UnreadSpan;
                for (int i = numberSpan.Length - 1; i >= 0; i--)
                {
                    byte t = numberSpan[i];
                    if (i != 0)
                    {
                        t |= 0x80; // 非最后一个字节设置最高位
                    }
                    block.Write(t);
                }
                numberBlock.Dispose();
            }
        }

        /// <summary>
        /// 按 BER 规则编码长度并写入目标块（支持短/长格式，长格式最大支持 4 字节长度）。
        /// </summary>
        /// <param name="block">目标写入块。</param>
        /// <param name="length">要编码的长度（非负）。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="length"/> 为负值时抛出。</exception>
        public static void EncodeLength(ref ByteBlock block, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            // 短格式（长度<128）
            if (length <= 0x7F)
            {
                block.Write((byte)length);
                return;
            }

            // 长格式（长度>=128）
            ByteBlock lengthBlock = new(5);
            int byteCount = 0;
            int temp = length;
            // 计算需要的字节数
            while (temp > 0)
            {
                lengthBlock.Write((byte)(temp & 0xFF));
                temp >>= 8;
                byteCount++;
            }
            lengthBlock.Reverse(); // 转为大端序
                                   // 第一个字节：0x80 + 字节数
            block.Write((byte)(0x80 | byteCount));
            block.Write(lengthBlock);
            lengthBlock.Dispose();
        }

        /// <summary>
        /// 将 ASN.1 TagClass 枚举映射为 BER 首字节中对应的高两位值（0x00/0x40/0x80/0xC0）。
        /// </summary>
        /// <param name="tagClass">要映射的 <see cref="TagClass"/>。</param>
        /// <returns>对应的字节位掩码。</returns>
        public static byte ToByte(this TagClass tagClass)
        {
            return tagClass switch
            {
                TagClass.Universal => 0x00,
                TagClass.Application => 0x40,
                TagClass.ContextSpecific => 0x80,
                TagClass.Private => 0xC0,
                _ => 0x00
            };
        }

        #endregion 编码方法

        #region 解码方法

        /// <summary>
        /// 从 <see cref="ByteBlock"/> 中解码一个 ASN.1 INTEGER 并返回 Int64（包含 Tag/Capacity 的消费）。
        /// </summary>
        /// <param name="block">源字节块，读取位置会推进。</param>
        /// <returns>解码得到的 Int64 值。</returns>
        /// <exception cref="InvalidDataException">当当前位置不是 INTEGER 或长度不合法时抛出。</exception>
        public static long DecodeInteger(ref ByteBlock block)
        {
            var tag = DecodeTag(ref block);
            if (tag.IsConstructed || tag.ToUTagNumber() != UniversalTagNumber.Integer)
                throw new InvalidDataException("不是整数类型的BER数据");

            int length = DecodeLength(ref block);
            if (length <= 0)
                throw new InvalidDataException("无效的整数长度");

            ReadOnlySpan<byte> src = block.Read(length);
            bool negative = (src[0] & 0x80) != 0;

            Span<byte> big = stackalloc byte[8];
            big.Fill(negative ? (byte)0xFF : (byte)0x00);

            if (length <= 8)
            {
                src.CopyTo(big.Slice(8 - length));
            }
            else
            {
                int extra = length - 8;
                byte pad = negative ? (byte)0xFF : (byte)0x00;
                for (int i = 0; i < extra; i++)
                {
                    if (src[i] != pad)
                        throw new OverflowException("整数超出 Int64 可表示范围。");
                }
                src.Slice(extra).CopyTo(big);
            }

            return BinaryPrimitives.ReadInt64BigEndian(big);
        }

        /// <summary>
        /// 从 <see cref="ByteBlock"/> 中解码 BOOLEAN（包含 Tag/Capacity/Value）。
        /// </summary>
        /// <param name="block">源字节块。</param>
        /// <returns>解析得到的布尔值。</returns>
        /// <exception cref="InvalidDataException">当当前位置不是 BOOLEAN 或长度不为 1 时抛出。</exception>
        public static bool DecodeBoolean(ref ByteBlock block)
        {
            var tag = DecodeTag(ref block);
            if (tag.IsConstructed || tag.ToUTagNumber() != UniversalTagNumber.Boolean)
                throw new InvalidDataException("不是布尔类型的BER数据");

            int length = DecodeLength(ref block);
            if (length != 1)
                throw new InvalidDataException("无效的布尔长度");

            byte valueByte = block.Read();
            return valueByte != 0x00;
        }

        /// <summary>
        /// 从 <see cref="ByteBlock"/> 中解码一个 UTF8String 并返回字符串（包含 Tag/Capacity/Value 的消费）。
        /// </summary>
        /// <param name="block">源字节块。</param>
        /// <returns>解码得到的字符串。</returns>
        /// <exception cref="InvalidDataException">当当前位置不是 UTF8String 时抛出。</exception>
        public static string DecodeUtf8String(ref ByteBlock block)
        {
            var tag = DecodeTag(ref block);
            if (tag.ToUTagNumber() != UniversalTagNumber.UTF8String)
                throw new InvalidDataException("不是UTF8字符串类型的BER数据");

            int length = DecodeLength(ref block);
            ReadOnlySpan<byte> valueBytes = block.Read(length);
            return Encoding.UTF8.GetString(valueBytes);
        }

        /// <summary>
        /// 尝试解析当前位置的 OCTET STRING，并将其内容写入新的 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="block">源字节块（不会在失败分支推进读取位置）。</param>
        /// <param name="valueBlock">成功时输出包含 OCTET STRING 内容的新块（调用方负责释放）。</param>
        /// <returns>成功解析返回 true，否则 false。</returns>
        public static bool TryDecodeOctetString(ref ByteBlock block, out ByteBlock valueBlock)
        {
            valueBlock = new ByteBlock();
            var temp = block.PeekByteBlock();
            if (temp.Remaining == 0)
                return false;

            var tag = DecodeTag(ref temp);
            if (tag.ToUTagNumber() != UniversalTagNumber.OctetString)
                return false;

            int length = DecodeLength(ref temp);
            valueBlock.Write(temp.Read(length));
            block.ReadAdvance(temp.Consumed - block.Consumed);
            return true;
        }

        /// <summary>
        /// 尝试解析当前位置的 NULL 项（如存在则消费该项并返回 true）。
        /// </summary>
        /// <param name="block">源字节块。</param>
        /// <returns>若当前位置为 NULL 并成功解析返回 true，否则 false。</returns>
        public static bool TryDecodeNull(ref ByteBlock block)
        {
            var temp = block.PeekByteBlock();
            if (temp.Remaining == 0)
                return false;
            var tag = DecodeTag(ref temp);
            if (tag.ToUTagNumber() != UniversalTagNumber.Null)
                return false;
            int length = DecodeLength(ref block);
            if (length != 0)
                return false;

            block.ReadAdvance(temp.Consumed - block.Consumed);
            return true;
        }

        /// <summary>
        /// 尝试判断当前位置是否为 SEQUENCE（构造类型）且长度可用；仅做预检（若满足会推进原始块到 Tag 后并返回 true）。
        /// </summary>
        /// <param name="block">源字节块。</param>
        /// <returns>若当前位置为构造型 Sequence 且剩余长度足够返回 true，否则 false。</returns>
        public static bool TryDecodeSequence(ref ByteBlock block)
        {
            var temp = block.PeekByteBlock();
            if (block.Remaining == 0)
                return false;

            var tag = DecodeTag(ref temp);
            if (tag.ToUTagNumber() != UniversalTagNumber.Sequence || !tag.IsConstructed)
                return false;

            block.ReadAdvance(temp.Consumed - block.Consumed);
            int length = DecodeLength(ref block);
            if (block.Remaining < length)
                return false;

            return true;
        }

        /// <summary>
        /// 预读并解析当前位置的 Tag（不会消费 Value/Capacity，仅推进 Tag 的字节），返回解析得到的 <see cref="Asn1Tag"/>。
        /// </summary>
        /// <param name="block">源字节块。</param>
        /// <param name="tag">输出解析到的 Tag（成功时）。</param>
        /// <returns>成功解析返回 true，否则 false（通常是输入不足）。</returns>
        public static bool TryDecodeTag(ref ByteBlock block, out Asn1Tag tag)
        {
            tag = default;
            var temp = block.PeekByteBlock();
            if (temp.Remaining == 0)
                return false;
            tag = DecodeTag(ref temp);
            block.ReadAdvance(temp.Consumed - block.Consumed);
            return true;
        }

        /// <summary>
        /// 从流当前位置解析 ASN.1 Tag（含对高 tag-number 的多字节解析），并消费对应的 Tag 字节。
        /// </summary>
        /// <param name="block">源字节块，读取位置会推进。</param>
        /// <returns>解析得到的 <see cref="Asn1Tag"/>。</returns>
        public static Asn1Tag DecodeTag(ref ByteBlock block)
        {
            byte firstByte = block.Read();
            // 高两位为类别掩码
            byte classBits = (byte)(firstByte & 0xC0);
            TagClass tc = (TagClass)classBits; // 0..3 对应 TagClass 枚举
            bool isConstructed = (firstByte & 0x20) != 0;
            int tagNumber = firstByte & 0x1F;

            if (tagNumber == 0x1F)
            {
                tagNumber = 0;
                byte b;
                do
                {
                    b = block.Read();
                    tagNumber = (tagNumber << 7) | (b & 0x7F);
                } while ((b & 0x80) != 0);
            }

            return new Asn1Tag(tc, tagNumber, isConstructed);
        }

        /// <summary>
        /// 从 <see cref="ByteBlock"/> 中读取并解析 BER 的 Capacity 字段（仅支持确定长度表示）。
        /// </summary>
        /// <param name="block">源字节块，读取位置会推进到 Capacity 字段之后。</param>
        /// <returns>返回 Value 部分的长度（字节数）。</returns>
        /// <exception cref="InvalidDataException">当遇到无限长度或超出支持范围时抛出。</exception>
        public static int DecodeLength(ref ByteBlock block)
        {
            byte firstByte = block.Read();
            if ((firstByte & 0x80) == 0)
            {
                // 短格式
                return firstByte;
            }

            int byteCount = firstByte & 0x7F;
            if (byteCount == 0)
                throw new InvalidDataException("不支持无限长度格式");
            if (byteCount > 4)
                throw new InvalidDataException("长度超过支持的最大范围（4字节）");

            // 直接读取到局部 buffer（高位对齐），避免对临时 ByteBlock 的写入/状态依赖
            Span<byte> buf = stackalloc byte[4];
            int offset = 4 - byteCount;
            for (int i = 0; i < byteCount; i++)
            {
                buf[offset + i] = block.Read();
            }

            return BinaryPrimitives.ReadInt32BigEndian(buf);
        }

        /// <summary>
        /// 将 <see cref="Asn1Tag"/> 的 TagValue 转换为 <see cref="UniversalTagNumber"/> （仅在 Tag 为 Universal 时有意义）。
        /// </summary>
        /// <param name="tag">要转换的 <see cref="Asn1Tag"/>。</param>
        /// <returns>对应的 <see cref="UniversalTagNumber"/> 值。</returns>
        public static UniversalTagNumber ToUTagNumber(this Asn1Tag tag)
        {
            return (UniversalTagNumber)tag.TagValue;
        }

        #endregion 解码方法
    }
}