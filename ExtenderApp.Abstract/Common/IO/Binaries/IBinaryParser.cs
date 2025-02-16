

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
        /// 将对象序列化为二进制数据并写入到流中。
        /// </summary>
        /// <typeparam name="T">要序列化的对象的类型。</typeparam>
        /// <param name="stream">要写入数据的流。</param>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="options">序列化选项，可以为null。</param>
        /// <returns>如果序列化成功，则返回true；否则返回false。</returns>
        void Serialize<T>(Stream stream, T value, object? options = null);

        /// <summary>
        /// 获取指定类型对象中的元素数量。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">对象。</param>
        /// <returns>对象中的元素数量。</returns>
        int GetCount<T>(T value);
    }
}
