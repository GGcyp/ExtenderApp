using System.Formats.Asn1;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// 封装 SNMP 值的轻量结构体，承载一组 <see cref="DataBuffer"/>（通过 <see cref="ValueList{T}"/> 管理）。
    /// </summary>
    public readonly struct SnmpValue : IDisposable
    {
        /// <summary>
        /// 表示空值的静态只读实例（对应 ASN.1 NULL 提示）。
        /// </summary>
        public static SnmpValue Empty => new(UniversalTagNumber.Null, DataBuffer.Empty);

        /// <summary>
        /// 表示此值的预期 SNMP 数据类型（作为解析/格式化的提示）。 注意：Agent 返回的实际 ASN.1 tag 才是最终准则，本字段仅作便捷提示。
        /// </summary>
        public UniversalTagNumber Type { get; }

        /// <summary>
        /// 存放实际数据的缓冲列表（ <see cref="ValueList{DataBuffer}"/>）。
        /// - 常见用法：对于简单类型（Integer/OctetString），该列表通常包含 1 个 <see cref="DataBuffer{T}"/>； 对于构造类型（Sequence）可能包含多个缓冲区表示子元素。
        /// - 调用方对该字段负责初始化并在不再使用时确保调用 <see cref="Dispose"/> 回收内部缓冲。
        /// </summary>
        public DataBuffer Buffer { get; }

        /// <summary>
        /// 获取当前值是否为空（类型为 Null 或内部缓冲列表为空）。
        /// </summary>
        public bool IsEmpty => Type == UniversalTagNumber.Null || DataBuffer.IsEmptyOrNull(Buffer);

        /// <summary>
        /// 创建一个指定类型的 <see cref="SnmpValue"/>（未分配内部缓冲，调用方可随后填充 <see cref="Buffer"/>）。
        /// </summary>
        /// <param name="type">值的类型提示。</param>
        public SnmpValue(UniversalTagNumber type, DataBuffer buffer) : this()
        {
            Type = type;
            Buffer = buffer;
        }

        /// <summary>
        /// 返回值的可读字符串表示（委托给内部 <see cref="ValueList{DataBuffer}.ToString"/>）。 若内部未初始化则返回 " <c>&lt;null&gt;</c>"。
        /// </summary>
        /// <returns>用于日志或 UI 的字符串表示。</returns>
        public override string ToString()
        {
            // ValueList<TLinkClient>.ToString() 应实现友好显示；防御性地处理未初始化情况。
            try
            {
                return Buffer.ToString() ?? "<null>";
            }
            catch
            {
                return "<null>";
            }
        }

        /// <summary>
        /// 释放内部资源：调用 <see cref="ValueList{T}.Dispose"/> 回收其使用的池资源/数组。
        /// </summary>
        public void Dispose()
        {
            Buffer.Release();
        }

        public static implicit operator SnmpValue(int value)
        {
            return new SnmpValue(UniversalTagNumber.Integer, DataBuffer<long>.Get(value));
        }

        public static implicit operator SnmpValue(long value)
        {
            return new SnmpValue(UniversalTagNumber.Integer, DataBuffer<long>.Get(value));
        }

        public static implicit operator SnmpValue(byte[] value)
        {
            return default;
            //return new SnmpValue(UniversalTagNumber.OctetString, DataBuffer<ByteBlock>.Get(new ByteBlock(value)));
        }

        public static implicit operator SnmpValue(ByteBlock value)
        {
            return new SnmpValue(UniversalTagNumber.OctetString, DataBuffer<ByteBlock>.Get(value));
        }
    }
}