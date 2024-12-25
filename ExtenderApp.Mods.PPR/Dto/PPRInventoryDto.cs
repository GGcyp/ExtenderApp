using ExtenderApp.Common.ObjectPools;
using ExtenderApp.ViewModels;
using PropertyChanged;

namespace ExtenderApp.Mod.PPR
{
    /// <summary>
    /// PPRInventoryEntity的数据交互类
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class PPRInventoryDto : BaseDto<PPRInventoryEntity>
    {
        private static ObjectPool<DtoList<PPRPeriodQuantityDto, PPRPeriodQuantityEntity>> pool =
            ObjectPool.Create<DtoList<PPRPeriodQuantityDto, PPRPeriodQuantityEntity>>();

        /// <summary>
        /// 清单ID
        /// </summary>
        public string ProjectID
        {
            get => Entity.ProjectID;
            set => Entity.ProjectID = value;
        }

        /// <summary>
        /// 单项工程名字
        /// </summary>
        public string InventoryProjectName
        {
            get => Entity.InventoryProjectName;
            set => Entity.InventoryProjectName = value;
        }

        /// <summary>
        /// 项目特征
        /// </summary>
        public string ProjectFeatureDescription
        {
            get => Entity.ProjectFeatureDescription;
            set => Entity.ProjectFeatureDescription = value;
        }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit
        {
            get => Entity.Unit;
            set => Entity.Unit = value;
        }

        /// <summary>
        /// 单项目总金额
        /// </summary>
        public double TotalAmount
        {
            get => Entity.TotalAmount;
            set => Entity.TotalAmount = value;
        }

        /// <summary>
        /// 当前完成工程量
        /// </summary>
        public double CompletedQuantity
        {
            get => Entity.CompletedQuantity;
            set => Entity.CompletedQuantity = value;
        }

        /// <summary>
        /// 已报工程量
        /// </summary>
        public double ReportedQuantity
        {
            get => Entity.ReportedQuantity;
            set => Entity.ReportedQuantity = value;
        }

        /// <summary>
        /// 清单内工程量
        /// </summary>
        public double BillOfQuantitiesQuantity
        {
            get => Entity.BillOfQuantitiesQuantity;
            set => Entity.BillOfQuantitiesQuantity = value;
        }

        /// <summary>
        /// 单价
        /// </summary>
        public double UnitPrice
        {
            get => Entity.UnitPrice;
            set => Entity.UnitPrice = value;
        }

        /// <summary>
        /// 剩余清单内可报工程量
        /// </summary>
        public double RemainingBillQuantity
        {
            get => Entity.RemainingBillQuantity;
            set => Entity.RemainingBillQuantity = value;
        }

        /// <summary>
        /// 剩余现场实际工程量
        /// </summary>
        public double RemainingActualQuantity
        {
            get => Entity.RemainingActualQuantity;
            set => Entity.RemainingActualQuantity = value;
        }

        /// <summary>
        /// 当前清单备注项
        /// </summary>
        public string ProjectRemark
        {
            get => Entity.ProjectRemark;
            set => Entity.ProjectRemark = value;
        }

        /// <summary>
        /// 清单内周期工程量
        /// </summary>
        public DtoList<PPRPeriodQuantityDto, PPRPeriodQuantityEntity> Periods { get; set; }

        protected override void OnEntityChanged()
        {
            if (Entity is null || Entity?.PeriodQuantityList is null)
            {
                if (Periods is null) return;
                Periods.Clear();
                pool.Release(Periods);
                Periods = null;
            }
            else
            {
                if (Periods is null)
                {
                    Periods = pool.Get();
                    Periods.UpdatePageSize(int.MaxValue);
                }
                Periods.Clear();
                Periods.AddEntitiesList(Entity.PeriodQuantityList);
                Periods.Refresh();
            }
        }
    }
}
