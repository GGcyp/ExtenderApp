using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件解析器接口
    /// </summary>
    public interface IFileParser
    {
        /// <summary>
        /// 将对象序列化为文件。
        /// </summary>
        /// <typeparam name="T">要序列化的对象的类型。</typeparam>
        /// <param name="operate">文件操作对象。</param>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="options">序列化选项，可以为null。</param>
        /// <returns>如果序列化成功返回true，否则返回false。</returns>
        bool Serialize<T>(FileOperate operate, T value, object? options = null);

        /// <summary>
        /// 将指定格式的数据反序列化为指定类型的对象。
        /// </summary>
        /// <typeparam name="T">要反序列化的目标类型。</typeparam>
        /// <param name="operate">包含反序列化所需信息的 <see cref="FileOperate"/> 对象。</param>
        /// <param name="options">可选的，反序列化时使用的附加选项。</param>
        /// <returns>反序列化后的对象；如果反序列化失败，则返回 null。</returns>
        T? Deserialize<T>(FileOperate operate, object? options = null);
    }
}
