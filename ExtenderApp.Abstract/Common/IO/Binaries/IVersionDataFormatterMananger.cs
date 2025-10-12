using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 支持按版本管理的二进制格式化器管理接口。
    /// 提供默认长度提示，并允许向管理器注册具体的格式化器实例。
    /// </summary>
    public interface IVersionDataFormatterMananger : IBinaryFormatter
    {
        /// <summary>
        /// 向管理器注册一个格式化器实例。
        /// 具体支持的格式化器类型（如 <see cref="IBinaryFormatter"/>、<see cref="IBinaryFormatter{T}"/> 或带版本的格式化器）由实现决定。
        /// </summary>
        /// <param name="formatter">要注册的格式化器实例。</param>
        void AddFormatter(object formatter);
    }

    /// <summary>
    /// 面向类型 <typeparamref name="T"/> 的按版本二进制序列化/反序列化接口。
    /// 在 <see cref="IBinaryFormatter{T}"/> 基础上，额外提供带 <see cref="Version"/> 的读写与长度估算方法。
    /// </summary>
    /// <typeparam name="T">目标序列化/反序列化的类型。</typeparam>
    public interface IVersionDataFormatterMananger<T> : IBinaryFormatter<T>, IVersionDataFormatterMananger
    {
        /// <summary>
        /// 将 <paramref name="value"/> 依据指定 <paramref name="version"/> 的协议写入到 <see cref="ByteBlock"/>。
        /// 实现应在写入后推进 <paramref name="block"/> 的写入位置。
        /// </summary>
        /// <param name="block">目标写入器。</param>
        /// <param name="value">要序列化的值。</param>
        /// <param name="version">序列化所采用的协议版本。</param>
        void Serialize(ref ByteBlock block, T value, Version version);

        /// <summary>
        /// 按指定 <paramref name="version"/> 的协议从 <see cref="ByteBlock"/> 读取并构造一个 <typeparamref name="T"/> 实例。
        /// 实现应在读取后推进 <paramref name="block"/> 的读取位置。
        /// </summary>
        /// <param name="block">数据来源。</param>
        /// <param name="version">反序列化所采用的协议版本。</param>
        /// <returns>反序列化得到的对象。</returns>
        T Deserialize(ref ByteBlock block, Version version);

        /// <summary>
        /// 返回在指定 <paramref name="version"/> 下序列化 <paramref name="value"/> 预计需要的字节数，用于预留写缓冲。
        /// 可返回精确值或合理的估算值。
        /// </summary>
        /// <param name="value">待估算长度的值。</param>
        /// <param name="version">估算时采用的协议版本。</param>
        /// <returns>序列化所需的字节数。</returns>
        long GetLength(T value, Version version);
    }
}
