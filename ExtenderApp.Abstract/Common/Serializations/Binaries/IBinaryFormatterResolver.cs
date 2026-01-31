namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 二进制格式化器解析器接口
    /// </summary>
    public interface IBinaryFormatterResolver
    {
        /// <summary>
        /// 获取指定类型的二进制格式化器
        /// </summary>
        /// <param name="type">类型参数</param>
        /// <returns>返回二进制格式化器的接口</returns>
        IBinaryFormatter GetFormatter(Type type);

        /// <summary>
        /// 获取指定类型的二进制格式化器
        /// </summary>
        /// <typeparam name="T">类型参数</typeparam>
        /// <returns>返回指定类型的二进制格式化器</returns>
        IBinaryFormatter<T> GetFormatter<T>();
    }
}