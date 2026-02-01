namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 二进制序列化契约，扩展自 <see cref="ISerialization"/>，提供针对二进制格式的辅助方法。
    /// <para>该接口定义获取序列化长度与类型格式化器的能力，便于实现高性能/无拷贝的二进制读写。</para>
    /// </summary>
    /// <remarks>实现应保证与 <see cref="ISerialization"/> 的语义一致：序列化/反序列化为内存级别操作， 并对长度估算、格式化器获取等方法提供合理实现。异常处理策略由具体实现决定。</remarks>
    public interface IBinarySerialization : ISerialization
    {
        /// <summary>
        /// 计算或估算指定对象按当前二进制格式序列化后的字节长度。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="value">要序列化的对象实例。</param>
        /// <returns>返回序列化后的字节长度（可能为精确值或估算值）。实现可用于缓冲预分配。</returns>
        long GetLength<T>(T value);

        /// <summary>
        /// 获取指定类型在当前序列化格式下的默认序列化长度（若格式化器提供此信息）。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <returns>返回类型的默认序列化字节长度；若格式化器不提供默认值，返回实现约定的值（例如 0 或 -1 表示未知）。</returns>
        long GetDefaulLength<T>();

        /// <summary>
        /// 尝试获取指定类型的二进制格式化器。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="formatter">二进制格式化器实例</param>
        /// <returns>若成功获取格式化器则返回 true，否则返回 false。</returns>
        bool TryGetFormatter<T>(out IBinaryFormatter<T> formatter);
    }
}