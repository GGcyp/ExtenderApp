
namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示不同类型的二进制编码值。
    /// </summary>
    public enum BinaryType : byte
    {
        /// <summary>
        /// 未知类型。
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 整数类型。
        /// </summary>
        Integer = 1,
        /// <summary>
        /// 空值类型。
        /// </summary>
        Nil = 2,
        /// <summary>
        /// 布尔类型。
        /// </summary>
        Boolean = 3,
        /// <summary>
        /// 浮点类型。
        /// </summary>
        Float = 4,
        /// <summary>
        /// 字符串类型。
        /// </summary>
        String = 5,
        /// <summary>
        /// 二进制数据类型。
        /// </summary>
        Binary = 6,
        /// <summary>
        /// 数组类型。
        /// </summary>
        Array = 7,
        /// <summary>
        /// 映射类型。
        /// </summary>
        Map = 8,
        /// <summary>
        /// 扩展类型。
        /// </summary>
        Extension = 9,
    }
}
