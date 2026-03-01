using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 为 <see cref="IFileOperate"/> 提供的扩展方法，简化常见的同步/异步读写操作（针对 <see cref="ByteBuffer"/> 与 <see cref="ByteBlock"/>）。
    /// </summary>
    public static class FileOperateExtensions
    {
        #region Write

        /// <summary>
        /// 将 <see cref="BinaryReaderAdapter"/> 中的数据写入到指定的 <see cref="IFileOperate"/> 实例中（同步）。
        /// </summary>
        /// <param name="operate">目标文件操作实例，不能为 <c>null</c>。</param>
        /// <param name="reader">提供要写入数据的读取器。</param>
        /// <param name="filePosition">写入文件的起始偏移（字节）。默认值为 0。</param>
        /// <returns>返回写入结果，包含实际写入的字节数或错误信息的 <see cref="Result{T}"/>。</returns>
        public static Result<long> Write(this IFileOperate operate, BinaryReaderAdapter reader, long filePosition = 0)
        {
            return operate.Write(ref reader, filePosition);
        }
        #endregion

        #region Read

        /// <summary>
        /// 从指定的 <see cref="IFileOperate"/> 中读取 <paramref name="length"/> 字节的数据并写入到 <see cref="BinaryWriterAdapter"/> 中（同步）。
        /// </summary>
        /// <param name="operate">源文件操作实例，不能为 <c>null</c>。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="writer">用于接收读取数据的写入器。</param>
        /// <param name="filePosition">从文件的起始偏移（字节）处开始读取。默认值为 0。</param>
        /// <returns>返回读取结果，包含实际读取的字节数或错误信息的 <see cref="Result{T}"/>。</returns>
        public static Result<long> Read(this IFileOperate operate, long length, BinaryWriterAdapter writer, long filePosition = 0)
        {
            return operate.Read(length, ref writer, filePosition);
        }

        #endregion
    }
}