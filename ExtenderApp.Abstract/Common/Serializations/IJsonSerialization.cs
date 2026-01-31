namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供 JSON 序列化与反序列化的契约，基于 <see cref="ISerialization"/> 扩展字符串级别的操作。
    /// <para>实现应负责将对象与 JSON 文本相互转换，要求对常见异常（如格式错误）做出合理处理或返回 null。</para>
    /// </summary>
    public interface IJsonSerialization : ISerialization
    {
        /// <summary>
        /// 将指定的 JSON 文本反序列化为目标类型的对象实例。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="json">包含 JSON 数据的字符串（不得为 <c>null</c>；实现可对空/空白字符串返回 null 或抛出异常）。</param>
        /// <returns>反序列化得到的对象实例；若解析失败或输入无效可返回 <c>null</c>（或由实现抛出异常）。</returns>
        T? Deserialize<T>(string json);

        /// <summary>
        /// 将指定对象序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="value">要序列化的对象（可为 <c>null</c>）。</param>
        /// <returns>序列化得到的 JSON 文本（非 null，格式由实现决定）。</returns>
        string SerializeToString<T>(T value);
    }
}