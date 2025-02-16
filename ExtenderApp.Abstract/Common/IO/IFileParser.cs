using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件解析接口
    /// </summary>
    public interface IFileParser
    {
        /// <summary>
        /// 同步读取文件内容，返回指定类型的数据
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        /// <returns>读取到的数据，如果读取失败则返回null</returns>
        T? Read<T>(ExpectLocalFileInfo info, IConcurrentOperate fileOperate = null, object? options = null);

        /// <summary>
        /// 同步读取文件内容，返回指定类型的数据
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        /// <returns>读取到的数据，如果读取失败则返回null</returns>
        T? Read<T>(FileOperateInfo info, IConcurrentOperate fileOperate = null, object? options = null);

        /// <summary>
        /// 异步读取文件内容，读取完成后调用回调函数
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="callback">回调函数，参数为读取到的数据</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        void ReadAsync<T>(ExpectLocalFileInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null, object? options = null);

        /// <summary>
        /// 异步读取文件内容，读取完成后调用回调函数
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="callback">回调函数，参数为读取到的数据</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        void ReadAsync<T>(FileOperateInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null, object? options = null);

        /// <summary>
        /// 同步写入数据到文件
        /// </summary>
        /// <typeparam name="T">要写入的数据类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        void Write<T>(ExpectLocalFileInfo info, T value, IConcurrentOperate fileOperate = null, object? options = null);

        /// <summary>
        /// 同步写入数据到文件
        /// </summary>
        /// <typeparam name="T">要写入的数据类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        void Write<T>(FileOperateInfo info, T value, IConcurrentOperate fileOperate = null, object? options = null);

        /// <summary>
        /// 异步写入数据到文件，写入完成后调用回调函数
        /// </summary>
        /// <typeparam name="T">要写入的数据类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="callback">回调函数，无参数</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null, object? options = null);

        /// <summary>
        /// 异步写入数据到文件，写入完成后调用回调函数
        /// </summary>
        /// <typeparam name="T">要写入的数据类型</typeparam>
        /// <param name="operate">文件操作信息</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="callback">回调函数，无参数</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        void WriteAsync<T>(FileOperateInfo operate, T value, Action? callback = null, IConcurrentOperate fileOperate = null, object? options = null);
    }
}
