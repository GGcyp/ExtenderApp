namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义一个本地格式化器接口，用于将对象序列化为二进制格式，并提供版本信息。
    /// </summary>
    /// <typeparam name="T">需要格式化的对象类型。</typeparam>
    public interface IVersionDataFormatter<T> : IBinaryFormatter<T>
    {
        /// <summary>
        /// 获取格式化器的版本信息。
        /// </summary>
        Version FormatterVersion { get; }
    }
}
