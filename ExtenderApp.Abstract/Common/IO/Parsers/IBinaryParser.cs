
using System.Buffers;
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
        /// <param name="span">包含序列化数据的字节数组。</param>
        /// <returns>反序列化后的对象，如果反序列化失败则返回null。</returns>
        T? Deserialize<T>(ReadOnlyMemory<byte> span);

        /// <summary>
        /// 从流中反序列化对象。
        /// </summary>
        /// <typeparam name="T">要反序列化的目标类型。</typeparam>
        /// <param name="stream">包含要反序列化数据的流。</param>
        /// <returns>反序列化后的对象，如果流为空或格式不正确则返回null。</returns>
        T? Deserialize<T>(Stream stream);

        /// <summary>
        /// 异步反序列化给定流或字节数组为指定类型的对象。
        /// </summary>
        /// <typeparam name="T">目标对象的类型。</typeparam>
        /// <param name="stream">包含要反序列化的数据的流。</param>
        /// <param name="token">用于取消操作的取消令牌。</param>
        /// <returns>反序列化后的对象，如果反序列化失败则返回null。</returns>
        Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken token);
        
        /// <summary>
        /// 从给定的字节序列中异步反序列化对象。
        /// </summary>
        /// <typeparam name="T">要反序列化的对象类型。</typeparam>
        /// <param name="span">包含要反序列化的数据的字节序列。</param>
        /// <param name="token">用于取消操作的取消令牌。</param>
        /// <returns>返回一个包含反序列化对象的Task。</returns>
        Task<T?> DeserializeAsync<T>(ReadOnlyMemory<byte> span, CancellationToken token);


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

        /// <summary>
        /// 异步地将对象序列化为字节数组。
        /// </summary>
        /// <typeparam name="T">要序列化的对象的类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <returns>返回一个包含序列化后字节的Task。</returns>
        Task<byte[]> SerializeAsync<T>(T value, CancellationToken token);

        /// <summary>
        /// 异步地将对象序列化为字节流。
        /// </summary>
        /// <typeparam name="T">要序列化的对象的类型。</typeparam>
        /// <param name="stream">要写入序列化数据的流。</param>
        /// <param name="value">要序列化的对象。</param>
        /// <returns>返回一个Task。</returns>
        Task SerializeAsync<T>(Stream stream, T value, CancellationToken token);

        #endregion

        #region LZ4

        /// <summary>
        /// 将数据写入LZ4压缩块
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="compression">压缩类型</param>
        void Write<T>(ExpectLocalFileInfo info, T value, CompressionType compression);

        /// <summary>
        /// 将数据写入LZ4压缩块
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="compression">压缩类型</param>
        void Write<T>(FileOperateInfo info, T value, CompressionType compression);

        /// <summary>
        /// 将数据写入LZ4压缩格式
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="fileOperate">文件操作接口</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="compression">压缩类型</param>
        void Write<T>(IFileOperate fileOperate, T value, CompressionType compression);

        /// <summary>
        /// 将只读内存序列写入LZ4压缩格式
        /// </summary>
        /// <param name="readOnlyMemories">只读内存序列</param>
        /// <param name="writer">二进制写入器</param>
        /// <param name="compression">压缩类型</param>
        void ToLz4(in ReadOnlySequence<byte> readOnlyMemories, ref ExtenderBinaryWriter writer, CompressionType compression);

        /// <summary>
        /// 异步地将数据写入LZ4压缩块
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="compression">压缩类型</param>
        /// <param name="callback">回调函数</param>
        void WriteAsync<T>(ExpectLocalFileInfo info, T value, CompressionType compression, Action? callback = null);

        /// <summary>
        /// 异步地将数据写入LZ4压缩块
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="compression">压缩类型</param>
        /// <param name="callback">回调函数</param>
        void WriteAsync<T>(FileOperateInfo info, T value, CompressionType compression, Action? callback = null);

        /// <summary>
        /// 异步地将数据写入LZ4压缩格式
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="fileOperate">文件操作接口</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="compression">压缩类型</param>
        /// <param name="callback">回调函数</param>
        void WriteAsync<T>(IFileOperate fileOperate, T value, CompressionType compression, Action? callback = null);

        /// <summary>
        /// 对指定类型的数据进行压缩。
        /// </summary>
        /// <typeparam name="T">要压缩的数据的类型。</typeparam>
        /// <param name="value">要压缩的数据。</param>
        /// <param name="compression">压缩类型。</param>
        /// <returns>压缩后的字节数组。</returns>
        byte[] Compression<T>(T value, CompressionType compression);

        /// <summary>
        /// 对字节序列进行压缩。
        /// </summary>
        /// <param name="input">要压缩的字节序列。</param>
        /// <param name="compression">压缩类型。</param>
        /// <returns>压缩后的字节数组。</returns>
        byte[] Compression(ReadOnlySpan<byte> input, CompressionType compression);

        /// <summary>
        /// 对只读字节序列进行压缩。
        /// </summary>
        /// <param name="readOnlyMemories">要压缩的只读字节序列。</param>
        /// <param name="compression">压缩类型。</param>
        /// <returns>压缩后的字节数组。</returns>
        byte[] Compression(in ReadOnlySequence<byte> readOnlyMemories, CompressionType compression);

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
