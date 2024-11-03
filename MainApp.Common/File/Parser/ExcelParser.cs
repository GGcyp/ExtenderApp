using MainApp.Common.Data;
using OfficeOpenXml;

namespace MainApp.Common.File
{
    internal class ExcelParser : FileParser
    {
        private FileExtensionType extensionType;
        public override FileExtensionType ExtensionType => extensionType;

        public ExcelParser() : base(null)
        {
            extensionType = FileExtensionType.Xlsx + FileExtensionType.Xls;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        #region Read

        protected override void Read(FileInfoData infoData, Action<object> processAction)
        {
            if (!infoData.Exists)
            {
                throw new ArgumentNullException($"没有找到这个文件,{infoData.FileName} : {infoData.Path} ");
            }

            ExcelTable table = new ExcelTable();
            using (var package = new ExcelPackage(infoData.Info))
            {
                ExcelWorksheets worksheets = package.Workbook.Worksheets;

                foreach(var sheet in worksheets)
                {
                    table.Add(sheet.Name, ReadToExcelTable(sheet));
                }
            }
            processAction?.Invoke(table);
        }

        private Table<string> ReadToExcelTable(ExcelWorksheet worksheet)
        {
            ExcelAddressBase dimension = worksheet.Dimension;

            int startRow = dimension.Start.Row;
            int startColumn = dimension.Start.Column;
            int endRow = dimension.End.Row;
            int endColumn = dimension.End.Column;

            Table<string> table = new Table<string>(endRow - startRow);
            for (int row = startRow; row <= endRow; row++)
            {
                TableRow<string> tableRow = new TableRow<string>(endColumn);
                for (int col = startColumn; col <= endColumn; col++)
                {
                    var cellValue = worksheet.Cells[row, col].Text;
                    tableRow.Add(cellValue);
                }
                table.Add(tableRow);
            }
            return table;
        }

        #endregion

        protected override void Write(FileInfoData infoData, Action<object> processAction)
        {
            // 写入Excel文件
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells[1, 1].Value = "Hello, EPPlus!";

                // 保存文件
                package.SaveAs(infoData.Info);
            }
        }
    }
}
