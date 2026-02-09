using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.IO.FileParsers
{
    /// <summary>
    /// 序列化抽象基类，提供对象 ⇄ 二进制 内存级别的序列化/反序列化契约。
    /// <para>实现类应负责将对象序列化为字节或缓冲，并能从内存/序列中还原对象。</para>
    /// </summary>
    public abstract class Serialization : DisposableObject, ISerialization
    {
        #region Serialize

        ///<inheritdoc/>
        public abstract byte[] Serialize<T>(T value);

        ///<inheritdoc/>
        public abstract void Serialize<T>(T value, Span<byte> span);

        ///<inheritdoc/>
        public abstract void Serialize<T>(T value, out ByteBuffer buffer);

        #endregion Serialize

        #region Deserialize

        ///<inheritdoc/>
        public abstract T? Deserialize<T>(ReadOnlySpan<byte> span);

        ///<inheritdoc/>
        public abstract T? Deserialize<T>(ReadOnlyMemory<byte> memory);

        ///<inheritdoc/>
        public abstract T? Deserialize<T>(ReadOnlySequence<byte> memories);

        #endregion Deserialize
    }
}