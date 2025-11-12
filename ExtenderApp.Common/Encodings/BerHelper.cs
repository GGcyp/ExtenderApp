

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
        public const int SequenceTag = 0x30; // 序列（构造类型）

        #endregion

        #region 编码方法
        /// <summary>
        /// 编码整数类型
        /// </summary>
        /// <param name="value">整数数值</param>
        /// <returns>BER编码后的字节数组</returns>
        public static byte[] EncodeInteger(long value)
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
        public static byte[] EncodeUtf8String(string value)
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
        /// 编码序列（构造类型，包含多个子元素）
        /// </summary>
        /// <param name="elements">子元素的BER编码数组</param>
        /// <returns>BER编码后的字节数组</returns>
        public static byte[] EncodeSequence(params byte[][] elements)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            // 1. 编码标签（通用类+构造类型+序列标签）
            byte[] tagBytes = EncodeTag(UniversalClass, isConstructed: true, SequenceTag);

            // 2. 拼接子元素作为序列值
            List<byte> valueBytes = new List<byte>();
            foreach (var elem in elements)
            {
                valueBytes.AddRange(elem);
            }

            // 3. 编码长度
            byte[] lengthBytes = EncodeLength((byte)valueBytes.Count);

            // 拼接TLV
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
        public static void EncodeInteger(ref ByteBlock block, long value)
        {
            // 1. 编码标签（通用类+原始类型+整数标签）
            EncodeTag(ref block, UniversalClass, false, IntegerTag);

            // 2. 编码值（二进制补码形式，去除前导零）
            ulong unsignedValue = (ulong)Math.Abs(value);

            // 处理零值
            if (unsignedValue == 0)
            {
                EncodeLength(ref block, 1);
                block.Write(0x00);
            }
            else
            {
                // 提取字节（大端序）
                ByteBlock valueBlock = new();
                while (unsignedValue > 0)
                {
                    valueBlock.Write((byte)(unsignedValue & 0xFF));
                    unsignedValue >>= 8;
                }

                EncodeLength(ref block, valueBlock.Length);
                // 反转字节以符合大端序
                valueBlock.Reverse();
                byte[] bytes = ArrayPool<byte>.Shared.Rent(valueBlock.Length);

                // 处理负数（补码调整）
                if (value < 0)
                {
                    var valueSpan = valueBlock.UnreadSpan;
                    for (int i = 0; i < valueSpan.Length; i++)
                    {
                        bytes[i] = (byte)~valueSpan[i];
                    }
                    // 加1完成补码
                    int carry = 1;
                    for (int i = bytes.Length - 1; i >= 0 && carry > 0; i--)
                    {
                        int temp = bytes[i] + carry;
                        bytes[i] = (byte)(temp & 0xFF);
                        carry = temp >> 8;
                    }
                    valueBlock.Reset();
                    valueBlock.Write(bytes);

                    if (carry > 0)
                    {
                        block.Write(0xFF);// 溢出时补高位
                    }
                }

                // 确保无符号数最高位不为1（避免被误判为负数）
                if (value >= 0 && (bytes[0] & 0x80) != 0)
                {
                    block.Write(0x00);
                }

                block.Write(valueBlock);
                ArrayPool<byte>.Shared.Return(bytes);
                valueBlock.Dispose();
            }

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

        #endregion

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
                var (tagClass, isConstructed, tagNumber) = DecodeTag(reader);
                if (tagNumber != IntegerTag)
                {
                    throw new InvalidDataException("不是整数类型的BER数据");
                }

                // 解析长度
                int length = DecodeLength(reader);
                if (length <= 0)
                {
                    throw new InvalidDataException("无效的整数长度");
                }

                // 解析值
                byte[] valueBytes = reader.ReadBytes(length);
                bool isNegative = (valueBytes[0] & 0x80) != 0;

                if (isNegative)
                {
                    // 负数：补码转原码
                    for (int i = 0; i < valueBytes.Length; i++)
                    {
                        valueBytes[i] = (byte)~valueBytes[i];
                    }
                    // 加1
                    int carry = 1;
                    for (int i = valueBytes.Length - 1; i >= 0 && carry > 0; i--)
                    {
                        int temp = valueBytes[i] + carry;
                        valueBytes[i] = (byte)(temp & 0xFF);
                        carry = temp >> 8;
                    }
                    // 转换为负数
                    return -BitConverter.ToInt64(PadTo8Bytes(valueBytes), 0);
                }
                else
                {
                    // 正数
                    return BitConverter.ToInt64(PadTo8Bytes(valueBytes), 0);
                }
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
        #endregion
    }
}
