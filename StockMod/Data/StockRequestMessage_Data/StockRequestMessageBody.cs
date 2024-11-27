namespace StockMod.Data
{
    /// <summary>
    /// 股票请求消息数据类
    /// </summary>
    internal struct StockRequestMessageBody
    {
        /// <summary>
        /// 股票代码
        /// </summary>
        public string Code{ get; set; }

        /// <summary>
        /// K线类型
        /// </summary>
        public int KlineType { get; set; }

        /// <summary>
        /// K线结束时间戳
        /// </summary>
        public int KlineTimestampEnd { get; set; }

        /// <summary>
        /// 查询K线数量
        /// </summary>
        public int QueryKlineNum { get; set; }

        /// <summary>
        /// 复权类型
        /// </summary>
        public int AdjustType { get; set; }
    }
}
