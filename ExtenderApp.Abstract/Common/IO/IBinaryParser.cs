
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 二进制文件解析器接口，继承自文件解析器接口。
    /// </summary>
    /// <remarks>
    /// 该接口定义了用于处理二进制文件解析的方法。
    /// </remarks>
    public interface IBinaryParser : IFileParser
    {
        #region Deserialize

        /// <summary>
        /// 将字节数组反序列化为指定类型的对象。
        /// </summary>
        /// <typeparam name="T">要反序列化的目标类型。</typeparam>
        /// <param name="bytes">包含二进制数据的字节数组。</param>
        /// <returns>反序列化后的对象，如果无法反序列化则返回null。</returns>
        T? Deserialize<T>(byte[] bytes);

        /// <summary>
        /// 从流中反序列化对象。
        /// </summary>
        /// <typeparam name="T">要反序列化的目标类型。</typeparam>
        /// <param name="stream">包含要反序列化数据的流。</param>
        /// <returns>反序列化后的对象，如果流为空或格式不正确则返回null。</returns>
        T? Deserialize<T>(Stream stream);

        #endregion

        #region Serialize

        /// <summary>
        /// 将指定类型的对象序列化为字节数组。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <returns>包含序列化后数据的字节数组。</returns>
        byte[] Serialize<T>(T value);

        /// <summary>
        /// 将指定对象序列化为字节数组。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="bytes">用于存储序列化结果的字节数组。</param>
        void Serialize<T>(T value, byte[] bytes);

        /// <summary>
        /// 将指定类型的值序列化到给定的<see cref="ExtenderBinaryWriter"/>对象中。
        /// </summary>
        /// <typeparam name="T">要序列化的值的类型。</typeparam>
        /// <param name="writer"><see cref="ExtenderBinaryWriter"/>对象，用于写入序列化后的数据。</param>
        /// <param name="value">要序列化的值。</param>
        void Serialize<T>(ref ExtenderBinaryWriter writer, T value);

        /// <summary>
        /// 将对象序列化为二进制数据并写入到流中。
        /// </summary>
        /// <typeparam name="T">要序列化的对象的类型。</typeparam>
        /// <param name="stream">要写入数据的流。</param>
        /// <param name="value">要序列化的对象。</param>
        /// <returns>如果序列化成功，则返回true；否则返回false。</returns>
        void Serialize<T>(Stream stream, T value);

        /// <summary>
        /// 将对象序列化为字节数组。
        /// </summary>
        /// <typeparam name="T">要序列化的对象的类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="bytes">存储序列化结果的字节数组。</param>
        /// <param name="length">序列化后的字节长度。</param>
        void Serialize<T>(T value, byte[] bytes, out long length);

        /// <summary>
        /// 将对象序列化为字节数组。
        /// </summary>
        /// <typeparam name="T">要序列化的对象的类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="bytes">存储序列化结果的字节数组。</param>
        /// <param name="length">序列化后的字节长度。</param>
        void Serialize<T>(T value, byte[] bytes, out int length);

        /// <summary>
        /// 将指定值序列化为字节数组，并返回字节数组和数据长度。
        /// 字节数组从默认ArrayPool<byte>中获取，需要注意回收
        /// </summary>
        /// <typeparam name="T">要序列化的对象的类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="length">输出参数，表示序列化后的字节数组的长度。</param>
        /// <returns>包含序列化后数据的字节数组。</returns>
        byte[] SerializeForArrayPool<T>(T value, out long length);

        /// <summary>
        /// 将指定值序列化为字节数组，并返回字节数组和数据长度。
        /// 字节数组从默认ArrayPool<byte>中获取，需要注意回收
        /// </summary>
        /// <typeparam name="T">要序列化的对象的类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="length">输出参数，表示序列化后的字节数组的长度。</param>
        /// <returns>包含序列化后数据的字节数组。</returns>
        byte[] SerializeForArrayPool<T>(T value, out int length);

        #endregion

        #region Read

        /// <summary>
        /// 从指定位置读取指定长度的字节数组。
        /// </summary>
        /// <param name="info">包含文件信息的对象，可以为ExpectLocalFileInfo、LocalFileInfo、FileOperateInfo或IConcurrentOperate类型。</param>
        /// <param name="position">从文件的哪个位置开始读取数据，以字节为单位。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>包含读取到的字节数据的数组，如果读取失败或文件内容为空，则返回null。</returns>
        byte[]? Read(ExpectLocalFileInfo info, long position, long length);

        /// <summary>
        /// 从指定位置读取指定长度的数据。
        /// </summary>
        /// <param name="info">本地文件信息对象。</param>
        /// <param name="position">开始读取的位置。</param>
        /// <param name="length">要读取的长度。</param>
        /// <returns>读取的数据，如果读取失败则返回null。</returns>
        byte[]? Read(LocalFileInfo info, long position, long length);

        /// <summary>
        /// 从指定位置读取指定长度的字节数组。
        /// </summary>
        /// <param name="info">包含文件操作信息的FileOperateInfo对象。</param>
        /// <param name="position">从文件的哪个位置开始读取数据，以字节为单位。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>包含读取到的字节数据的数组，如果读取失败或文件内容为空，则返回null。</returns>
        byte[]? Read(FileOperateInfo info, long position, long length);

        /// <summary>
        /// 从指定位置读取指定长度的字节数组。
        /// </summary>
        /// <param name="fileOperate">实现IConcurrentOperate接口的并发操作对象。</param>
        /// <param name="position">从文件的哪个位置开始读取数据，以字节为单位。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>包含读取到的字节数据的数组，如果读取失败或文件内容为空，则返回null。</returns>
        byte[]? Read(IConcurrentOperate? fileOperate, long position, long length);

        /// <summary>
        /// 从指定位置开始读取指定长度的文件内容到指定字节数组中
        /// </summary>
        /// <param name="info">文件信息对象，支持多种类型</param>
        /// <param name="position">读取起始位置</param>
        /// <param name="length">读取长度</param>
        /// <param name="bytes">存储读取内容的字节数组</param>
        /// <returns>若读取成功返回true，否则返回false</returns>
        bool Read(ExpectLocalFileInfo info, long position, long length, byte[] bytes);

        /// <summary>
        /// 从指定位置开始读取指定长度的文件内容到指定字节数组中
        /// </summary>
        /// <param name="info">LocalFileInfo类型的文件信息对象</param>
        /// <param name="position">读取起始位置</param>
        /// <param name="length">读取长度</param>
        /// <param name="bytes">存储读取内容的字节数组</param>
        /// <returns>若读取成功返回true，否则返回false</returns>
        bool Read(LocalFileInfo info, long position, long length, byte[] bytes);

        /// <summary>
        /// 从指定位置开始读取指定长度的文件内容到指定字节数组中
        /// </summary>
        /// <param name="info">FileOperateInfo类型的文件信息对象</param>
        /// <param name="position">读取起始位置</param>
        /// <param name="length">读取长度</param>
        /// <param name="bytes">存储读取内容的字节数组</param>
        /// <returns>若读取成功返回true，否则返回false</returns>
        bool Read(FileOperateInfo info, long position, long length, byte[] bytes);

        /// <summary>
        /// 从指定位置开始读取指定长度的文件内容到指定字节数组中
        /// </summary>
        /// <param name="fileOperate">IConcurrentOperate类型的文件操作接口对象，可为null</param>
        /// <param name="position">读取起始位置</param>
        /// <param name="length">读取长度</param>
        /// <param name="bytes">存储读取内容的字节数组</param>
        /// <returns>若读取成功返回true，否则返回false</returns>
        bool Read(IConcurrentOperate? fileOperate, long position, long length, byte[] bytes);

        #endregion

        #region ReadAsync

        /// <summary>
        /// 异步读取文件内容
        /// </summary>
        /// <param name="info">文件信息对象，支持多种类型，包括ExpectLocalFileInfo、LocalFileInfo、FileOperateInfo、IConcurrentOperate</param>
        /// <param name="position">读取的起始位置</param>
        /// <param name="length">读取的长度</param>
        /// <param name="callback">读取完成后的回调函数，参数为读取到的字节数组（可能为null）</param>
        void ReadAsync(ExpectLocalFileInfo info, long position, long length, Action<byte[]?> callback);

        /// <summary>
        /// 异步读取文件内容。
        /// </summary>
        /// <param name="info">文件信息对象。</param>
        /// <param name="position">开始读取的位置。</param>
        /// <param name="length">要读取的长度。</param>
        /// <param name="callback">读取完成后的回调函数，参数为读取到的字节数组（可能为null）。</param>
        void ReadAsync(LocalFileInfo info, long position, long length, Action<byte[]?> callback);

        /// <summary>
        /// 异步读取文件内容
        /// </summary>
        /// <param name="info">文件操作信息对象，类型为FileOperateInfo</param>
        /// <param name="position">读取的起始位置</param>
        /// <param name="length">读取的长度</param>
        /// <param name="callback">读取完成后的回调函数，参数为读取到的字节数组（可能为null）</param>
        void ReadAsync(FileOperateInfo info, long position, long length, Action<byte[]?> callback);

        /// <summary>
        /// 异步读取文件内容
        /// </summary>
        /// <param name="fileOperate">文件操作接口对象，类型为IConcurrentOperate，可能为null</param>
        /// <param name="position">读取的起始位置</param>
        /// <param name="length">读取的长度</param>
        /// <param name="callback">读取完成后的回调函数，参数为读取到的字节数组（可能为null）</param>
        void ReadAsync(IConcurrentOperate? fileOperate, long position, long length, Action<byte[]?> callback);

        /// <summary>
        /// 异步读取文件内容到指定字节数组
        /// </summary>
        /// <param name="info">文件信息对象，支持多种类型，包括ExpectLocalFileInfo、LocalFileInfo、FileOperateInfo、IConcurrentOperate</param>
        /// <param name="position">读取的起始位置</param>
        /// <param name="length">读取的长度</param>
        /// <param name="bytes">用于存储读取内容的字节数组</param>
        /// <param name="callback">读取完成后的回调函数，参数为操作是否成功</param>
        void ReadAsync(ExpectLocalFileInfo info, long position, long length, byte[] bytes, Action<bool> callback);

        /// <summary>
        /// 异步读取文件内容。
        /// </summary>
        /// <param name="info">本地文件信息。</param>
        /// <param name="position">开始读取的位置。</param>
        /// <param name="length">要读取的长度。</param>
        /// <param name="bytes">存储读取数据的字节数组。</param>
        /// <param name="callback">读取完成后的回调函数，参数为读取是否成功。</param>
        void ReadAsync(LocalFileInfo info, long position, long length, byte[] bytes, Action<bool> callback);

        /// <summary>
        /// 异步读取文件内容到指定字节数组
        /// </summary>
        /// <param name="info">文件操作信息对象，类型为FileOperateInfo</param>
        /// <param name="position">读取的起始位置</param>
        /// <param name="length">读取的长度</param>
        /// <param name="bytes">用于存储读取内容的字节数组</param>
        /// <param name="callback">读取完成后的回调函数，参数为操作是否成功</param>
        void ReadAsync(FileOperateInfo info, long position, long length, byte[] bytes, Action<bool> callback);

        /// <summary>
        /// 异步读取文件内容到指定字节数组
        /// </summary>
        /// <param name="fileOperate">文件操作接口对象，类型为IConcurrentOperate，可能为null</param>
        /// <param name="position">读取的起始位置</param>
        /// <param name="length">读取的长度</param>
        /// <param name="bytes">用于存储读取内容的字节数组</param>
        /// <param name="callback">读取完成后的回调函数，参数为操作是否成功</param>
        void ReadAsync(IConcurrentOperate? fileOperate, long position, long length, byte[] bytes, Action<bool> callback);

        #endregion

        #region Write

        /// <summary>
        /// 将数据写入到指定的文件中
        /// </summary>
        /// <param name="info">包含本地文件信息的对象</param>
        /// <param name="bytes">要写入文件的字节数组</param>
        /// <param name="filePosition">写入文件的起始位置</param>
        void Write(ExpectLocalFileInfo info, byte[] bytes, long filePosition);

        /// <summary>
        /// 将字节数据写入到指定文件中。
        /// </summary>
        /// <param name="info">文件信息对象，包含文件的路径和名称。</param>
        /// <param name="bytes">要写入的字节数据。</param>
        /// <param name="filePosition">要写入的文件位置。</param>
        void Write(LocalFileInfo info, byte[] bytes, long filePosition);

        /// <summary>
        /// 将数据写入到指定的文件中
        /// </summary>
        /// <param name="info">包含文件操作信息的对象</param>
        /// <param name="bytes">要写入文件的字节数组</param>
        /// <param name="filePosition">写入文件的起始位置</param>
        void Write(FileOperateInfo info, byte[] bytes, long filePosition);

        /// <summary>
        /// 将数据写入到指定的文件中
        /// </summary>
        /// <param name="fileOperate">并发文件操作对象，可以为null</param>
        /// <param name="bytes">要写入文件的字节数组</param>
        /// <param name="filePosition">写入文件的起始位置</param>
        void Write(IConcurrentOperate? fileOperate, byte[] bytes, long filePosition);

        /// <summary>
        /// 将指定字节写入到本地文件中。
        /// </summary>
        /// <param name="info">本地文件信息。</param>
        /// <param name="bytes">要写入的字节数据。</param>
        /// <param name="filePosition">文件写入位置。</param>
        /// <param name="bytesPosition">字节数据的起始位置。</param>
        /// <param name="bytesLength">要写入的字节长度。</param>
        void Write(ExpectLocalFileInfo info, byte[] bytes, long filePosition, int bytesPosition, int bytesLength);

        /// <summary>
        /// 将指定字节序列写入到文件中。
        /// </summary>
        /// <param name="info">文件信息对象，包含文件的路径和名称。</param>
        /// <param name="bytes">包含要写入文件的字节序列的只读字节跨度。</param>
        /// <param name="filePosition">文件中的起始位置，从该位置开始写入字节。</param>
        /// <param name="bytesPosition">字节序列中的起始位置，从该位置开始读取字节。</param>
        /// <param name="bytesLength">要写入文件的字节长度。</param>
        void Write(LocalFileInfo info, byte[] bytes, long filePosition, int bytesPosition, int bytesLength);

        /// <summary>
        /// 将指定字节写入到文件中。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <param name="bytes">要写入的字节数据。</param>
        /// <param name="filePosition">文件写入位置。</param>
        /// <param name="bytesPosition">字节数据的起始位置。</param>
        /// <param name="bytesLength">要写入的字节长度。</param>
        void Write(FileOperateInfo info, byte[] bytes, long filePosition, int bytesPosition, int bytesLength);

        /// <summary>
        /// 将指定字节写入到文件中。
        /// </summary>
        /// <param name="fileOperate">并发文件操作信息。</param>
        /// <param name="bytes">要写入的字节数据。</param>
        /// <param name="filePosition">文件写入位置。</param>
        /// <param name="bytesPosition">字节数据的起始位置。</param>
        /// <param name="bytesLength">要写入的字节长度。</param>
        void Write(IConcurrentOperate? fileOperate, byte[] bytes, long filePosition, int bytesPosition, int bytesLength);

        #endregion

        #region WriteAsync

        /// <summary>
        /// 异步写入文件。
        /// </summary>
        /// <param name="info">文件信息对象。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="filePosition">要写入的文件位置。</param>
        /// <param name="callback">写入完成后的回调函数，参数为写入的字节数组。默认为null。</param>
        void WriteAsync(ExpectLocalFileInfo info, byte[] bytes, long filePosition, Action<byte[]>? callback = null);

        /// <summary>
        /// 异步写入文件。
        /// </summary>
        /// <param name="info">文件信息对象。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="filePosition">要写入的文件位置。</param>
        /// <param name="callback">写入完成后的回调函数，参数为写入的字节数组。默认为null。</param>
        void WriteAsync(LocalFileInfo info, byte[] bytes, long filePosition, Action<byte[]>? callback = null);

        /// <summary>
        /// 异步写入文件。
        /// </summary>
        /// <param name="info">文件操作信息对象。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="filePosition">要写入的文件位置。</param>
        /// <param name="callback">写入完成后的回调函数，参数为写入的字节数组。默认为null。</param>
        void WriteAsync(FileOperateInfo info, byte[] bytes, long filePosition, Action<byte[]>? callback = null);

        /// <summary>
        /// 异步写入文件。
        /// </summary>
        /// <param name="fileOperate">并发文件操作对象。默认为null。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="filePosition">要写入的文件位置。</param>
        /// <param name="callback">写入完成后的回调函数，参数为写入的字节数组。默认为null。</param>
        void WriteAsync(IConcurrentOperate? fileOperate, byte[] bytes, long filePosition, Action<byte[]>? callback = null);

        /// <summary>
        /// 异步写入文件的方法。
        /// </summary>
        /// <param name="info">文件信息对象。</param>
        /// <param name="bytes">要写入文件的数据字节数组。</param>
        /// <param name="filePosition">文件开始写入的位置。</param>
        /// <param name="bytesPosition">字节数组开始写入的位置。</param>
        /// <param name="bytesLength">要写入的字节数。</param>
        /// <param name="callback">写入完成后的回调函数，参数为写入的字节数组。</param>
        void WriteAsync(ExpectLocalFileInfo info, byte[] bytes, long filePosition, int bytesPosition, int bytesLength, Action<byte[]>? callback = null);

        /// <summary>
        /// 异步写入文件的方法。
        /// </summary>
        /// <param name="info">本地文件信息对象。</param>
        /// <param name="bytes">要写入文件的数据字节数组。</param>
        /// <param name="filePosition">文件开始写入的位置。</param>
        /// <param name="bytesPosition">字节数组开始写入的位置。</param>
        /// <param name="bytesLength">要写入的字节数。</param>
        /// <param name="callback">写入完成后的回调函数，参数为写入的字节数组。</param>
        void WriteAsync(LocalFileInfo info, byte[] bytes, long filePosition, int bytesPosition, int bytesLength, Action<byte[]>? callback = null);

        /// <summary>
        /// 异步写入文件的方法。
        /// </summary>
        /// <param name="info">文件操作信息对象。</param>
        /// <param name="bytes">要写入文件的数据字节数组。</param>
        /// <param name="filePosition">文件开始写入的位置。</param>
        /// <param name="bytesPosition">字节数组开始写入的位置。</param>
        /// <param name="bytesLength">要写入的字节数。</param>
        /// <param name="callback">写入完成后的回调函数，参数为写入的字节数组。</param>
        void WriteAsync(FileOperateInfo info, byte[] bytes, long filePosition, int bytesPosition, int bytesLength, Action<byte[]>? callback = null);

        /// <summary>
        /// 异步写入文件的方法。
        /// </summary>
        /// <param name="fileOperate">并发操作接口对象。</param>
        /// <param name="bytes">要写入文件的数据字节数组。</param>
        /// <param name="filePosition">文件开始写入的位置。</param>
        /// <param name="bytesPosition">字节数组开始写入的位置。</param>
        /// <param name="bytesLength">要写入的字节数。</param>
        /// <param name="callback">写入完成后的回调函数，参数为写入的字节数组。</param>
        void WriteAsync(IConcurrentOperate? fileOperate, byte[] bytes, long filePosition, int bytesPosition, int bytesLength, Action<byte[]>? callback = null);

        #endregion

        /// <summary>
        /// 获取指定类型对象中的序列化后的长度。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">对象。</param>
        /// <returns>序列化后的长度。</returns>
        long GetLength<T>(T value);

        /// <summary>
        /// 获取指定类型对象的默认序列化长度。
        /// </summary>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <returns>默认序列化长度。</returns>
        long GetDefaulLength<T>();

        /// <summary>
        /// 获取指定类型的二进制格式化器。
        /// </summary>
        /// <typeparam name="T">要获取格式化器的类型。</typeparam>
        /// <returns>返回指定类型的二进制格式化器。</returns>
        IBinaryFormatter<T> GetFormatter<T>();
    }
}
