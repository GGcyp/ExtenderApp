namespace StockMod.Data
{
    internal struct StockRequestMessage_Json
    {
        /// <summary>
        /// 跟踪信息
        /// </summary>
        public string trace { get; }

        /// <summary>
        /// 股票请求消息数据
        /// </summary>
        public StockRequestMessageBody_Json data { get; }

        public StockRequestMessage_Json(string trace, StockRequestMessageBody_Json data)
        {
            this.trace = trace;
            //data = new(data);
        }
    }
}
