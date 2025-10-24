namespace ExtenderApp.Data
{
    /// <summary>
    /// 链路头部信息，用于描述数据包的基础元数据。
    /// </summary>
    /// <remarks>
    /// 为值类型且仅包含只读属性，构造后不可变，适合在网络读写中按值传递。
    /// </remarks>
    public readonly struct LinkHeade
    {
        /// <summary>
        /// 负载数据类型标识。
        /// </summary>
        public int DataType { get; }

        /// <summary>
        /// 负载数据长度（字节）。用于指明负载区的大小，便于一次性读取或校验。
        /// </summary>
        public int DataLength { get; }

        /// <summary>
        /// 使用指定的数据类型与头部长度构造 <see cref="LinkHeade"/>。
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="dataLength">数据长度</param>
        public LinkHeade(int dataType, int dataLength)
        {
            DataType = dataType;
            DataLength = dataLength;
        }
    }
}
