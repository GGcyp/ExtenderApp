namespace MainApp.Mod.PPR
{
    /// <summary>
    /// 每期工程量支付情况实例类
    /// </summary>
    public class PPRPeriodQuantityEntity
    {
        /// <summary>
        /// 每期最后修改时间
        /// </summary>
        public string? Time { get; set; }

        /// <summary>
        /// 期数
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// 本期金额
        /// </summary>
        public double FrequencyAmount{ get; set; }

        /// <summary>
        /// 本期工程量
        /// </summary>
        public double FrequencyQuantity{ get; set; }

        /// <summary>
        /// 本期已报工程量
        /// </summary>
        public double FrequencyReportedQuantity { get; set; }

        /// <summary>
        /// 本期备注
        /// </summary>
        public string FrequencyRemark { get; set; }
    }
}
