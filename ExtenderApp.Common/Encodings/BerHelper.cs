

using System.Buffers;
using System.Text;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Encodings
{
    public static class BerHelper
    {
        #region 标签(Tag)定义

        // 标签类别（Class）
        public const byte UniversalClass = 0x00; // 通用类

        public const byte ApplicationClass = 0x40; // 应用类
        public const byte ContextSpecificClass = 0x80; // 上下文特定类
        public const byte PrivateClass = 0xC0; // 私有类

        // 通用类标签值（Universal Tag Number）
        public const int IntegerTag = 0x02; // 整数

        public const int Utf8StringTag = 0x0C; // UTF8字符串
        public const int SequenceTag = 0x10; // 序列（构造类型）

        #endregion 标签(Tag)定义

        #region 编码方法

        /// <summary>
        /// 编码整数类型
        /// </summary>
        /// <param name="value">整数数值</param>
        /// <returns>BER编码后的字节数组</returns>
        public static byte[] Encode(long value)
        {
            // 1. 编码标签（通用类+原始类型+整数标签）
            byte[] tagBytes = EncodeTag(UniversalClass, isConstructed: false, IntegerTag);

            // 2. 编码值（二进制补码形式，去除前导零）
            List<byte> valueBytes = new List<byte>();
            bool isNegative = value < 0;
            ulong unsignedValue = isNegative ? (ulong)(-value) : (ulong)value;

            // 处理零值
            if (unsignedValue == 0)
            {
                valueBytes.Add(0x00);
            }
            else
            {
                // 提取字节（大端序）
                while (unsignedValue > 0)
                {
                    valueBytes.Add((byte)(unsignedValue & 0xFF));
                    unsignedValue >>= 8;
                }
                // 反转字节以符合大端序
                valueBytes.Reverse();

                // 处理负数（补码调整）
                if (isNegative)
                {
                    for (int i = 0; i < valueBytes.Count; i++)
                    {
                        valueBytes[i] = (byte)~valueBytes[i];
                    }
                    // 加1完成补码
                    int carry = 1;
                    for (int i = valueBytes.Count - 1; i >= 0 && carry > 0; i--)
                    {
                        int temp = valueBytes[i] + carry;
                        valueBytes[i] = (byte)(temp & 0xFF);
                        carry = temp >> 8;
                    }
                    if (carry > 0)
                    {
                        valueBytes.Insert(0, 0xFF); // 溢出时补高位
                    }
                }

                // 确保无符号数最高位不为1（避免被误判为负数）
                if (!isNegative && (valueBytes[0] & 0x80) != 0)
                {
                    valueBytes.Insert(0, 0x00);
                }
            }

            // 3. 编码长度
            byte[] lengthBytes = EncodeLength((byte)valueBytes.Count);

            // 拼接TLV
            return Combine(tagBytes, lengthBytes, valueBytes.ToArray());
        }

        /// <summary>
        /// 编码UTF8字符串
        /// </summary>
        /// <param name="value">字符串内容</param>
        /// <returns>BER编码后的字节数组</returns>
        public static byte[] Encode(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            // 1. 编码标签（通用类+原始类型+UTF8字符串标签）
            byte[] tagBytes = EncodeTag(UniversalClass, isConstructed: false, Utf8StringTag);

            // 2. 编码值（UTF8字节）
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);

            // 3. 编码长度
            byte[] lengthBytes = EncodeLength((byte)valueBytes.Length);

            // 拼接TLV
            return Combine(tagBytes, lengthBytes, valueBytes);
        }

        /// <summary>
        /// 编码序列（构造类型，包含多个“完整 TLV 子项”）。
        /// </summary>
        /// <param name="elements">
        /// 子项的 BER 编码（每个元素必须是完整的 TLV：Tag + Length + Item1）。
        /// </param>
        /// <returns>SEQUENCE 的完整 TLV 字节数组。</returns>
        public static byte[] Encode(params byte[][] elements)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            // 1) 外层 Tag（通用类 + 构造类型 + TagNumber=0x10 -> 0x30）
            byte[] tagBytes = EncodeTag(UniversalClass, isConstructed: true, SequenceTag);

            // 2) 将所有子项（完整 TLV）拼接为 Item1
            List<byte> valueBytes = new List<byte>();
            foreach (var elem in elements)
            {
                valueBytes.AddRange(elem);
            }

            // 3) 外层 Length（子项 TLV 总长度）
            byte[] lengthBytes = EncodeLength((byte)valueBytes.Count);

            // 4) 拼接 TLV
            return Combine(tagBytes, lengthBytes, valueBytes.ToArray());
        }

        /// <summary>
        /// 编码标签（Tag）
        /// </summary>
        /// <param name="tagClass">标签类别（Class）</param>
        /// <param name="isConstructed">是否为构造类型</param>
        /// <param name="tagNumber">标签号</param>
        /// <returns>编码后的标签字节</returns>
        public static byte[] EncodeTag(byte tagClass, bool isConstructed, int tagNumber)
        {
            if (tagNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(tagNumber));

            List<byte> tagBytes = new List<byte>();
            byte firstByte = tagClass;
            // 设置构造类型标志（第6位）
            if (isConstructed)
            {
                firstByte |= 0x20;
            }

            // 标签号处理：<=31用单字节，>31用多字节
            if (tagNumber <= 31)
            {
                firstByte |= (byte)tagNumber;
                tagBytes.Add(firstByte);
            }
            else
            {
                firstByte |= 0x1F; // 标记为多字节模式
                tagBytes.Add(firstByte);

                // 多字节编码（每个字节最高位为1，最后一个为0）
                ByteBlock numberBlock = new ByteBlock();
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
                    tagBytes.Add(t);
                }
            }

            return tagBytes.ToArray();
        }

        /// <summary>
        /// 编码长度（Length）
        /// </summary>
        /// <param name="length">内容长度</param>
        /// <returns>编码后的长度字节</returns>
        public static byte[] EncodeLength(int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            // 短格式（长度<128）
            if (length <= 0x7F)
            {
                return new[] { (byte)length };
            }

            // 长格式（长度>=128）
            List<byte> lengthBytes = new List<byte>();
            int byteCount = 0;
            int temp = length;
            // 计算需要的字节数
            while (temp > 0)
            {
                lengthBytes.Add((byte)(temp & 0xFF));
                temp >>= 8;
                byteCount++;
            }
            lengthBytes.Reverse(); // 转为大端序
                                   // 第一个字节：0x80 + 字节数
            lengthBytes.Insert(0, (byte)(0x80 | byteCount));

            return lengthBytes.ToArray();
        }

        /// <summary>
        /// 编码整数类型
        /// </summary>
        /// <param name="value">整数数值</param>
        /// <returns>BER编码后的字节数组</returns>
        public static void Encode(ref ByteBlock block, long value)
        {
            // 写标签：Universal, Primitive, INTEGER
            EncodeTag(ref block, UniversalClass, false, IntegerTag);

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
        /// 编码UTF8字符串
        /// </summary>
        /// <param name="block">字节块</param>
        /// <param name="value">字符串内容</param>
        /// <exception cref="ArgumentNullException">当value为空时触发</exception>
        public static void Encode(ref ByteBlock block, string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            EncodeTag(ref block, UniversalClass, false, Utf8StringTag);

            Encoding encoding = Encoding.UTF8;
            int length = encoding.GetByteCount(value);
            EncodeLength(ref block, length);

            Span<byte> span = block.GetSpan(length);
            Encoding.UTF8.GetBytes(value, span);
            block.WriteAdvance(length);
        }

        /// <summary>
        /// 将 <paramref name="buffer"/> 的剩余内容作为 SEQUENCE 的 Item1 写入（构造类型）。
        /// </summary>
        /// <param name="block">目标块，写入 SEQUENCE 的 TLV。</param>
        /// <param name="buffer">作为 Item1 的内容；应由若干完整 TLV 子项顺序拼接而成。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为空时抛出。</exception>
        /// <remarks>本方法仅包裹外层 Tag 与 Length，不修改 <paramref name="buffer"/> 内的子项结构。</remarks>
        public static void Encode(ref ByteBlock block, ByteBuffer buffer)
        {
            if (buffer.IsEmpty)
                throw new ArgumentNullException(nameof(buffer));

            EncodeTag(ref block, UniversalClass, true, SequenceTag);
            EncodeLength(ref block, (int)buffer.Remaining);
            block.Write(buffer);
        }

        /// <summary>
        /// 将 <paramref name="buffer"/> 的未读内容作为 SEQUENCE 的 Item1 写入（构造类型）。
        /// </summary>
        /// <param name="block">目标块，写入 SEQUENCE 的 TLV。</param>
        /// <param name="buffer">作为 Item1 的内容；应由若干完整 TLV 子项顺序拼接而成。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为空时抛出。</exception>
        /// <remarks>本方法仅包裹外层 Tag 与 Length，不修改 <paramref name="buffer"/> 内的子项结构。</remarks>
        public static void Encode(ref ByteBlock block, ByteBlock buffer)
        {
            if (buffer.IsEmpty)
                throw new ArgumentNullException(nameof(buffer));

            EncodeTag(ref block, UniversalClass, true, SequenceTag);
            EncodeLength(ref block, (int)buffer.Remaining);
            block.Write(buffer);
        }

        /// <summary>
        /// 将若干完整 TLV 子项打包为 SEQUENCE 并写入 <paramref name="block"/>（构造类型）。
        /// </summary>
        /// <param name="block">目标块，写入 SEQUENCE 的 TLV。</param>
        /// <param name="elements">每个元素必须是完整 TLV（Tag + Length + Item1）。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="elements"/> 为空时抛出。</exception>
        public static void Encode(ref ByteBlock block, params byte[][] elements)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            EncodeTag(ref block, UniversalClass, true, SequenceTag);
            var buffer = ByteBuffer.CreateBuffer();
            buffer.Write(elements);
            EncodeLength(ref block, (int)buffer.Remaining);
            block.Write(buffer);
            buffer.Dispose();
        }

        /// <summary>
        /// 编码标签（Tag）
        /// </summary>
        /// <param name="block">字节块引用</param>
        /// <param name="tagClass">标签类别（Class）</param>
        /// <param name="isConstructed">是否为构造类型</param>
        /// <param name="tagNumber">标签号</param>
        public static void EncodeTag(ref ByteBlock block, byte tagClass, bool isConstructed, int tagNumber)
        {
            if (tagNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(tagNumber));

            byte firstByte = tagClass;
            // 设置构造类型标志（第6位）
            firstByte = isConstructed ? firstByte |= 0x20 : firstByte;

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
        /// 编码长度（Length）
        /// </summary>
        /// <param name="block">字节块引用</param>
        /// <param name="length">内容长度</param>
        /// <returns>编码后的长度字节</returns>
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

        #endregion 编码方法

        #region 解码方法

        /// <summary>
        /// 解码BER数据为整数
        /// </summary>
        /// <param name="berData">BER编码数据</param>
        /// <returns>解码后的整数</returns>
        public static long DecodeInteger(byte[] berData)
        {
            using (MemoryStream ms = new MemoryStream(berData))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                // 解析标签
                var (_, isConstructed, tagNumber) = DecodeTag(reader);
                if (isConstructed || tagNumber != IntegerTag)
                {
                    throw new InvalidDataException("不是整数类型的BER数据");
                }

                // 解析长度
                int length = DecodeLength(reader);
                if (length <= 0)
                {
                    throw new InvalidDataException("无效的整数长度");
                }

                // 按长度读取值字节（大端补码）
                byte[] srcArr = reader.ReadBytes(length);
                ReadOnlySpan<byte> src = srcArr.AsSpan();

                bool negative = (src[0] & 0x80) != 0;

                // 规整为 8 字节大端（符号扩展）
                Span<byte> big = stackalloc byte[8];
                big.Fill(negative ? (byte)0xFF : (byte)0x00);

                if (length <= 8)
                {
                    src.CopyTo(big.Slice(8 - length));
                }
                else
                {
                    // 长度超过 8，需验证高位是否仅为符号扩展
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
        }

        /// <summary>
        /// 解码BER数据为UTF8字符串
        /// </summary>
        /// <param name="berData">BER编码数据</param>
        /// <returns>解码后的字符串</returns>
        public static string DecodeUtf8String(byte[] berData)
        {
            using (MemoryStream ms = new MemoryStream(berData))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                // 解析标签
                var (tagClass, isConstructed, tagNumber) = DecodeTag(reader);
                if (tagNumber != Utf8StringTag)
                {
                    throw new InvalidDataException("不是UTF8字符串类型的BER数据");
                }

                // 解析长度
                int length = DecodeLength(reader);

                // 解析值
                byte[] valueBytes = reader.ReadBytes(length);
                return Encoding.UTF8.GetString(valueBytes);
            }
        }

        /// <summary>
        /// 解码序列（返回子元素列表）
        /// </summary>
        /// <param name="berData">BER编码数据</param>
        /// <returns>子元素的BER编码数组</returns>
        public static byte[][] DecodeSequence(byte[] berData)
        {
            using (MemoryStream ms = new MemoryStream(berData))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                // 解析标签
                var (tagClass, isConstructed, tagNumber) = DecodeTag(reader);
                if (tagNumber != SequenceTag || !isConstructed)
                {
                    throw new InvalidDataException("不是序列类型的BER数据");
                }

                // 解析长度
                int length = DecodeLength(reader);
                byte[] sequenceData = reader.ReadBytes(length);

                // 解析子元素
                List<byte[]> elements = new List<byte[]>();
                using (MemoryStream seqMs = new MemoryStream(sequenceData))
                using (BinaryReader seqReader = new BinaryReader(seqMs))
                {
                    while (seqMs.Position < seqMs.Length)
                    {
                        // 读取子元素的标签
                        long tagStart = seqMs.Position;
                        var (_, _, _) = DecodeTag(seqReader);
                        int tagLength = (int)(seqMs.Position - tagStart);

                        // 读取子元素的长度
                        long lengthStart = seqMs.Position;
                        int valueLength = DecodeLength(seqReader);
                        int lengthLength = (int)(seqMs.Position - lengthStart);

                        // 读取子元素的值
                        byte[] valueBytes = seqReader.ReadBytes(valueLength);

                        // 拼接子元素的TLV
                        byte[] tagBytes = new byte[tagLength];
                        Array.Copy(sequenceData, tagStart, tagBytes, 0, tagLength);

                        byte[] lengthBytes = new byte[lengthLength];
                        Array.Copy(sequenceData, lengthStart, lengthBytes, 0, lengthLength);

                        elements.Add(Combine(tagBytes, lengthBytes, valueBytes));
                    }
                }

                return elements.ToArray();
            }
        }

        /// <summary>
        /// 解码标签（Tag）
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        /// <returns>标签类别、是否构造类型、标签号</returns>
        public static (byte tagClass, bool isConstructed, int tagNumber) DecodeTag(BinaryReader reader)
        {
            byte firstByte = reader.ReadByte();
            byte tagClass = (byte)(firstByte & 0xC0); // 高2位：类别
            bool isConstructed = (firstByte & 0x20) != 0; // 第6位：构造类型标志
            int tagNumber = firstByte & 0x1F; // 低5位：标签号（可能是单字节或多字节标志）

            // 处理多字节标签号
            if (tagNumber == 0x1F)
            {
                tagNumber = 0;
                byte b;
                do
                {
                    b = reader.ReadByte();
                    tagNumber = (tagNumber << 7) | (b & 0x7F);
                } while ((b & 0x80) != 0); // 最高位为1表示后续还有字节
            }

            return (tagClass, isConstructed, tagNumber);
        }

        /// <summary>
        /// 解码长度（Length）
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        /// <returns>内容长度</returns>
        public static int DecodeLength(BinaryReader reader)
        {
            byte firstByte = reader.ReadByte();
            if ((firstByte & 0x80) == 0)
            {
                // 短格式
                return firstByte;
            }
            else
            {
                // 长格式：计算后续字节数
                int byteCount = firstByte & 0x7F;
                if (byteCount == 0)
                {
                    throw new InvalidDataException("不支持无限长度格式");
                }
                if (byteCount > 4)
                {
                    throw new InvalidDataException("长度超过支持的最大范围（4字节）");
                }

                byte[] lengthBytes = reader.ReadBytes(byteCount);
                Array.Reverse(lengthBytes); // 转为小端序以便BitConverter处理
                lengthBytes = PadTo4Bytes(lengthBytes); // 补全为4字节

                return BitConverter.ToInt32(lengthBytes, 0);
            }
        }

        /// <summary>
        /// 从 <see cref="ByteBlock"/> 解码 INTEGER（有符号整型，最短补码表示）。
        /// </summary>
        /// <param name="block">数据源（读指针将推进）。</param>
        /// <returns>解码得到的 Int64 值。</returns>
        /// <exception cref="InvalidDataException">当标签不是 INTEGER 或长度非法时抛出。</exception>
        /// <exception cref="OverflowException">当值超出 Int64 可表示范围时抛出。</exception>
        public static long DecodeInteger(ref ByteBlock block)
        {
            var (_, isConstructed, tagNumber) = DecodeTag(ref block);
            if (isConstructed || tagNumber != IntegerTag)
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
        #endregion

        #region 辅助方法

        /// <summary>
        /// 拼接字节数组
        /// </summary>
        private static byte[] Combine(params byte[][] arrays)
        {
            int totalLength = 0;
            foreach (var arr in arrays)
            {
                totalLength += arr.Length;
            }
            byte[] result = new byte[totalLength];
            int offset = 0;
            foreach (var arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }
            return result;
        }

        /// <summary>
        /// 补全字节数组为8字节（用于整数转换）
        /// </summary>
        private static byte[] PadTo8Bytes(byte[] bytes)
        {
            if (bytes.Length >= 8)
            {
                return bytes;
            }
            byte[] padded = new byte[8];
            Array.Copy(bytes, 0, padded, 8 - bytes.Length, bytes.Length);
            return padded;
        }

        /// <summary>
        /// 补全字节数组为4字节（用于长度转换）
        /// </summary>
        private static byte[] PadTo4Bytes(byte[] bytes)
        {
            if (bytes.Length >= 4)
            {
                return bytes;
            }
            byte[] padded = new byte[4];
            Array.Copy(bytes, 0, padded, 4 - bytes.Length, bytes.Length);
            return padded;
        }

        #endregion 辅助方法
    }
}