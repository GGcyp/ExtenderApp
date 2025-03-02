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
        /// 创建一个分片文件。
        /// </summary>
        /// <param name="fileInfo">期望的本地文件信息。</param>
        /// <param name="info">分割信息。</param>
        void Create(ExpectLocalFileInfo fileInfo, SplitterInfo info);

        /// <summary>
        /// 根据给定的本地文件信息创建 SplitterInfo 实例
        /// </summary>
        /// <param name="info">本地文件信息</param>
        /// <param name="maxLength">每个分片的最大长度</param>
        /// <param name="createLoaderChunks">是否创建加载器分片，默认为 true</param>
        /// <returns>创建的 SplitterInfo 实例</returns>
        SplitterInfo Create(LocalFileInfo info, int maxLength, bool createLoaderChunks = true);

        /// <summary>
        /// 从文件中读取数据块。
        /// </summary>
        /// <param name="info">文件操作信息对象。</param>
        /// <param name="chunkIndex">要读取的数据块的索引。</param>
        /// <param name="splitterInfo">分割器信息对象。</param>
        /// <param name="fileOperate">并发操作接口，可选。</param>
        /// <param name="bytes">可选的字节数组，如果提供则优先使用。</param>
        /// <returns>读取到的字节数组。</returns>
        byte[] Read(FileOperateInfo info, uint chunkIndex, SplitterInfo splitterInfo, IConcurrentOperate? fileOperate = null, byte[]? bytes = null);

        /// <summary>
        /// 从本地文件中读取数据块。
        /// </summary>
        /// <param name="info">期望的本地文件信息对象。</param>
        /// <param name="chunkIndex">要读取的数据块的索引。</param>
        /// <param name="splitterInfo">分割器信息对象。</param>
        /// <param name="fileOperate">并发操作接口，可选。</param>
        /// <param name="bytes">可选的字节数组，如果提供则优先使用。</param>
        /// <returns>读取到的字节数组。</returns>
        byte[] Read(ExpectLocalFileInfo info, uint chunkIndex, SplitterInfo splitterInfo, IConcurrentOperate? fileOperate = null, byte[]? bytes = null);

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
