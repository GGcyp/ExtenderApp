using ExtenderApp.Buffer;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供对象与二进制之间的序列化/反序列化契约（内存/序列级别）。 本接口侧重内存/序列操作：从内存或序列反序列化为对象，或将对象序列化为内存/缓冲。
    /// </summary>
    public interface ISerialization
    {
        /// <summary>
        /// 将指定对象序列化为字节数组（完全副本）。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="value">要序列化的对象实例。</param>
        /// <returns>包含序列化结果的字节数组（非 null，但长度可能为 0，视实现而定）。</returns>
        byte[] Serialize<T>(T value);

        /// <summary>
        /// 将指定对象序列化并写入到调用方提供的字节跨度（ <see cref="Span{Byte}"/>）中。 实现应在写入前校验或信任调用方提供的缓冲长度并按约定写入数据。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="value">要序列化的对象实例。</param>
        /// <param name="span">目标栈上写入器（按引用传递），方法在成功写入后会推进写入器的位置。注意 <c>SpanWriter</c> 为 <c>ref struct</c>，具有栈生命周期限制。</param>
        void Serialize<T>(T value, ref SpanWriter<byte> span);

        /// <summary>
        /// 将指定对象序列化并写入到调用方提供的顺序缓冲（ <see cref="AbstractBuffer{byte}"/>）。 实现应将序列化结果写入该缓冲，调用方负责缓冲的生命周期与管理。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="value">要序列化的对象实例。</param>
        /// <param name="buffer">调用方提供的目标缓冲；实现应将序列化结果写入该缓冲并按约定推进其已提交/已写入位置（或在文档中注明语义）。</param>
        void Serialize<T>(T value, AbstractBuffer<byte> buffer);

        /// <summary>
        /// 将指定对象序列化并输出为新的顺序缓冲（ <see cref="AbstractBuffer{byte}"/>）。 实现应分配或填充输出缓冲，调用方负责对该缓冲的释放与生命周期管理。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="value">要序列化的对象实例。</param>
        /// <param name="buffer">输出参数：被填充的目标缓冲，方法应为调用方分配或填充该缓冲并返回。</param>
        void Serialize<T>(T value, out AbstractBuffer<byte> buffer);

        /// <summary>
        /// 从给定的只读字节跨度中反序列化出目标类型的实例。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="span">包含序列化数据的只读字节跨度（可能由调用方从更大序列中切片）。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或反序列化失败可返回 <c>null</c>（或由实现抛出异常）。</returns>
        T? Deserialize<T>(ReadOnlySpan<byte> span);

        /// <summary>
        /// 从给定的缓冲读取器中反序列化出目标类型实例。 实现应从读取器中读取并按约定推进其已消费位置，读取失败时可返回 <c>null</c> 或抛出异常。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="reader">包含序列化数据的栈上读取器（按引用传递），方法在成功读取后会推进读取器的位置。注意 <c>SpanReader</c> 为 <c>ref struct</c>，具有栈生命周期限制。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或反序列化失败可返回 <c>null</c>（或由实现抛出异常）。</returns>
        T? Deserialize<T>(ref SpanReader<byte> reader);

        /// <summary>
        /// 从给定的顺序缓冲中反序列化出目标类型的实例。 实现应从缓冲中读取并按约定推进其已消费位置。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="buffer">包含序列化数据的顺序缓冲（可能由多段组成）。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或反序列化失败可返回 <c>null</c>（或由实现抛出异常）。</returns>
        T? Deserialize<T>(AbstractBuffer<byte> buffer);

        /// <summary>
        /// 从缓冲读取器中反序列化出目标类型实例。 实现应从读取器中读取并按约定推进其已消费位置，读取失败时可返回 <c>null</c> 或抛出异常。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="reader">包含序列化数据的缓冲读取器（可能由多段组成）。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或反序列化失败可返回 <c>null</c>（或由实现抛出异常）。</returns>
        T? Deserialize<T>(AbstractBufferReader<byte> reader);
    }
}