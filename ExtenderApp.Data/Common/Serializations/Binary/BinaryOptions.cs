namespace ExtenderApp.Data
{
    /// <summary>
    /// 二进制选项类
    /// </summary>
    public static class BinaryOptions
    {
        /// <summary>
        /// 空值编码。
        /// </summary>
        public static byte Nil { get; set; } = 0xc0;

        /// <summary>
        /// 布尔值False编码。
        /// </summary>
        public static byte False { get; set; } = 0xc2;

        /// <summary>
        /// 布尔值True编码。
        /// </summary>
        public static byte True { get; set; } = 0xc3;

        /// <summary>
        /// 32位浮点数数据标记。
        /// </summary>
        public static byte Float32 { get; set; } = 0xca;

        /// <summary>
        /// 64位浮点数数据标记。
        /// </summary>
        public static byte Float64 { get; set; } = 0xcb;

        /// <summary>
        /// 8位无符号整数数据标记。
        /// </summary>
        public static byte UInt8 { get; set; } = 0xcc;

        /// <summary>
        /// 16位无符号整数数据标记。
        /// </summary>
        public static byte UInt16 { get; set; } = 0xcd;

        /// <summary>
        /// 32位无符号整数数据标记。
        /// </summary>
        public static byte UInt32 { get; set; } = 0xce;

        /// <summary>
        /// 64位无符号整数数据标记。
        /// </summary>
        public static byte UInt64 { get; set; } = 0xcf;

        /// <summary>
        /// 8位有符号整数数据标记。
        /// </summary>
        public static byte Int8 { get; set; } = 0xd0;

        /// <summary>
        /// 16位有符号整数数据标记。
        /// </summary>
        public static byte Int16 { get; set; } = 0xd1;

        /// <summary>
        /// 32位有符号整数数据标记。
        /// </summary>
        public static byte Int32 { get; set; } = 0xd2;

        /// <summary>
        /// 64位有符号整数数据标记。
        /// </summary>
        public static byte Int64 { get; set; } = 0xd3;

        /// <summary>
        /// 8位字符串长度编码标记。
        /// </summary>
        public static byte String { get; set; } = 0xd9;

        /// <summary>
        /// 数组长度编码标记。
        /// </summary>
        public static byte Array { get; set; } = 0xdd;

        public static byte MapHeader { get; set; } = 0xde;
    }
}