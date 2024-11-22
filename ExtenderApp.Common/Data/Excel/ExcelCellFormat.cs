using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ExtenderApp.Common.Data
{
    public struct ExcelCellFormat
    {
        /// <summary>
        /// 数字格式字符串。
        /// </summary>
        public ExcelNumberFormat numberFormat;
        /// <summary>
        /// 字体颜色。
        /// </summary>
        public Color fontColor;
        /// <summary>
        /// 是否使用粗体。
        /// </summary>
        public bool bold;
        /// <summary>
        /// 是否使用斜体。
        /// </summary>
        public bool italic;
        /// <summary>
        /// 填充颜色。
        /// </summary>
        public Color fillColor;
        /// <summary>
        /// 边框样式。
        /// </summary>
        public ExcelBorderStyle borderStyle;

        public ExcelCellFormat(ExcelNumberFormat numberFormat = null, Color fontColor = default, bool bold = false, bool italic = false, Color fillColor = default, ExcelBorderStyle borderStyle = ExcelBorderStyle.None)
        {
            this.numberFormat = numberFormat;
            this.fontColor = fontColor.IsEmpty ? Color.Black : fontColor; // 默认黑色
            this.bold = bold;
            this.italic = italic;
            this.fillColor = fillColor.IsEmpty ? Color.White : fillColor; // 默认白色
            this.borderStyle = borderStyle;
        }

        public void SettingStyles(ExcelRange excelRange)
        {
            excelRange.Style.Numberformat = numberFormat;
        }
    }
}
