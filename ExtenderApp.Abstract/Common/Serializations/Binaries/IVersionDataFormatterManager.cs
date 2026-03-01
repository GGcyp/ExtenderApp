using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 支持按版本管理的二进制格式化器管理接口。 提供默认长度提示，并允许向管理器注册具体的格式化器实例。
    /// </summary>
    public interface IVersionDataFormatterManager : IBinaryFormatter
    {
        /// <summary>
        /// 向管理器注册一个格式化器实例。 具体支持的格式化器类型（如 <see cref="IBinaryFormatter"/>、 <see cref="IBinaryFormatter{T}"/> 或带版本的格式化器）由实现决定。
        /// </summary>
        /// <param name="formatter">要注册的格式化器实例。</param>
        void AddFormatter(object formatter);
    }

    /// <summary>
    /// 面向类型 <typeparamref name="T"/> 的按版本二进制序列化/反序列化接口。 在 <see cref="IBinaryFormatter{T}"/> 基础上，额外提供带 <see cref="Version"/> 的读写与长度估算方法。
    /// </summary>
    /// <typeparam name="T">目标序列化/反序列化的类型。</typeparam>
    public interface IVersionDataFormatterManager<T> : IBinaryFormatter<T>, IVersionDataFormatterManager
    {
        /// <summary>
        /// 将指定的 <paramref name="value"/> 按给定的 <paramref name="version"/> 序列化并写入到顺序缓冲写入适配器中。 实现应推进 <paramref name="writer"/> 的写入位置以反映已写入的字节数。
        /// </summary>
        /// <param name="writer">目标顺序缓冲写入适配器（按引用传递）。</param>
        /// <param name="value">要序列化的实例值。</param>
        /// <param name="version">用于序列化的协议/格式版本。</param>
        void Serialize(ref BinaryWriterAdapter writer, T value, Version version);

        /// <summary>
        /// 从顺序缓冲读取适配器中按给定的 <paramref name="version"/> 反序列化出一个 <typeparamref name="T"/> 实例。 实现应推进 <paramref name="reader"/> 的已消费位置以反映已读取的字节数。
        /// </summary>
        /// <param name="reader">来源顺序缓冲读取适配器（按引用传递）。</param>
        /// <param name="version">用于反序列化的协议/格式版本。</param>
        /// <returns>反序列化得到的 <typeparamref name="T"/> 实例。</returns>
        T Deserialize(ref BinaryReaderAdapter reader, Version version);

        /// <summary>
        /// 返回在指定 <paramref name="version"/> 下序列化 <paramref name="value"/> 预计需要的字节数，用于预留写缓冲。 可返回精确值或合理的估算值。
        /// </summary>
        /// <param name="value">待估算长度的值。</param>
        /// <param name="version">估算时采用的协议版本。</param>
        /// <returns>序列化所需的字节数。</returns>
        long GetLength(T value, Version version);
    }
}