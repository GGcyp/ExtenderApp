using MainApp.ViewModels;
using PropertyChanged;

namespace MainApp.Mod.PPR
{
    /// <summary>
    /// PPRPeriodQuantityDot 类表示一个与 PPRPeriodQuantityEntity周期数量相关的数据点。
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class PPRPeriodQuantityDto : BaseDto<PPRPeriodQuantityEntity>
    {
        /// <summary>
        /// 每期最后修改时间
        /// </summary>
        public string Time
        {
            get => Entity.Time;
            set => Entity.Time = value;
        }

        /// <summary>
        /// 期数
        /// </summary>
        public int Frequency
        {
            get => Entity.Frequency;
            set => Entity.Frequency = value;
        }

        /// <summary>
        /// 本期金额
        /// </summary>
        public double FrequencyAmount
        {
            get => Entity.FrequencyAmount;
            set => Entity.FrequencyAmount = value;
        }

        /// <summary>
        /// 本期工程量
        /// </summary>
        public double FrequencyQuantity
        {
            get => Entity.FrequencyQuantity;
            set => Entity.FrequencyQuantity = value;
        }

        /// <summary>
        /// 本期已报工程量
        /// </summary>
        public double FrequencyReportedQuantity
        {
            get => Entity.FrequencyReportedQuantity;
            set => Entity.FrequencyReportedQuantity = value;
        }

        /// <summary>
        /// 本期备注
        /// </summary>
        public string FrequencyRemark
        {
            get => Entity.FrequencyRemark;
            set => Entity.FrequencyRemark = value;
        }
    }
}
