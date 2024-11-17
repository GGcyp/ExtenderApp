namespace MainApp.Mod.PPR
{
    /// <summary>
    /// 实体，工程记录头文件
    /// ProjectPaymentRecordData
    /// </summary>
    public class PPREntity
    {
        /// <summary>
        /// 当前工程名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 当前工程金额
        /// </summary>
        public double ProjectAmount {  get; set; }

        /// <summary>
        /// 当前工程所包含的所有清单项
        /// </summary>
        public List<PPRInventoryEntity>? DataList { get; set; }

        public PPREntity() : this(string.Empty, 0)
        {

        }

        public PPREntity(string projectName, double projectAmount)
        {
            ProjectName = projectName;
            ProjectAmount = projectAmount;
        }

        /// <summary>
        /// 计算数据
        /// </summary>
        public void CalculatePPRData()
        {
            //LoopChildNodes(CalculatePPRData);
        }

        /// <summary>
        /// 计算数据
        /// </summary>
        public void CalculatePPRData(PPREntity head)
        {
            if (head.DataList == null) return;

            for (int i = 0; i < head.DataList.Count; i++)
            {
                PPRInventoryEntity data = head.DataList[i];
                data.Calculate();
            }
        }
    }
}
