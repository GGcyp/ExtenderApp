namespace ExtenderApp.Data
{
    /// <summary>
    /// 链路头部信息，用于描述数据包的基础元数据。
    /// </summary>
    /// <remarks>
    /// 为值类型且仅包含只读属性，构造后不可变，适合在网络读写中按值传递。
    /// </remarks>
    public struct LinkHeader
    {
        /// <summary>
        /// 负载数据类型标识。用于区分负载数据的业务类型（例如心跳、控制、业务数据等）。
        /// </summary>
        public int DataType { get; set; }

        /// <summary>
        /// 负载数据长度（字节）。用于指明负载区的大小，便于一次性读取或校验。
        /// </summary>
        public int DataLength { get; set; }

        /// <summary>
        /// 使用指定的数据类型与头部长度构造 <see cref="LinkHeader"/>。
        /// </summary>
        /// <param name="headerType">头部数据类型标识。</param>
        /// <param name="headerLength">头部长度（字节）。</param>
        /// <param name="dataType">数据类型</param>
        /// <param name="dataLength">数据长度</param>
        public LinkHeader(int dataType, int dataLength)
        {
            DataType = dataType;
            DataLength = dataLength;
        }
    }
}
