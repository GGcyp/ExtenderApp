namespace MainApp.Mod.PPR
{
    /// <summary>
    /// 清单项工程付款及工程量数据
    /// ProjectPaymentRecordData
    /// </summary>
    public class PPRInventoryEntity
    {
        /// <summary>
        /// 清单ID
        /// </summary>
        public string ProjectID { get; set; }

        /// <summary>
        /// 单项工程名字
        /// </summary>
        public string InventoryProjectName { get; set; }

        /// <summary>
        /// 项目特征
        /// </summary>
        public string? ProjectFeatureDescription { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 单项目总金额
        /// </summary>
        public double TotalAmount { get; set; }

        /// <summary>
        /// 当前完成工程量
        /// </summary>
        public double CompletedQuantity { get; set; }

        /// <summary>
        /// 已报工程量
        /// </summary>
        public double ReportedQuantity { get; set; }

        /// <summary>
        /// 清单内工程量
        /// </summary>
        public double BillOfQuantitiesQuantity { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public double UnitPrice { get; set; }

        /// <summary>
        /// 剩余清单内可报工程量
        /// </summary>
        public double RemainingBillQuantity { get; set; }

        /// <summary>
        /// 剩余现场实际工程量
        /// </summary>
        public double RemainingActualQuantity { get; set; }

        /// <summary>
        /// 当前清单备注项
        /// </summary>
        public string? ProjectRemark { get; set; }

        public List<PPRPeriodQuantityEntity>? PeriodQuantityList { get; set; }

        /// <summary>
        /// 计算数据
        /// </summary>
        public void Calculate()
        {
            //if (m_QuantityPeriods == null) return;

            //double t_CompletedQuantity = 0;
            //double t_ReportedQuantity = 0;

            //for (int i = 0; i < m_QuantityPeriods.Count; i++)
            //{
            //    var item = m_QuantityPeriods[i];
            //    t_CompletedQuantity += item.FrequencyQuantity;
            //    t_ReportedQuantity += item.FrequencyReportedQuantity;
            //    item.FrequencyAmount = item.FrequencyReportedQuantity * UnitPrice;
            //}

            //CompletedQuantity = t_CompletedQuantity;
            //ReportedQuantity = t_ReportedQuantity;

            //RemainingBillQuantity = BillOfQuantitiesQuantity - ReportedQuantity;
            //RemainingActualQuantity = CompletedQuantity - ReportedQuantity;
        }
    }
}
