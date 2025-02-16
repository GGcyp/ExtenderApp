using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件分割器接口
    /// 继承自文件解析器接口
    /// </summary>
    public interface ISplitterParser : IFileParser
    {
        /// <summary>
        /// 创建一个文件。
        /// </summary>
        /// <param name="fileInfo">期望的本地文件信息。</param>
        /// <param name="info">分割信息。</param>
        void Creat(ExpectLocalFileInfo fileInfo, SplitterInfo info);

        /// <summary>
        /// 写入字节数据到文件。
        /// </summary>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="bytes">要写入的字节数据。</param>
        /// <param name="chunkIndex">数据块的索引。</param>
        /// <param name="splitterInfo">分割信息（可选）。</param>
        /// <param name="fileOperate">文件操作对象（可选）。</param>
        void Write(ExpectLocalFileInfo info, byte[] bytes, uint chunkIndex, SplitterInfo? splitterInfo = null, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 写入泛型数据到文件。
        /// </summary>
        /// <typeparam name="T">泛型类型。</typeparam>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="chunkIndex">数据块的索引。</param>
        /// <param name="splitterInfo">分割信息（可选）。</param>
        /// <param name="fileOperate">文件操作对象（可选）。</param>
        void Write<T>(ExpectLocalFileInfo info, T value, uint chunkIndex, SplitterInfo? splitterInfo = null, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 异步写入字节数据到文件。
        /// </summary>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="bytes">要写入的字节数据。</param>
        /// <param name="chunkIndex">数据块的索引。</param>
        /// <param name="splitterInfo">分割信息（可选）。</param>
        /// <param name="fileOperate">文件操作对象（可选）。</param>
        void WriteAsync(ExpectLocalFileInfo info, byte[] bytes, uint chunkIndex, SplitterInfo? splitterInfo = null, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 异步写入泛型数据到文件。
        /// </summary>
        /// <typeparam name="T">泛型类型。</typeparam>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="chunkIndex">数据块的索引。</param>
        /// <param name="splitterInfo">分割信息（可选）。</param>
        /// <param name="fileOperate">文件操作对象（可选）。</param>
        void WriteAsync<T>(ExpectLocalFileInfo info, T value, uint chunkIndex, SplitterInfo? splitterInfo = null, IConcurrentOperate fileOperate = null);
    }
}
