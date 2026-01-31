using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 二进制格式化器基础接口，提供序列化时的默认长度提示。
    /// </summary>
    public interface IBinaryFormatter
    {
        /// <summary>
        /// 序列化的默认预估长度（字节数）。
        /// 用于预留写缓冲的大小提示，实际写入可能与该值不同。
        /// </summary>
        int DefaultLength { get; }

        /// <summary>
        /// 二进制格式化器的方法信息详情。
        /// </summary>
        BinaryFormatterMethodInfoDetails MethodInfoDetails { get; }
    }

    /// <summary>
    /// 针对类型 <typeparamref name="T"/> 的二进制序列化/反序列化器接口。
    /// </summary>
    /// <typeparam name="T">要序列化/反序列化的类型。</typeparam>
    public interface IBinaryFormatter<T> : IBinaryFormatter
    {
        /// <summary>
        /// 将 <paramref name="value"/> 按实现约定的二进制格式写入到 <see cref="ByteBuffer"/>。
        /// </summary>
        /// <param name="buffer">目标缓存。</param>
        /// <param name="value">要序列化的值。</param>
        void Serialize(ref ByteBuffer buffer, T value);

        /// <summary>
        /// 从给定的 <see cref="ByteBuffer"/> 读取并构造一个 <typeparamref name="T"/> 实例。
        /// </summary>
        /// <param name="buffer">数据缓存。</param>
        /// <returns>反序列化得到的 <typeparamref name="T"/> 实例。</returns>
        T Deserialize(ref ByteBuffer buffer);

        /// <summary>
        /// 返回序列化指定值预计需要的字节数，用于预留写缓冲。
        /// 具体实现可返回精确值或合理的估算值。
        /// </summary>
        /// <param name="value">待估算长度的值。</param>
        /// <returns>序列化所需的字节数。</returns>
        long GetLength(T value);
    }
}
