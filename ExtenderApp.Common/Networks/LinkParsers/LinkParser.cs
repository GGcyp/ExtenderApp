using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 表示一个抽象类，用于解析链接数据。
    /// </summary>
    public abstract class LinkParser : DisposableObject
    {
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <typeparam name="T">要发送的数据类型</typeparam>
        /// <param name="linker">链接对象</param>
        /// <param name="value">要发送的数据</param>
        /// <exception cref="ObjectDisposedException">如果对象已被释放</exception>
        /// <exception cref="ArgumentNullException">如果<paramref name="linker"/>或<paramref name="value"/>为null</exception>
        /// <exception cref="InvalidOperationException">如果链接未连接</exception>
        public void Send<T>(ILinker linker, T value)
        {
            ThrowIfDisposed();
            if (linker == null)
                throw new ArgumentNullException(nameof(linker));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (!linker.Connected)
                throw new InvalidOperationException("链接未连接");
            Serialize(value, out var bytes, out var start, out var length);
            linker.Send(bytes, start, length);
        }

        /// <summary>
        /// 将给定类型的值序列化为字节数组。
        /// </summary>
        /// <typeparam name="T">要序列化的值的类型。</typeparam>
        /// <param name="value">要序列化的值。</param>
        /// <param name="bytes">输出参数，存储序列化后的字节数组。</param>
        /// <param name="start">输出参数，存储序列化后的字节数组中的起始索引。</param>
        /// <param name="length">输出参数，存储序列化后的字节数组中的长度。</param>
        public abstract void Serialize<T>(T value, out byte[] bytes, out int start, out int length);

        /// <summary>
        /// 从字节数组中反序列化给定类型的值。
        /// </summary>
        /// <typeparam name="T">要反序列化的值的类型。</typeparam>
        /// <param name="bytes">包含序列化数据的字节数组。</param>
        /// <returns>反序列化后的值，如果反序列化失败则返回null。</returns>
        public abstract T? Deserialize<T>(byte[] bytes);
    }
}
