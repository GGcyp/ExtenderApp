using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Compressions
{
    /// <summary>
    /// 压缩器抽象类，提供压缩和解压缩的基本接口。
    /// </summary>
    public abstract class Compression : DisposableObject, ICompression
    {
        #region TryCompress

        /// <inheritdoc/>
        public abstract bool TryCompress(ReadOnlySpan<byte> span, out AbstractBuffer<byte> output);

        /// <inheritdoc/>
        public abstract bool TryCompress(AbstractBuffer<byte> input, out AbstractBuffer<byte> output, CompressionType compression = CompressionType.Block);

        #endregion TryCompress

        #region TryDecompress

        /// <inheritdoc/>
        public abstract bool TryDecompress(ReadOnlySpan<byte> span, out AbstractBuffer<byte> output);

        /// <inheritdoc/>
        public abstract bool TryDecompress(AbstractBuffer<byte> input, out AbstractBuffer<byte> output);

        #endregion TryDecompress
    }
}