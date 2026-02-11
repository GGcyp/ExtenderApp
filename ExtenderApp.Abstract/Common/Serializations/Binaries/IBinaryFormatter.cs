using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 二进制格式化器基础接口，提供序列化时的默认长度提示和方法信息元数据。
    /// </summary>
    public interface IBinaryFormatter
    {
        /// <summary>
        /// 序列化的默认预估长度（字节数）。
        /// 用于预留写缓冲的大小提示，实际写入可能与该值不同。
        /// </summary>
        int DefaultLength { get; }

        /// <summary>
        /// 二进制格式化器的方法信息详情（用于诊断或运行时代码生成场景）。
        /// </summary>
        BinaryFormatterMethodInfoDetails MethodInfoDetails { get; }
    }

    /// <summary>
    /// 针对类型 <typeparamref name="T"/> 的二进制序列化/反序列化器接口。
    /// 接口方法使用 <see cref="AbstractBuffer{byte}"/> 表示可供写入/读取的目标缓冲区抽象，避免直接依赖具体实现类型。
    /// </summary>
    /// <typeparam name="T">要序列化/反序列化的类型。</typeparam>
    /// <remarks>
    /// 此接口提供了基于段/缓冲与基于栈上 Span 读写器的重载。使用 <c>SpanReader{byte}</c> / <c>SpanWriter{byte}</c>
    /// 的重载采用 <c>ref</c> 参数传递以允许方法推进读取/写入位置。注意：<c>SpanReader</c> / <c>SpanWriter</c>
    /// 为 <c>ref struct</c>（栈上类型），因此在使用这些 API 时应遵循 <c>ref struct</c> 的限制（不可装箱、不可作为字段或跨异步边界）。
    /// 实现者也可以仅实现基于 <see cref="AbstractBuffer{byte}"/> 的成员，上层可通过适配器将 Span 路径映射到缓冲路径。
    /// </remarks>
    public interface IBinaryFormatter<T> : IBinaryFormatter
    {
        /// <summary>
        /// 将 <paramref name="value"/> 按实现约定的二进制格式写入到 <paramref name="buffer"/> 中。
        /// </summary>
        /// <param name="buffer">目标缓存（写入时由调用方提供或预分配）。</param>
        /// <param name="value">要序列化的值。</param>
        void Serialize(AbstractBuffer<byte> buffer, T value);

        /// <summary>
        /// 在给定的栈上写入器中序列化指定值并推进写入位置。
        /// </summary>
        /// <remarks>
        /// 该重载接收 <c>ref SpanWriter&lt;byte&gt;</c> 以允许实现推进写入位置并将更改反映给调用者。
        /// 由于 <c>SpanWriter</c> 为 <c>ref struct</c>，此方法不能在异步方法或将写入器装箱的上下文中使用。
        /// 若实现无法支持基于 Span 的高性能路径，可在实现中通过将数据写入临时 <see cref="AbstractBuffer{byte}"/> 来完成序列化。
        /// </remarks>
        /// <param name="writer">目标栈上写入器，按引用传递，方法将在成功写入后推进其位置。</param>
        /// <param name="value">要序列化的值。</param>
        void Serialize(ref SpanWriter<byte> writer, T value);

        /// <summary>
        /// 从给定的 <paramref name="buffer"/> 中读取并构造一个 <typeparamref name="T"/> 实例。
        /// </summary>
        /// <param name="buffer">来源缓存读取器（读取时推进其已消费位置）。</param>
        /// <returns>反序列化得到的 <typeparamref name="T"/> 实例。</returns>
        T Deserialize(AbstractBufferReader<byte> buffer);

        /// <summary>
        /// 从栈上读取器中反序列化一个值并推进读取位置。
        /// </summary>
        /// <remarks>
        /// 该重载采用 <c>ref SpanReader&lt;byte&gt;</c>（栈上类型）以允许实现推进读取位置并将更改反映给调用者。
        /// 注意 <c>SpanReader</c> 的生命周期限制：不可跨异步或装箱上下文传递。</remarks>
        /// <param name="buffer">要读取的栈上读取器，按引用传递，成功读取后其位置将被推进。</param>
        /// <returns>反序列化得到的目标值。</returns>
        T Deserialize(ref SpanReader<byte> buffer);

        /// <summary>
        /// 返回序列化指定值预计需要的字节数，用于预留写缓冲。
        /// 具体实现可返回精确值或合理的估算值。
        /// </summary>
        /// <param name="value">待估算长度的值。</param>
        /// <returns>序列化所需的字节数。</returns>
        long GetLength(T value);
    }
}