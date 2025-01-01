namespace ExtenderApp.Data.File
{
    /// <summary>
    /// 表示二进制范围的结构体。
    /// </summary>
    public struct BinaryRang
    {
        /// <summary>
        /// 最小的固定负整数值。
        /// </summary>
        public int MinFixNegativeInt { get; set; }

        /// <summary>
        /// 最大的固定负整数值。
        /// </summary>
        public int MaxFixNegativeInt { get; set; }

        /// <summary>
        /// 最大的固定正整数值。
        /// </summary>
        public int MaxFixPositiveInt { get; set; }

        /// <summary>
        /// 最小的固定字符串长度。
        /// </summary>
        public int MinFixStringLength { get; set; }

        /// <summary>
        /// 最大的固定字符串长度。
        /// </summary>
        public int MaxFixStringLength { get; set; }

        /// <summary>
        /// 最大的固定映射数量。
        /// </summary>
        public int MaxFixMapCount { get; set; }

        /// <summary>
        /// 最大的固定数组数量。
        /// </summary>
        public int MaxFixArrayCount { get; set; }

        /// <summary>
        /// 默认构造函数，用于初始化各个属性为对应的常量值。
        /// </summary>
        public BinaryRang()
        {
            MinFixNegativeInt = -32;
            MaxFixNegativeInt = -1;
            MaxFixPositiveInt = 127;
            MinFixStringLength = 0;
            MaxFixStringLength = 31;
            MaxFixMapCount = 15;
            MaxFixArrayCount = 15;
        }
    }
}
