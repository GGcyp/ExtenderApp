using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 表示一个抽象类，用于解析链接数据。
    /// </summary>
    public abstract class LinkParser : DisposableObject
    {
        private readonly SequencePool<byte> _sequencePool;

        public LinkParser(SequencePool<byte> sequencePool)
        {
            _sequencePool = sequencePool;
        }

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

        }

        /// <summary>
        /// 异步发送数据。
        /// </summary>
        /// <typeparam name="T">要发送的数据类型。</typeparam>
        /// <param name="linker">发送数据的链接。</param>
        /// <param name="value">要发送的数据。</param>
        /// <exception cref="ObjectDisposedException">如果当前实例已被释放。</exception>
        /// <exception cref="ArgumentNullException">如果 <paramref name="linker"/> 或 <paramref name="value"/> 为 null。</exception>
        /// <exception cref="InvalidOperationException">如果 <paramref name="linker"/> 未连接。</exception>
        public void SendAsync<T>(ILinker linker, T value)
        {

        }

        internal void Receive(byte[] bytes, int length)
        {
            var buffer = new ByteBuffer(new ReadOnlyMemory<byte>(bytes, 0, length));
            Receive(ref buffer);
        }

        /// <summary>
        /// 接收数据。
        /// </summary>
        /// <param name="buffer">用于读取数据的二进制读取器。</param>
        protected abstract void Receive(ref ByteBuffer buffer);

        /// <summary>
        /// 将指定类型的值序列化为二进制格式，并写入到指定的二进制写入器中。
        /// </summary>
        /// <typeparam name="T">要序列化的值的类型。</typeparam>
        /// <param name="buffer">二进制写入器，用于写入序列化后的数据。</param>
        /// <param name="value">要序列化的值。</param>
        public abstract void Serialize<T>(ref ByteBuffer buffer, T value);
    }
}
