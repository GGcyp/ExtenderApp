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
    public interface IBinaryFormatter<T> : IBinaryFormatter
    {
        /// <summary>
        /// 将 <paramref name="value"/> 按实现约定的二进制格式写入到 <paramref name="buffer"/> 中。
        /// </summary>
        /// <param name="buffer">目标缓存（写入时由调用方提供或预分配）。</param>
        /// <param name="value">要序列化的值。</param>
        void Serialize(AbstractBuffer<byte> buffer, T value);

        /// <summary>
        /// 从给定的 <paramref name="buffer"/> 中读取并构造一个 <typeparamref name="T"/> 实例。
        /// </summary>
        /// <param name="buffer">来源缓存（读取时推进其已消费位置）。</param>
        /// <returns>反序列化得到的 <typeparamref name="T"/> 实例。</returns>
        T Deserialize(AbstractBufferReader<byte> buffer);

        /// <summary>
        /// 返回序列化指定值预计需要的字节数，用于预留写缓冲。
        /// 具体实现可返回精确值或合理的估算值。
        /// </summary>
        /// <param name="value">待估算长度的值。</param>
        /// <returns>序列化所需的字节数。</returns>
        long GetLength(T value);
    }
}