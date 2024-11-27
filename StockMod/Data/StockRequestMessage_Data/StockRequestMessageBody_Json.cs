namespace StockMod.Data
{
    internal struct StockRequestMessageBody_Json
    {
        private readonly StockRequestMessageBody _body;

        /// <summary>
        /// 股票代码
        /// </summary>
        public string code => _body.Code;

        /// <summary>
        /// K线类型
        /// </summary>
        public int kline_type =>_body.KlineType;

        /// <summary>
        /// K线结束时间戳
        /// </summary>
        public int kline_timestamp_end => _body.KlineTimestampEnd;

        /// <summary>
        /// 查询K线数量
        /// </summary>
        public int query_kline_num => _body.QueryKlineNum;

        /// <summary>
        /// 复权类型
        /// </summary>
        public int adjust_type => _body.AdjustType;

        public StockRequestMessageBody_Json(StockRequestMessageBody body)
        {
            _body = body;
        }
    }
}
