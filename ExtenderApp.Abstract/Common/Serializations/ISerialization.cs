using ExtenderApp.Buffer;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供通用的序列化和反序列化功能，用于将对象与二进制表示相互转换。
    /// </summary>
    public interface ISerialization
    {
        /// <summary>
        /// 将给定的值序列化为字节数组。
        /// </summary>
        /// <typeparam name="T">要序列化的类型。</typeparam>
        /// <param name="value">要序列化的值。</param>
        /// <returns>返回包含序列化结果的字节数组。</returns>
        byte[] Serialize<T>(T value);

        /// <summary>
        /// 将给定的值序列化到提供的 <see cref="SpanWriter{Byte}"/> 中。
        /// </summary>
        /// <typeparam name="T">要序列化的类型。</typeparam>
        /// <param name="writer">用于写入序列化数据的 Span 写入器（引用传递）。</param>
        /// <param name="value">要序列化的值。</param>
        void Serialize<T>(ref SpanWriter<byte> writer, T value);

        /// <summary>
        /// 将给定的值序列化到提供的 <see cref="BinaryWriterAdapter"/> 中。
        /// </summary>
        /// <typeparam name="T">要序列化的类型。</typeparam>
        /// <param name="writer">用于写入序列化数据的二进制写入器。</param>
        /// <param name="value">要序列化的值。</param>
        void Serialize<T>(ref BinaryWriterAdapter writer, T value);

        /// <summary>
        /// 将给定的值序列化并通过输出参数返回一个 <see cref="SequenceBuffer{Byte}"/>。
        /// </summary>
        /// <typeparam name="T">要序列化的类型。</typeparam>
        /// <param name="value">要序列化的值。</param>
        /// <param name="buffer">输出参数，包含序列化后数据的序列缓冲区。</param>
        void Serialize<T>(T value, out AbstractBuffer<byte> buffer);

        /// <summary>
        /// 从 <see cref="SpanReader{Byte}"/> 中反序列化出指定类型的值。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="reader">用于读取数据的 Span 读取器（引用传递）。</param>
        /// <returns>反序列化得到的值，或 null。</returns>
        T? Deserialize<T>(ref SpanReader<byte> reader);

        /// <summary>
        /// 从 <see cref="BinaryReaderAdapter"/> 中反序列化出指定类型的值。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="reader">用于读取数据的二进制读取器（引用传递）。</param>
        /// <returns>反序列化得到的值，或 null。</returns>
        T? Deserialize<T>(ref BinaryReaderAdapter reader);
    }
}