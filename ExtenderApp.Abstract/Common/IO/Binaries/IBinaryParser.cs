
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
        /// <param name="options">反序列化选项对象，可以为null。</param>
        /// <returns>反序列化后的对象，如果流为空或格式不正确则返回null。</returns>
        T? Deserialize<T>(Stream stream, object? options = null);

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
        /// 将对象序列化为二进制数据并写入到流中。
        /// </summary>
        /// <typeparam name="T">要序列化的对象的类型。</typeparam>
        /// <param name="stream">要写入数据的流。</param>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="options">序列化选项，可以为null。</param>
        /// <returns>如果序列化成功，则返回true；否则返回false。</returns>
        void Serialize<T>(Stream stream, T value, object? options = null);

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

        /// <summary>
        /// 获取指定类型对象中的序列化后的长度。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">对象。</param>
        /// <returns>序列化后的长度。</returns>
        long GetLength<T>(T value);

        /// <summary>
        /// 从指定位置读取指定长度的数据。
        /// </summary>
        /// <param name="info">文件操作信息对象，包含文件的路径等信息。</param>
        /// <param name="position">开始读取的位置（以字节为单位）。</param>
        /// <param name="length">要读取的长度（以字节为单位）。</param>
        /// <param name="fileOperate">并发操作接口，用于处理并发读写等操作，可为null。</param>
        /// <param name="bytes">预分配的字节数组，如果提供，则读取的数据将写入此数组中，否则将分配一个新的字节数组。</param>
        /// <returns>包含读取数据的字节数组。</returns>
        byte[] Read(FileOperateInfo info, long position, long length, IConcurrentOperate? fileOperate = null, byte[]? bytes = null);

        /// <summary>
        /// 从指定位置读取指定长度的数据。
        /// </summary>
        /// <param name="info">期望的本地文件信息对象，包含文件的路径等信息。</param>
        /// <param name="position">开始读取的位置（以字节为单位）。</param>
        /// <param name="length">要读取的长度（以字节为单位）。</param>
        /// <param name="fileOperate">并发操作接口，用于处理并发读写等操作，可为null。</param>
        /// <param name="bytes">预分配的字节数组，如果提供，则读取的数据将写入此数组中，否则将分配一个新的字节数组。</param>
        /// <returns>包含读取数据的字节数组。</returns>
        byte[] Read(ExpectLocalFileInfo info, long position, long length, IConcurrentOperate? fileOperate = null, byte[]? bytes = null);

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
