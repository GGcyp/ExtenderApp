using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 分割器解析器接口
    /// </summary>
    public interface ISplitterParser
    {
        #region Create

        /// <summary>
        /// 创建信息文件
        /// </summary>
        /// <param name="fileInfo">期望的本地文件信息</param>
        /// <param name="sInfo">分割器信息</param>
        void CreateInfoFile(ExpectLocalFileInfo fileInfo, SplitterInfo sInfo);

        /// <summary>
        /// 创建信息文件
        /// </summary>
        /// <param name="fileInfo">本地文件信息</param>
        /// <param name="sInfo">分割器信息</param>
        void CreateInfoFile(LocalFileInfo fileInfo, SplitterInfo sInfo);

        /// <summary>
        /// 创建信息文件
        /// </summary>
        /// <param name="operate">并发操作接口</param>
        /// <param name="sInfo">分割器信息</param>
        void CreateInfoFile(IConcurrentOperate operate, SplitterInfo sInfo);

        /// <summary>
        /// 为文件创建信息
        /// </summary>
        /// <param name="targetFileInfo">目标文件信息</param>
        /// <param name="createLoaderChunks">是否创建加载块，默认为true</param>
        /// <returns>分割器信息</returns>
        SplitterInfo CreateInfoForFile(LocalFileInfo targetFileInfo, bool createLoaderChunks = false);

        /// <summary>
        /// 为文件创建信息
        /// </summary>
        /// <param name="targetFileInfo">目标文件信息</param>
        /// <param name="chunkMaxLength">最大长度</param>
        /// <param name="createLoaderChunks">是否创建加载块，默认为true</param>
        /// <returns>分割器信息</returns>
        SplitterInfo CreateInfoForFile(LocalFileInfo targetFileInfo, int chunkMaxLength, bool createLoaderChunks = false);

        #endregion

        #region Read

        /// <summary>
        /// 读取信息
        /// </summary>
        /// <param name="fileInfo">期望的本地文件信息</param>
        /// <returns>分割器信息，如果不存在则返回null</returns>
        SplitterInfo? ReadInfo(ExpectLocalFileInfo fileInfo);

        /// <summary>
        /// 读取信息
        /// </summary>
        /// <param name="fileInfo">本地文件信息</param>
        /// <returns>分割器信息，如果不存在则返回null</returns>
        SplitterInfo? ReadInfo(LocalFileInfo fileInfo);

        /// <summary>
        /// 读取点数据
        /// </summary>
        /// <param name="fileOperate">文件操作接口</param>
        /// <param name="chunkIndex">块索引</param>
        /// <param name="sinfo">分割器信息</param>
        /// <returns>分割器数据传输对象</returns>
        SplitterDto ReadDot(IConcurrentOperate fileOperate, uint chunkIndex, SplitterInfo sinfo);

        /// <summary>
        /// 读取数据传输对象
        /// </summary>
        /// <param name="fileInfo">本地文件信息</param>
        /// <param name="chunkIndex">块索引</param>
        /// <param name="sinfo">分割器信息</param>
        /// <returns>分割器数据传输对象</returns>
        SplitterDto ReadDto(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo);

        /// <summary>
        /// 从指定文件中读取数据。
        /// </summary>
        /// <param name="fileInfo">文件信息。</param>
        /// <param name="chunkIndex">数据块索引。</param>
        /// <param name="sinfo">拆分器信息。</param>
        /// <param name="bytes">存储读取数据的字节数组。</param>
        void Read(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo, byte[] bytes);

        /// <summary>
        /// 从指定文件中读取数据并返回字节数组。
        /// </summary>
        /// <param name="fileInfo">文件信息。</param>
        /// <param name="chunkIndex">数据块索引。</param>
        /// <param name="sinfo">拆分器信息。</param>
        /// <returns>读取的数据，如果读取失败则返回null。</returns>
        byte[]? Read(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo);

        /// <summary>
        /// 从本地文件中读取指定块的数据。
        /// </summary>
        /// <typeparam name="T">要读取的数据类型。</typeparam>
        /// <param name="fileInfo">本地文件信息。</param>
        /// <param name="chunkIndex">要读取的块的索引。</param>
        /// <param name="sinfo">拆分器信息。</param>
        /// <returns>读取的数据。</returns>
        T Read<T>(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo);
        /// <summary>
        /// 读取指定索引块的数据，并返回指定类型的对象。
        /// </summary>
        /// <typeparam name="T">返回对象的类型。</typeparam>
        /// <param name="operate">并发操作接口。</param>
        /// <param name="chunkIndex">要读取的索引块。</param>
        /// <param name="sinfo">分割信息。</param>
        /// <returns>返回指定类型的对象。</returns>
        T Read<T>(IConcurrentOperate operate, uint chunkIndex, SplitterInfo sinfo);

        /// <summary>
        /// 读取指定索引块的数据到指定的字节数组中。
        /// </summary>
        /// <param name="operate">并发操作接口。</param>
        /// <param name="chunkIndex">要读取的索引块。</param>
        /// <param name="sinfo">分割信息。</param>
        /// <param name="bytes">存储读取数据的字节数组。</param>
        void Read(IConcurrentOperate operate, uint chunkIndex, SplitterInfo sinfo, byte[] bytes);

        /// <summary>
        /// 读取指定索引块的数据，并返回一个字节数组。
        /// </summary>
        /// <param name="operate">并发操作接口。</param>
        /// <param name="chunkIndex">要读取的索引块。</param>
        /// <param name="sinfo">分割信息。</param>
        /// <returns>包含读取数据的字节数组；如果没有数据可读，则返回null。</returns>
        byte[]? Read(IConcurrentOperate operate, uint chunkIndex, SplitterInfo sinfo);

        #endregion

        #region ReadAsync

        /// <summary>
        /// 异步从指定文件中读取数据，读取完成后调用回调方法。
        /// </summary>
        /// <param name="fileInfo">文件信息。</param>
        /// <param name="chunkIndex">数据块索引。</param>
        /// <param name="sinfo">拆分器信息。</param>
        /// <param name="callback">读取完成后的回调方法，参数为读取到的数据。</param>
        void ReadAsync(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo, Action<byte[]?> callback);

        /// <summary>
        /// 异步从指定文件中读取数据到指定的字节数组中，读取完成后调用回调方法。
        /// </summary>
        /// <param name="fileInfo">文件信息。</param>
        /// <param name="chunkIndex">数据块索引。</param>
        /// <param name="sinfo">拆分器信息。</param>
        /// <param name="bytes">存储读取数据的字节数组。</param>
        /// <param name="callback">读取完成后的回调方法，参数为读取是否成功。</param>
        void ReadAsync(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo, byte[] bytes, Action<bool> callback);

        /// <summary>
        /// 异步从指定文件中读取数据，读取完成后调用回调方法。
        /// </summary>
        /// <param name="fileInfo">文件信息。</param>
        /// <param name="chunkIndex">数据块索引。</param>
        /// <param name="sinfo">拆分器信息。</param>
        /// <param name="callback">读取完成后的回调方法，参数为读取到的数据。</param>
        /// <typeparam name="T">回调方法的参数类型。</typeparam>
        void ReadAsync<T>(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo, Action<T?> callback);

        /// <summary>
        /// 异步读取数据
        /// </summary>
        /// <param name="operate">并发操作接口</param>
        /// <param name="chunkIndex">数据块索引</param>
        /// <param name="sinfo">拆分信息</param>
        /// <param name="callback">回调函数，用于处理读取到的数据</param>
        void ReadAsync(IConcurrentOperate operate, uint chunkIndex, SplitterInfo sinfo, Action<byte[]?> callback);

        /// <summary>
        /// 异步读取数据到指定缓冲区
        /// </summary>
        /// <param name="operate">并发操作接口</param>
        /// <param name="chunkIndex">数据块索引</param>
        /// <param name="sinfo">拆分信息</param>
        /// <param name="bytes">存储读取数据的缓冲区</param>
        /// <param name="callback">回调函数，返回读取操作是否成功</param>
        void ReadAsync(IConcurrentOperate operate, uint chunkIndex, SplitterInfo sinfo, byte[] bytes, Action<bool> callback);

        /// <summary>
        /// 异步读取数据并返回泛型类型结果
        /// </summary>
        /// <typeparam name="T">返回结果的泛型类型</typeparam>
        /// <param name="operate">并发操作接口</param>
        /// <param name="chunkIndex">数据块索引</param>
        /// <param name="sinfo">拆分信息</param>
        /// <param name="callback">回调函数，用于处理读取到的数据并返回泛型类型结果</param>
        void ReadAsync<T>(IConcurrentOperate operate, uint chunkIndex, SplitterInfo sinfo, Action<T?> callback);

        #endregion

        #region Write

        /// <summary>
        /// 将指定类型的值写入到指定文件中。
        /// </summary>
        /// <typeparam name="T">要写入的值的数据类型。</typeparam>
        /// <param name="targetFileInfo">目标文件的信息。</param>
        /// <param name="sinfo">分片信息。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="chunkIndex">分片索引。</param>
        void Write<T>(LocalFileInfo targetFileInfo, SplitterInfo sinfo, T value, uint chunkIndex);

        /// <summary>
        /// 将字节数组写入到指定文件中。
        /// </summary>
        /// <param name="targetFileInfo">目标文件的信息。</param>
        /// <param name="sinfo">分片信息。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="chunkIndex">分片索引。</param>
        /// <param name="bytesLength">要写入的字节长度，默认为0表示写入整个字节数组。</param>
        void Write(LocalFileInfo targetFileInfo, SplitterInfo sinfo, byte[] bytes, uint chunkIndex, int bytesLength = 0);

        /// <summary>
        /// 将字节数组写入指定的操作器
        /// </summary>
        /// <param name="operate">操作器接口</param>
        /// <param name="sinfo">分片信息</param>
        /// <param name="bytes">要写入的字节数组</param>
        /// <param name="chunkIndex">分片索引</param>
        /// <param name="bytesLength">要写入的字节长度，默认为0</param>
        void Write(IConcurrentOperate operate, SplitterInfo sinfo, byte[] bytes, uint chunkIndex, int bytesLength = 0);

        /// <summary>
        /// 将指定类型的值写入指定的操作器
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="operate">操作器接口</param>
        /// <param name="sinfo">分片信息</param>
        /// <param name="value">要写入的值</param>
        /// <param name="chunkIndex">分片索引</param>
        void Write<T>(IConcurrentOperate operate, SplitterInfo sinfo, T value, uint chunkIndex);
        /// <summary>
        /// 将数据写入到指定的文件或并发操作中。
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="targetFileInfo">目标文件信息，表示要写入数据的文件</param>
        /// <param name="sinfo">分割器信息</param>
        /// <param name="dto">分割器数据传输对象</param>
        void Write<T>(LocalFileInfo targetFileInfo, SplitterInfo sinfo, SplitterDto dto);

        /// <summary>
        /// 将数据写入到指定的并发操作中。
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="operate">并发操作接口，表示要写入数据的并发操作</param>
        /// <param name="sinfo">分割器信息</param>
        /// <param name="dto">分割器数据传输对象</param>
        void Write<T>(IConcurrentOperate operate, SplitterInfo sinfo, SplitterDto dto);

        #endregion

        #region WriteAsync

        /// <summary>
        /// 异步写入数据到指定文件或并发操作对象。
        /// </summary>
        /// <param name="targetFileInfo">目标文件信息，用于指定要写入的文件。</param>
        /// <param name="sinfo">分片信息，包含分片大小、总大小等信息。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="chunkIndex">当前分片的索引。</param>
        /// <param name="bytesLength">要写入的字节长度，默认为0表示写入整个字节数组。</param>
        /// <param name="callback">写入完成后的回调函数，参数为写入的字节数组。</param>
        void WriteAsync(LocalFileInfo targetFileInfo, SplitterInfo sinfo, byte[] bytes, uint chunkIndex, int bytesLength = 0, Action<byte[]>? callback = null);

        /// <summary>
        /// 异步写入数据到并发操作对象。
        /// </summary>
        /// <param name="operate">并发操作对象，用于处理并发写入。</param>
        /// <param name="sinfo">分片信息，包含分片大小、总大小等信息。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="chunkIndex">当前分片的索引。</param>
        /// <param name="bytesLength">要写入的字节长度，默认为0表示写入整个字节数组。</param>
        /// <param name="callback">写入完成后的回调函数，参数为写入的字节数组。</param>
        void WriteAsync(IConcurrentOperate operate, SplitterInfo sinfo, byte[] bytes, uint chunkIndex, int bytesLength = 0, Action<byte[]>? callback = null);

        /// <summary>
        /// 异步写入泛型值到指定文件。
        /// </summary>
        /// <typeparam name="T">要写入的值的类型。</typeparam>
        /// <param name="targetFileInfo">目标文件信息，用于指定要写入的文件。</param>
        /// <param name="sinfo">分片信息，包含分片大小、总大小等信息。</param>
        /// <param name="value">要写入的泛型值。</param>
        /// <param name="chunkIndex">当前分片的索引。</param>
        /// <param name="callback">写入完成后的回调函数。</param>
        void WriteAsync<T>(LocalFileInfo targetFileInfo, SplitterInfo sinfo, T value, uint chunkIndex, Action? callback = null);

        /// <summary>
        /// 异步写入泛型值到并发操作对象。
        /// </summary>
        /// <typeparam name="T">要写入的值的类型。</typeparam>
        /// <param name="operate">并发操作对象，用于处理并发写入。</param>
        /// <param name="sinfo">分片信息，包含分片大小、总大小等信息。</param>
        /// <param name="value">要写入的泛型值。</param>
        /// <param name="chunkIndex">当前分片的索引。</param>
        /// <param name="callback">写入完成后的回调函数。</param>
        void WriteAsync<T>(IConcurrentOperate operate, SplitterInfo sinfo, T value, uint chunkIndex, Action? callback = null);

        /// <summary>
        /// 异步写入文件。
        /// </summary>
        /// <param name="targetFileInfo">目标文件信息。</param>
        /// <param name="sinfo">分割器信息。</param>
        /// <param name="dto">分割器数据传输对象。</param>
        /// <param name="callback">写入完成后的回调方法，可为空。</param>
        void WriteAsync(LocalFileInfo targetFileInfo, SplitterInfo sinfo, SplitterDto dto, Action? callback = null);

        /// <summary>
        /// 异步写入文件。
        /// </summary>
        /// <param name="operate">并发操作接口。</param>
        /// <param name="sinfo">分割器信息。</param>
        /// <param name="dto">分割器数据传输对象。</param>
        /// <param name="callback">写入完成后的回调方法，可为空。</param>
        void WriteAsync(IConcurrentOperate operate, SplitterInfo sinfo, SplitterDto dto, Action? callback = null);

        #endregion
    }
}
