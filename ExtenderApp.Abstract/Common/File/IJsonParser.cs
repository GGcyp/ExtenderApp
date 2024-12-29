using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义一个用于解析JSON文件的接口
    /// </summary>
    public interface IJsonParser : IFileParser
    {
        /// <summary>
        /// 反序列化给定的文件信息数据到指定类型的对象。
        /// </summary>
        /// <param name="operate">包含文件信息的 Fileoperate 对象。</param>
        /// <param name="type">目标对象的类型。</param>
        /// <param name="options">可选的反序列化选项。</param>
        /// <returns>反序列化后的对象，如果反序列化失败则返回 null。</returns>
        object? Deserialize(FileOperate operate, Type type, object? options = null);

        /// <summary>
        /// 从给定的 JSON 字符串反序列化到指定类型的对象。
        /// </summary>
        /// <param name="json">包含 JSON 数据的字符串。</param>
        /// <param name="type">目标对象的类型。</param>
        /// <param name="options">可选的反序列化选项。</param>
        /// <returns>反序列化后的对象，如果反序列化失败则返回 null。</returns>
        object? Deserialize(string json, Type type, object? options = null);

        /// <summary>
        /// 异步反序列化给定的文件信息数据为指定类型的对象。
        /// </summary>
        /// <param name="operate">包含文件信息的 Fileoperate 对象。</param>
        /// <param name="type">要反序列化为的目标类型。</param>
        /// <param name="options">可选的反序列化选项。</param>
        /// <returns>异步返回反序列化后的对象。</returns>
        ValueTask<object?> DeserializeAsync(FileOperate operate, Type type, object? options = null);

        /// <summary>
        /// 将指定格式的数据反序列化为指定类型的对象。
        /// </summary>
        /// <typeparam name="T">要反序列化的目标类型。</typeparam>
        /// <param name="operate">包含反序列化所需信息的 <see cref="FileOperate"/> 对象。</param>
        /// <param name="options">可选的，反序列化时使用的附加选项。</param>
        /// <returns>反序列化后的对象；如果反序列化失败，则返回 null。</returns>
        T? Deserialize<T>(FileOperate operate, object? options = null);

        /// <summary>
        /// 将指定的 JSON 字符串反序列化为指定类型的对象。
        /// </summary>
        /// <typeparam name="T">要反序列化的目标类型。</typeparam>
        /// <param name="json">包含要反序列化的 JSON 数据的字符串。</param>
        /// <param name="options">可选的，反序列化时使用的附加选项。</param>
        /// <returns>反序列化后的对象；如果反序列化失败，则返回 null。</returns>
        T? Deserialize<T>(string json, object? options = null);

        /// <summary>
        /// 异步反序列化方法。
        /// </summary>
        /// <typeparam name="T">反序列化目标类型的泛型参数。</typeparam>
        /// <param name="operate">包含文件信息的 Fileoperate 对象。</param>
        /// <param name="options">可选的反序列化选项对象，可以为 null。</param>
        /// <returns>返回一个包含反序列化结果的 ValueTask 对象，结果可能为 null。</returns>
        ValueTask<T?> DeserializeAsync<T>(FileOperate operate, object? options = null);

        /// <summary>
        /// 将给定的对象序列化为文件信息数据。
        /// </summary>
        /// <param name="jsonObject">要序列化的对象。</param>
        /// <param name="operate">包含文件信息的 Fileoperate 对象。</param>
        /// <param name="options">可选的序列化选项。</param>
        /// <returns>如果序列化成功则返回 true，否则返回 false。</returns>
        bool Serialize(object jsonObject, FileOperate operate, object? options = null);

        /// <summary>
        /// 将给定的对象序列化为 JSON 字符串。
        /// </summary>
        /// <param name="jsonObject">要序列化的对象。</param>
        /// <param name="options">可选的序列化选项。</param>
        /// <returns>序列化后的 JSON 字符串。</returns>
        string Serialize(object jsonObject, object? options = null);

        /// <summary>
        /// 异步将给定的对象序列化为文件信息数据。
        /// </summary>
        /// <param name="jsonObject">要序列化的对象。</param>
        /// <param name="operate">包含文件信息的 Fileoperate 对象。</param>
        /// <param name="options">可选的序列化选项。</param>
        /// <returns>异步返回序列化是否成功的布尔值。</returns>
        ValueTask<bool> SerializeAsync(object jsonObject, FileOperate operate, object? options = null);
    }
}
