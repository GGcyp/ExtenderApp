using MainApp.Abstract;
using MainApp.Common;
using MainApp.Common.Data;
using MainApp.Models;
using MainApp.Models.Converters;

namespace MainApp.Mods.PPR
{
    internal class PPRExcelPolicy : ExcelPolicy<object,object>
    {
        private const int JumpIndexCount = 6;

        protected override object CreateReadData() => null;

        protected override object CreateWriteData() => null;

        protected override void ToModel(IModel model, FileInfoData infoData, object readData, ExcelTable excelTable)
        {
            PPREntityNode root = new ();
            for (int i = 0; i < excelTable.Count; i++)
            {
                TableToPPREntity(excelTable[i].Item2, root);
            }
            model.AddDataSource(root);
        }

        protected override void ToFile(IModel model, FileInfoData infoData, object writeData, ExcelTable excelTable)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 将表格转换为PPREntity对象
        /// </summary>
        /// <param name="table">表格数据</param>
        /// <param name="root">根PPREntity对象</param>
        private void TableToPPREntity(Table<string> table, PPREntityNode root)
        {
            if (table[1][1] != "分部分项工程和单价措施项目清单与计价表")
            {
                throw new Exception("分部分项工程和单价措施项目清单与计价表");
            }

            int row = table.rowCount;
            //整个项目名称，金额
            root.Entity.ProjectName = table[2][1].Split('：')[1];
            root.Entity.ProjectAmount = Double.Parse(table[5][10]);

            PPREntity entity = null;
            //从5开始是正式内容
            for (int i = 6; i < row; i++)
            {
                TableRow<string> tableRow = table[i];
                string serialNumbermp = tableRow[1];
                //判断是否开始工程量清单的信息
                if (string.IsNullOrEmpty(serialNumbermp))
                {
                    //3是工程名称，10是金额
                    string proName = tableRow[3];
                    string proAmount = tableRow[10];
                    if (!string.IsNullOrEmpty(proName) && !string.IsNullOrEmpty(proAmount))
                    {
                        entity = new PPREntity(proName, Double.Parse(proAmount));
                        root.Add(entity);
                    }
                }
                else
                {
                    //拿到工程信息
                    if (double.TryParse(serialNumbermp, out double value))
                    {
                        if (entity.DataList == null)
                        {
                            entity.DataList = new ();
                        }

                        var list = entity.DataList;
                        string projectFeature = tableRow[4];
                        list.Add(new PPRInventoryEntity()
                        {
                            ProjectID = tableRow[2],
                            InventoryProjectName = GetProjectResidue(table, 3, i, row),
                            ProjectFeatureDescription = GetProjectResidue(table, 4, i, row),
                            Unit = tableRow[6],
                            BillOfQuantitiesQuantity = double.Parse(tableRow[7]),
                            UnitPrice = double.Parse(tableRow[8]),
                            TotalAmount = double.Parse(tableRow[10]),
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 获取项目残留信息
        /// </summary>
        /// <param name="table">表格数据</param>
        /// <param name="rowIndex">行索引</param>
        /// <param name="index">当前索引</param>
        /// <param name="maxIndex">最大索引</param>
        /// <returns>项目残留信息字符串</returns>
        private string GetProjectResidue(Table<string> table, int rowIndex, int index, int maxIndex)
        {
            TableRow<string> tableRow = table[index];
            string projectResidue = tableRow[rowIndex];
            if (index + JumpIndexCount > maxIndex) return projectResidue;

            tableRow = table[index + JumpIndexCount];
            if (!CheckRowResidue(tableRow)) return projectResidue;
            projectResidue += tableRow[rowIndex];
            return projectResidue;
        }

        /// <summary>
        /// 检查行是否包含残留信息
        /// </summary>
        /// <param name="tableRow">表格行数据</param>
        /// <returns>如果行包含残留信息，则返回true；否则返回false</returns>
        private bool CheckRowResidue(TableRow<string> tableRow)
        {
            return string.IsNullOrEmpty(tableRow[1]) && string.IsNullOrEmpty(tableRow[2]);
        }
    }
}
