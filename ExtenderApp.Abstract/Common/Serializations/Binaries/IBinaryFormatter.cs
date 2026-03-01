using System.Buffers;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供二进制格式化器的元数据和默认行为。 实现者描述如何将值序列化为二进制表示以及如何从二进制表示反序列化。
    /// </summary>
    public interface IBinaryFormatter
    {
        /// <summary>
        /// 获取此格式化器在序列化值时的默认字节长度提示。 可用于预分配缓冲区以提高性能。
        /// </summary>
        int DefaultLength { get; }

        /// <summary>
        /// 获取用于描述格式化器方法信息的详细数据， 包含发现或调用序列化/反序列化方法所需的元数据。
        /// </summary>
        BinaryFormatterMethodInfoDetails MethodInfoDetails { get; }
    }

    /// <summary>
    /// 负责序列化和反序列化类型 <typeparamref name="T"/> 的二进制格式化器接口。
    /// </summary>
    /// <typeparam name="T">要序列化/反序列化的目标类型。</typeparam>
    public interface IBinaryFormatter<T> : IBinaryFormatter
    {
        /// <summary>
        /// 将 <paramref name="value"/> 序列化到 <see cref="BinaryWriterAdapter"/> 中。
        /// </summary>
        /// <param name="writer">用于写入字节的二进制写入器。</param>
        /// <param name="value">要序列化的值。</param>
        void Serialize(ref BinaryWriterAdapter writer, T value);

        /// <summary>
        /// 将 <paramref name="value"/> 序列化到 <see cref="SpanWriter{Byte}"/> 中。
        /// </summary>
        /// <param name="writer">用于写入字节的 Span 写入器。</param>
        /// <param name="value">要序列化的值。</param>
        void Serialize(ref SpanWriter<byte> writer, T value);

        /// <summary>
        /// 从 <see cref="BinaryReaderAdapter"/> 中反序列化出 <typeparamref name="T"/> 类型的值。
        /// </summary>
        /// <param name="reader">用于读取字节的二进制读取器。</param>
        /// <returns>反序列化得到的值。</returns>
        T Deserialize(ref BinaryReaderAdapter reader);

        /// <summary>
        /// 从 <see cref="SpanReader{Byte}"/> 中反序列化出 <typeparamref name="T"/> 类型的值。
        /// </summary>
        /// <param name="reader">用于读取字节的 Span 读取器。</param>
        /// <returns>反序列化得到的值。</returns>
        T Deserialize(ref SpanReader<byte> reader);

        /// <summary>
        /// 获取序列化指定 <paramref name="value"/> 所需的字节长度。
        /// </summary>
        /// <param name="value">要测量的值。</param>
        /// <returns>序列化该值所需的字节数。</returns>
        long GetLength(T value);
    }
}