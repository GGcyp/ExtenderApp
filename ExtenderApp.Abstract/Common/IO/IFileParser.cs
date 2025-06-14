using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件解析接口
    /// </summary>
    public interface IFileParser
    {
        #region Read    

        /// <summary>
        /// 读取文件内容并转换为指定类型的对象
        /// </summary>
        /// <typeparam name="T">返回对象的类型</typeparam>
        /// <param name="info">ExpectLocalFileInfo类型的文件信息对象</param>
        /// <returns>转换后的对象，若读取失败或文件为空则返回null</returns>
        T? Read<T>(ExpectLocalFileInfo info);

        /// <summary>
        /// 读取文件内容并转换为指定类型的对象
        /// </summary>
        /// <typeparam name="T">返回对象的类型</typeparam>
        /// <param name="info">FileOperateInfo类型的文件信息对象</param>
        /// <returns>转换后的对象，若读取失败或文件为空则返回null</returns>
        T? Read<T>(FileOperateInfo info);

        /// <summary>
        /// 读取文件内容并转换为指定类型的对象
        /// </summary>
        /// <typeparam name="T">返回对象的类型</typeparam>
        /// <param name="fileOperate">IConcurrentOperate类型的文件操作接口对象</param>
        /// <returns>转换后的对象，若读取失败或文件为空则返回null</returns>
        T? Read<T>(IConcurrentOperate fileOperate);

        /// <summary>
        /// 从指定位置读取指定长度的数据，并将其转换为指定类型T。
        /// </summary>
        /// <typeparam name="T">需要转换成的数据类型。</typeparam>
        /// <param name="info">包含文件信息的对象。</param>
        /// <param name="position">开始读取的位置。</param>
        /// <param name="length">需要读取的长度。</param>
        /// <returns>转换后的T类型对象，如果读取失败或数据不足以完成转换则返回null。</returns>
        T? Read<T>(ExpectLocalFileInfo info, long position, long length);

        /// <summary>
        /// 从指定位置读取指定长度的数据，并将其转换为指定类型T。
        /// </summary>
        /// <typeparam name="T">需要转换成的数据类型。</typeparam>
        /// <param name="info">包含文件操作信息的对象。</param>
        /// <param name="position">开始读取的位置。</param>
        /// <param name="length">需要读取的长度。</param>
        /// <returns>转换后的T类型对象，如果读取失败或数据不足以完成转换则返回null。</returns>
        T? Read<T>(FileOperateInfo info, long position, long length);

        /// <summary>
        /// 从指定位置读取指定长度的数据，并将其转换为指定类型T。
        /// </summary>
        /// <typeparam name="T">需要转换成的数据类型。</typeparam>
        /// <param name="fileOperate">并发文件操作接口。</param>
        /// <param name="position">开始读取的位置。</param>
        /// <param name="length">需要读取的长度。</param>
        /// <returns>转换后的T类型对象，如果读取失败或数据不足以完成转换则返回null。</returns>
        T? Read<T>(IConcurrentOperate fileOperate, long position, long length);

        #endregion

        #region ReadAsync

        /// <summary>
        /// 异步读取文件内容。
        /// </summary>
        /// <typeparam name="T">回调参数的类型。</typeparam>
        /// <param name="info">文件信息。</param>
        /// <param name="callback">读取完成后的回调函数，参数为读取结果。</param>
        /// <remarks>
        /// 提供了多种重载方式，以支持不同类型的文件信息和读取参数。
        /// </remarks>
        void ReadAsync<T>(ExpectLocalFileInfo info, Action<T?> callback);

        /// <summary>
        /// 异步读取文件内容。
        /// </summary>
        /// <typeparam name="T">回调参数的类型。</typeparam>
        /// <param name="info">文件操作信息。</param>
        /// <param name="callback">读取完成后的回调函数，参数为读取结果。</param>
        void ReadAsync<T>(FileOperateInfo info, Action<T?> callback);

        /// <summary>
        /// 异步读取文件内容。
        /// </summary>
        /// <typeparam name="T">回调参数的类型。</typeparam>
        /// <param name="fileOperate">并发文件操作接口。</param>
        /// <param name="callback">读取完成后的回调函数，参数为读取结果。</param>
        void ReadAsync<T>(IConcurrentOperate fileOperate, Action<T?> callback);

        /// <summary>
        /// 异步读取文件内容，支持指定起始位置和长度。
        /// </summary>
        /// <typeparam name="T">回调参数的类型。</typeparam>
        /// <param name="info">文件信息。</param>
        /// <param name="position">读取起始位置。</param>
        /// <param name="length">读取长度。</param>
        /// <param name="callback">读取完成后的回调函数，参数为读取结果。</param>
        void ReadAsync<T>(ExpectLocalFileInfo info, long position, long length, Action<T?> callback);

        /// <summary>
        /// 异步读取文件内容，支持指定起始位置和长度。
        /// </summary>
        /// <typeparam name="T">回调参数的类型。</typeparam>
        /// <param name="info">文件操作信息。</param>
        /// <param name="position">读取起始位置。</param>
        /// <param name="length">读取长度。</param>
        /// <param name="callback">读取完成后的回调函数，参数为读取结果。</param>
        void ReadAsync<T>(FileOperateInfo info, long position, long length, Action<T?> callback);

        /// <summary>
        /// 异步读取文件内容，支持指定起始位置和长度。
        /// </summary>
        /// <typeparam name="T">回调参数的类型。</typeparam>
        /// <param name="fileOperate">并发文件操作接口。</param>
        /// <param name="position">读取起始位置。</param>
        /// <param name="length">读取长度。</param>
        /// <param name="callback">读取完成后的回调函数，参数为读取结果。</param>
        void ReadAsync<T>(IConcurrentOperate fileOperate, long position, long length, Action<T?> callback);

        #endregion

        #region Write    

        /// <summary>
        /// 将指定的值写入文件。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="info">文件信息对象，用于指定要写入的文件</param>
        /// <param name="value">要写入的值</param>
        void Write<T>(ExpectLocalFileInfo info, T value);

        /// <summary>
        /// 将指定的值写入文件。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="info">文件操作信息对象，用于指定要写入的文件</param>
        /// <param name="value">要写入的值</param>
        void Write<T>(FileOperateInfo info, T value);

        /// <summary>
        /// 将指定的值写入文件。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="fileOperate">并发文件操作接口，用于指定要写入的文件</param>
        /// <param name="value">要写入的值</param>
        void Write<T>(IConcurrentOperate fileOperate, T value);

        /// <summary>
        /// 将指定的值写入文件的指定位置。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="info">文件信息对象，用于指定要写入的文件</param>
        /// <param name="value">要写入的值</param>
        /// <param name="position">要写入的起始位置</param>
        void Write<T>(ExpectLocalFileInfo info, T value, long position);

        /// <summary>
        /// 将指定的值写入文件的指定位置。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="info">文件操作信息对象，用于指定要写入的文件</param>
        /// <param name="value">要写入的值</param>
        /// <param name="position">要写入的起始位置</param>
        void Write<T>(FileOperateInfo info, T value, long position);

        /// <summary>
        /// 将指定的值写入文件的指定位置。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="fileOperate">并发文件操作接口，用于指定要写入的文件</param>
        /// <param name="value">要写入的值</param>
        /// <param name="position">要写入的起始位置</param>
        void Write<T>(IConcurrentOperate fileOperate, T value, long position);

        #endregion

        #region WriteAsync   

        /// <summary>
        /// 异步写入数据到文件。
        /// </summary>
        /// <typeparam name="T">要写入的数据类型。</typeparam>
        /// <param name="info">文件信息对象。</param>
        /// <param name="value">要写入的数据。</param>
        /// <param name="callback">写入完成后的回调函数，可以为null。</param>
        void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action? callback = null);

        /// <summary>
        /// 异步写入数据到文件。
        /// </summary>
        /// <typeparam name="T">要写入的数据类型。</typeparam>
        /// <param name="info">文件操作信息对象。</param>
        /// <param name="value">要写入的数据。</param>
        /// <param name="callback">写入完成后的回调函数，可以为null。</param>
        void WriteAsync<T>(FileOperateInfo info, T value, Action? callback = null);

        /// <summary>
        /// 异步写入数据到文件。
        /// </summary>
        /// <typeparam name="T">要写入的数据类型。</typeparam>
        /// <param name="fileOperate">并发操作接口。</param>
        /// <param name="value">要写入的数据。</param>
        /// <param name="callback">写入完成后的回调函数，可以为null。</param>
        void WriteAsync<T>(IConcurrentOperate fileOperate, T value, Action? callback = null);

        /// <summary>
        /// 异步写入数据到文件的指定位置。
        /// </summary>
        /// <typeparam name="T">要写入的数据类型。</typeparam>
        /// <param name="info">文件信息对象。</param>
        /// <param name="value">要写入的数据。</param>
        /// <param name="position">写入的位置。</param>
        /// <param name="callback">写入完成后的回调函数，可以为null。</param>
        void WriteAsync<T>(ExpectLocalFileInfo info, T value, long position, Action? callback = null);

        /// <summary>
        /// 异步写入数据到文件的指定位置。
        /// </summary>
        /// <typeparam name="T">要写入的数据类型。</typeparam>
        /// <param name="info">文件操作信息对象。</param>
        /// <param name="value">要写入的数据。</param>
        /// <param name="position">写入的位置。</param>
        /// <param name="callback">写入完成后的回调函数，可以为null。</param>
        void WriteAsync<T>(FileOperateInfo info, T value, long position, Action? callback = null);

        /// <summary>
        /// 异步写入数据到文件的指定位置。
        /// </summary>
        /// <typeparam name="T">要写入的数据类型。</typeparam>
        /// <param name="fileOperate">并发操作接口。</param>
        /// <param name="value">要写入的数据。</param>
        /// <param name="position">写入的位置。</param>
        /// <param name="callback">写入完成后的回调函数，可以为null。</param>
        void WriteAsync<T>(IConcurrentOperate fileOperate, T value, long position, Action? callback = null);

        #endregion
    }
}
