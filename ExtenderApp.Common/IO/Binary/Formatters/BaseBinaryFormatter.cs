using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 针对类型 <typeparamref name="T"/> 的二进制序列化/反序列化抽象基类。
    /// 提供默认长度提示属性，具体编码细节由派生类实现。
    /// 继承自 <see cref="DisposableObject"/>，可用于管理需要释放的资源。
    /// </summary>
    public abstract class BaseBinaryFormatter<T> : IBinaryFormatter<T>
    {
        /// <summary>
        /// 序列化的默认预估长度（字节数）。
        /// 用于预留写缓冲的大小提示，实际写入可能与该值不同（为 0 表示未知或无需特殊预估）。
        /// </summary>
        public abstract int DefaultLength { get; }

        /// <summary>
        /// 从给定的 <see cref="ByteBuffer"/> 读取并构造一个 <typeparamref name="T"/> 实例。
        /// 实现应在成功读取后推进 <paramref name="buffer"/> 的读取位置。
        /// </summary>
        /// <param name="buffer">数据缓存。</param>
        /// <returns>反序列化得到的对象。</returns>
        public abstract T Deserialize(ref ByteBuffer buffer);

        /// <summary>
        /// 将 <paramref name="value"/> 按实现约定的二进制格式写入到 <see cref="ByteBuffer"/>。
        /// 实现应在写入后推进 <paramref name="buffer"/> 的写入位置。
        /// </summary>
        /// <param name="buffer">目标缓存。</param>
        /// <param name="value">要序列化的值。</param>
        public abstract void Serialize(ref ByteBuffer buffer, T value);

        /// <summary>
        /// 返回序列化指定值预计需要的字节数，用于预留写缓冲。
        /// 可返回精确值或合理的估算值。
        /// </summary>
        /// <param name="value">待估算长度的值。</param>
        /// <returns>序列化所需的字节数。</returns>
        public abstract long GetLength(T value);
    }
}
