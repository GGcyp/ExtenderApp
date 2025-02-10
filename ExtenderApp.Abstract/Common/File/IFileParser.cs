using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件解析器接口
    /// </summary>
    public interface IFileParser
    {
        /// <summary>
        /// 将对象序列化为文件
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="value">需要序列化的对象</param>
        /// <param name="options">序列化选项，可以为null</param>
        /// <returns>如果序列化成功，则返回true；否则返回false</returns>
        bool Serialize<T>(ExpectLocalFileInfo info, T value, object? options = null);

        /// <summary>
        /// 将指定对象序列化为二进制格式并保存到文件中。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="operate">文件操作接口，用于执行文件读写操作。</param>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="options">可选的序列化选项，可以为null。</param>
        /// <returns>如果序列化成功，则返回true；否则返回false。</returns>
        bool Serialize<T>(FileOperate operate, T value, object? options = null);

        /// <summary>
        /// 异步将对象序列化为文件
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="value">需要序列化的对象</param>
        /// <param name="options">序列化选项，可以为null</param>
        /// <returns>异步任务，如果序列化成功，则任务返回true；否则返回false</returns>
        ValueTask<bool> SerializeAsync<T>(ExpectLocalFileInfo info, T value, object? options = null);

        /// <summary>
        /// 异步序列化对象并保存到文件。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="operate">文件操作枚举，指定是写入还是追加。</param>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="options">序列化选项，可以为null。</param>
        /// <returns>返回一个<see cref="ValueTask{TResult}"/>，表示异步操作的结果。如果操作成功，返回true；否则返回false。</returns>
        ValueTask<bool> SerializeAsync<T>(FileOperate operate, T value, object? options = null);

        /// <summary>
        /// 将文件反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="options">反序列化选项，可以为null</param>
        /// <returns>反序列化后的对象，如果反序列化失败，则返回null</returns>
        T? Deserialize<T>(ExpectLocalFileInfo info, object? options = null);

        /// <summary>
        /// 反序列化文件中的数据为指定类型T的对象。
        /// </summary>
        /// <typeparam name="T">目标对象的类型。</typeparam>
        /// <param name="operate">文件操作对象，用于读取文件内容。</param>
        /// <param name="options">可选的反序列化选项，可以为null。</param>
        /// <returns>返回反序列化后的对象，如果反序列化失败则返回null。</returns>
        T? Deserialize<T>(FileOperate operate, object? options = null);

        /// <summary>
        /// 异步将文件反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="options">反序列化选项，可以为null</param>
        /// <returns>异步任务，返回反序列化后的对象，如果反序列化失败，则返回null</returns>
        ValueTask<T?> DeserializeAsync<T>(ExpectLocalFileInfo info, object? options = null);

        /// <summary>
        /// 从文件中异步反序列化对象。
        /// </summary>
        /// <typeparam name="T">需要反序列化的目标类型。</typeparam>
        /// <param name="operate">文件操作对象，包含文件路径等信息。</param>
        /// <param name="options">可选参数，用于配置反序列化行为。</param>
        /// <returns>返回反序列化后的对象，类型为T。</returns>
        ValueTask<T?> DeserializeAsync<T>(FileOperate operate, object? options = null);
    }
}
