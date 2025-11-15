using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Encodings;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// 封装 SNMP 值的轻量结构体，承载一组 <see
    /// cref="DataBuffer"/>（通过 <see
    /// cref="ValueList{T}"/> 管理）。
    /// </summary>
    public struct SnmpValue : IDisposable
    {
        /// <summary>
        /// 表示空值的静态只读实例（对应 ASN.1 NULL 提示）。
        /// </summary>
        public static SnmpValue Empty => new(SnmpDataType.Null, DataBuffer.Empty);

        /// <summary>
        /// 表示此值的预期 SNMP 数据类型（作为解析/格式化的提示）。
        /// 注意：Agent 返回的实际 ASN.1 tag 才是最终准则，本字段仅作便捷提示。
        /// </summary>
        public SnmpDataType Type { get; private set; }

        /// <summary>
        /// 存放实际数据的缓冲列表（ <see cref="ValueList{DataBuffer}"/>）。
        /// - 常见用法：对于简单类型（Integer/OctetString），该列表通常包含
        ///   1 个 <see cref="DataBuffer{T}"/>； 对于构造类型（Sequence）可能包含多个缓冲区表示子元素。
        /// - 调用方对该字段负责初始化并在不再使用时确保调用 <see
        ///   cref="Dispose"/> 回收内部缓冲。
        /// </summary>
        public DataBuffer buffer { get; }

        public bool IsEmpty => Type == SnmpDataType.Null || DataBuffer.IsEmptyOrNull(buffer);

        /// <summary>
        /// 创建一个指定类型的 <see
        /// cref="SnmpValue"/>（未分配内部缓冲，调用方可随后填充
        /// <see cref="buffer"/>）。
        /// </summary>
        /// <param name="type">值的类型提示。</param>
        public SnmpValue(SnmpDataType type, DataBuffer buffer) : this()
        {
            Type = type;
            this.buffer = buffer;
        }

        /// <summary>
        /// 返回值的可读字符串表示（委托给内部 <see
        /// cref="ValueList{DataBuffer}.ToString"/>）。
        /// 若内部未初始化则返回 " <c>&lt;null&gt;</c>"。
        /// </summary>
        /// <returns>用于日志或 UI 的字符串表示。</returns>
        public override string ToString()
        {
            // ValueList<T>.ToString() 应实现友好显示；防御性地处理未初始化情况。
            try
            {
                return buffer.ToString() ?? "<null>";
            }
            catch
            {
                return "<null>";
            }
        }

        /// <summary>
        /// 释放内部资源：调用 <see
        /// cref="ValueList{T}.Dispose"/> 回收其使用的池资源/数组。
        /// </summary>
        public void Dispose()
        {
            buffer.Release();
        }

        internal void Encode(ref ByteBlock block)
        {
            if (DataBuffer.IsEmptyOrNull(buffer))
            {
                BEREncoding.EncodeNull(ref block);
                return;
            }

            switch (Type)
            {
                case SnmpDataType.Null:
                    BEREncoding.EncodeNull(ref block);
                    break;

                case SnmpDataType.Integer:
                    if (buffer is DataBuffer<long> longBuffer)
                    {
                        BEREncoding.Encode(ref block, longBuffer.Item1);
                    }
                    else if (buffer is DataBuffer<int> intBuffer)
                    {
                        BEREncoding.Encode(ref block, intBuffer.Item1);
                    }
                    else
                    {
                        throw new InvalidOperationException("无法获得数字");
                    }
                    break;

                case SnmpDataType.OctetString:
                    if (buffer is DataBuffer<ByteBlock> byteBuffer)
                    {
                        BEREncoding.EncodeOctetString(ref block, byteBuffer.Item1);
                    }
                    else
                    {
                        throw new InvalidOperationException("无法获得字节数组");
                    }
                    break;

                default:
                    throw new NotImplementedException($"Encoding for SNMP data type {Type} is not implemented.");
            }
        }

        public static SnmpValue Create<T>(T value)
        {
            if (value == null)
            {
                return Empty;
            }

            return value switch
            {
                int intValue => new SnmpValue(SnmpDataType.Integer, DataBuffer<int>.Get(intValue)),
                long longValue => new SnmpValue(SnmpDataType.Integer, DataBuffer<long>.Get(longValue)),
                byte[] byteArray => new SnmpValue(SnmpDataType.OctetString, DataBuffer<ByteBlock>.Get(new ByteBlock(byteArray))),
                ByteBlock byteBlock => new SnmpValue(SnmpDataType.OctetString, DataBuffer<ByteBlock>.Get(byteBlock)),
                _ => throw new NotSupportedException($"Cannot create SnmpValue from type {typeof(T).FullName}."),
            };
        }

        public static implicit operator SnmpValue(int value)
        {
            return Create(value);
        }

        public static implicit operator SnmpValue(long value)
        {
            return Create(value);
        }

        public static implicit operator SnmpValue(byte[] value)
        {
            return Create(value);
        }

        public static implicit operator SnmpValue(ByteBlock value)
        {
            return Create(value);
        }
    }
}