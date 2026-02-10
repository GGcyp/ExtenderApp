using ExtenderApp.Buffer;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供对象与二进制之间的序列化/反序列化契约（内存/序列级别）。
    /// <para>本接口侧重内存/序列操作：从内存或序列反序列化为对象，或将对象序列化为内存/缓冲。</para>
    /// </summary>
    public interface ISerialization
    {
        /// <summary>
        /// 将指定对象序列化为字节数组（完全副本）。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="value">要序列化的对象实例。</param>
        /// <returns>包含序列化结果的字节数组（非 null，但长度可能为0，视实现而定）。</returns>
        byte[] Serialize<T>(T value);

        /// <summary>
        /// 将指定对象序列化并写入到调用方提供的字节跨度（<see cref="Span{Byte}"/>）中。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="value">要序列化的对象实例。</param>
        /// <param name="span">调用方提供的字节跨度，用于接收序列化结果。实现应尽量遵守长度约定，或在写入前验证 <paramref name="span"/> 大小。</param>
        void Serialize<T>(T value, Span<byte> span);

        /// <summary>
        /// 将指定对象序列化并输出到调用方提供的顺序缓冲（<see cref="AbstractBuffer{Byte}"/>）。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="value">要序列化的对象实例。</param>
        /// <param name="buffer">输出参数：被填充的 <see cref="AbstractBuffer{byte}"/>，调用方负责生命周期约定。</param>
        void Serialize<T>(T value, out AbstractBuffer<byte> buffer);

        /// <summary>
        /// 将指定对象序列化并写入到调用方提供的顺序缓冲（<see cref="AbstractBuffer{Byte}"/>）。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="value">要序列化的对象实例。</param>
        /// <param name="buffer">调用方提供的缓冲实例；实现应在写入时遵守调用方的生命周期约定并在必要时抛出或返回错误。</param>
        void Serialize<T>(T value, AbstractBuffer<byte> buffer);

        /// <summary>
        /// 从缓冲读取器中 <see cref="AbstractBufferReader<byte>"/>反序列化出目标类型实例。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="memories">包含序列化数据的只读序列（可能由多段组成）。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或反序列化失败可返回 null（或由实现抛出异常）。</returns>
        T? Deserialize<T>(AbstractBufferReader<byte> reader);
    }
}