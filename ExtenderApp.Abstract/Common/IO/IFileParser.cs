using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件解析接口
    /// </summary>
    public interface IFileParser
    {
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="info">文件信息</param>
        void Delete(ExpectLocalFileInfo info);

        /// <summary>
        /// 获取操作对象
        /// </summary>
        /// <param name="info">文件操作信息</param>
        /// <returns>操作对象</returns>
        IConcurrentOperate? GetOperate(FileOperateInfo info);

        /// <summary>
        /// 同步读取文件内容
        /// </summary>
        /// <typeparam name="T">读取内容的类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="fileOperate">并发操作对象，默认为null</param>
        /// <returns>读取的内容</returns>
        T? Read<T>(ExpectLocalFileInfo info, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 同步读取文件内容
        /// </summary>
        /// <typeparam name="T">读取内容的类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="fileOperate">并发操作对象，默认为null</param>
        /// <returns>读取的内容</returns>
        T? Read<T>(FileOperateInfo info, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 异步读取文件内容
        /// </summary>
        /// <typeparam name="T">读取内容的类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="callback">读取完成后的回调函数</param>
        /// <param name="fileOperate">并发操作对象，默认为null</param>
        void ReadAsync<T>(ExpectLocalFileInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 异步读取文件内容
        /// </summary>
        /// <typeparam name="T">读取内容的类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="callback">读取完成后的回调函数</param>
        /// <param name="fileOperate">并发操作对象，默认为null</param>
        void ReadAsync<T>(FileOperateInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 写入文件内容
        /// </summary>
        /// <typeparam name="T">写入内容的类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="value">写入的内容</param>
        /// <param name="fileOperate">并发操作对象，默认为null</param>
        void Write<T>(ExpectLocalFileInfo info, T value, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 写入文件内容
        /// </summary>
        /// <typeparam name="T">写入内容的类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="value">写入的内容</param>
        /// <param name="fileOperate">并发操作对象，默认为null</param>
        void Write<T>(FileOperateInfo info, T value, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 异步写入文件内容
        /// </summary>
        /// <typeparam name="T">写入内容的类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="value">写入的内容</param>
        /// <param name="callback">写入完成后的回调函数，默认为null</param>
        /// <param name="fileOperate">并发操作对象，默认为null</param>
        void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 异步写入文件内容
        /// </summary>
        /// <typeparam name="T">写入内容的类型</typeparam>
        /// <param name="operate">文件操作信息</param>
        /// <param name="value">写入的内容</param>
        /// <param name="callback">写入完成后的回调函数，默认为null</param>
        /// <param name="fileOperate">并发操作对象，默认为null</param>
        void WriteAsync<T>(FileOperateInfo operate, T value, Action? callback = null, IConcurrentOperate fileOperate = null);
    }

    /// <summary>
    /// 文件解析器接口
    /// </summary>
    /// <typeparam name="TOption">文件解析选项类型</typeparam>
    public interface IFileParser<TOption> : IFileParser
    {
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="info">要删除的文件信息</param>
        void Delete(ExpectLocalFileInfo info);

        /// <summary>
        /// 获取文件操作实例
        /// </summary>
        /// <param name="info">文件操作信息</param>
        /// <returns>文件操作实例</returns>
        IConcurrentOperate? GetOperate(FileOperateInfo info);

        /// <summary>
        /// 同步读取文件内容
        /// </summary>
        /// <typeparam name="T">文件内容类型</typeparam>
        /// <param name="info">本地文件信息</param>
        /// <param name="options">文件解析选项</param>
        /// <param name="fileOperate">并发操作实例</param>
        /// <returns>文件内容</returns>
        T? Read<T>(ExpectLocalFileInfo info, TOption options, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 同步读取文件内容
        /// </summary>
        /// <typeparam name="T">文件内容类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="options">文件解析选项</param>
        /// <param name="fileOperate">并发操作实例</param>
        /// <returns>文件内容</returns>
        T? Read<T>(FileOperateInfo info, TOption options, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 异步读取文件内容
        /// </summary>
        /// <typeparam name="T">文件内容类型</typeparam>
        /// <param name="info">本地文件信息</param>
        /// <param name="callback">读取完成后的回调函数</param>
        /// <param name="options">文件解析选项</param>
        /// <param name="fileOperate">并发操作实例</param>
        void ReadAsync<T>(ExpectLocalFileInfo info, TOption options, Action<T>? callback = null, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 异步读取文件内容
        /// </summary>
        /// <typeparam name="T">文件内容类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="callback">读取完成后的回调函数</param>
        /// <param name="options">文件解析选项</param>
        /// <param name="fileOperate">并发操作实例</param>
        void ReadAsync<T>(FileOperateInfo info, TOption options, Action<T>? callback = null, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 同步写入文件内容
        /// </summary>
        /// <typeparam name="T">文件内容类型</typeparam>
        /// <param name="info">本地文件信息</param>
        /// <param name="value">要写入的内容</param>
        /// <param name="options">文件解析选项</param>
        /// <param name="fileOperate">并发操作实例</param>
        void Write<T>(ExpectLocalFileInfo info, T value, TOption options, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 同步写入文件内容
        /// </summary>
        /// <typeparam name="T">文件内容类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="value">要写入的内容</param>
        /// <param name="options">文件解析选项</param>
        /// <param name="fileOperate">并发操作实例</param>
        void Write<T>(FileOperateInfo info, T value, TOption options, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 异步写入文件内容
        /// </summary>
        /// <typeparam name="T">文件内容类型</typeparam>
        /// <param name="info">本地文件信息</param>
        /// <param name="value">要写入的内容</param>
        /// <param name="options">文件解析选项</param>
        /// <param name="callback">写入完成后的回调函数</param>
        /// <param name="fileOperate">并发操作实例</param>
        void WriteAsync<T>(ExpectLocalFileInfo info, T value, TOption options, Action? callback = null, IConcurrentOperate fileOperate = null);

        /// <summary>
        /// 异步写入文件内容
        /// </summary>
        /// <typeparam name="T">文件内容类型</typeparam>
        /// <param name="operate">文件操作信息</param>
        /// <param name="value">要写入的内容</param>
        /// <param name="options">文件解析选项</param>
        /// <param name="callback">写入完成后的回调函数</param>
        /// <param name="fileOperate">并发操作实例</param>
        void WriteAsync<T>(FileOperateInfo operate, T value, TOption options, Action? callback = null, IConcurrentOperate fileOperate = null);
    }
}
