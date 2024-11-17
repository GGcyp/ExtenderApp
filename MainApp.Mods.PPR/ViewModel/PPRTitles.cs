using MainApp.Views.Themes;
using PropertyChanged;

namespace MainApp.Mod.PPR
{
    /// <summary>
    /// PPR详细列表数据标题类
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class PPRTitles
    {
        /// <summary>
        /// 清单ID
        /// </summary>
        public TitleDetails ProjectIDTitle { get; set; }

        /// <summary>
        /// 单项工程名字
        /// </summary>
        public TitleDetails InventoryProjectNameTitle { get; set; }

        /// <summary>
        /// 项目特征
        /// </summary>
        public TitleDetails ProjectFeatureDescriptionTitle { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public TitleDetails UnitTitle { get; set; }

        /// <summary>
        /// 单项目总金额
        /// </summary>
        public TitleDetails TotalAmountTitle { get; set; }

        /// <summary>
        /// 当前完成工程量
        /// </summary>
        public TitleDetails CompletedQuantityTitle { get; set; }

        /// <summary>
        /// 已报工程量
        /// </summary>
        public TitleDetails ReportedQuantityTitle { get; set; }

        /// <summary>
        /// 清单内工程量
        /// </summary>
        public TitleDetails BillOfQuantitiesQuantityTitle { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public TitleDetails UnitPriceTitle { get; set; }

        /// <summary>
        /// 剩余清单内可报工程量
        /// </summary>
        public TitleDetails RemainingBillQuantityTitle { get; set; }

        /// <summary>
        /// 剩余现场实际工程量
        /// </summary>
        public TitleDetails RemainingActualQuantityTitle { get; set; }

        /// <summary>
        /// 当前清单备注项
        /// </summary>
        public TitleDetails ProjectRemarkTitle { get; set; }

        /// <summary>
        /// 进度款期数
        /// </summary>
        public TitleDetails FrequencyTitle { get; set; }

        /// <summary>
        /// 本期金额
        /// </summary>
        public TitleDetails FrequencyAmountTitle { get; set; }

        /// <summary>
        /// 本期工程量
        /// </summary>
        public TitleDetails FrequencyQuantityTitle { get; set; }

        /// <summary>
        /// 已报工程量
        /// </summary>
        public TitleDetails FrequencyReportedQuantityTitle { get; set; }

        /// <summary>
        /// 本期进度款备注
        /// </summary>
        public TitleDetails FrequencyRemarkTitle { get; set; }

        public PPRTitles()
        {
            ProjectIDTitle = new() { Title = "清单编号" };
            InventoryProjectNameTitle = new() { Title = "清单名称" };
            ProjectFeatureDescriptionTitle = new() { Title = "项目特征" };
            UnitTitle = new() { Title = "单位" };
            TotalAmountTitle = new() { Title = "单项目总金额" };
            CompletedQuantityTitle = new() { Title = "当前完成工程量" };
            ReportedQuantityTitle = new() { Title = "已报工程量" };
            BillOfQuantitiesQuantityTitle = new() { Title = "清单内工程量" };
            UnitPriceTitle = new() { Title = "单价" };
            RemainingBillQuantityTitle = new() { Title = "剩余清单内可报工程量" };
            RemainingActualQuantityTitle = new() { Title = "剩余现场实际工程量" };
            ProjectRemarkTitle = new() { Title = "当前清单备注" };
            FrequencyTitle = new() { Title = "进度款期数" };
            FrequencyAmountTitle = new() { Title = "本期金额" };
            FrequencyQuantityTitle = new() { Title = "本期工程量" };
            FrequencyReportedQuantityTitle = new() { Title = "本期已报工程量" };
            FrequencyRemarkTitle = new() { Title = "本期进度款备注" };
        }
    }
}
