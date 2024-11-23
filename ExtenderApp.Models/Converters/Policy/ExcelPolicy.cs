using ExtenderApp.Abstract;
using ExtenderApp.Data;


namespace ExtenderApp.Models.Converters
{
    public abstract class ExcelPolicy<TReadData,TWriteData> : BaseModelConvertPolicy<TReadData, TWriteData> where TReadData : class, new() where TWriteData : class, new()
    {
        private FileExtensionType extensionType = FileExtensionType.Xls + FileExtensionType.Xlsx;
        public override FileExtensionType ExtensionType => extensionType;

        protected override void ConvertToFile(IModel model, FileInfoData infoData, TWriteData writeData, object fileData)
        {
            throw new NotImplementedException();
        }

        protected override void ConvertToModel(IModel model, FileInfoData infoData, TReadData readData, object fileData)
        {
            var excelTable = fileData as ExcelTable;

            if (excelTable == null) throw new ArgumentNullException(nameof(excelTable));

            ToModel(model, infoData, readData, excelTable!);
        }

        /// <summary>
        /// 将数据转换为模型对象的抽象方法。
        /// </summary>
        /// <param name="model">模型对象。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="readData">读取数据。</param>
        /// <param name="excelTable">Excel表格。</param>
        protected abstract void ToModel(IModel model, FileInfoData infoData, TReadData readData, ExcelTable excelTable);

        /// <summary>
        /// 将模型对象写入文件的抽象方法。
        /// </summary>
        /// <param name="model">模型对象。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="writeData">写入数据（此处参数名可能存在误导，因为通常用于读取操作，但在此处是写入操作的上下文数据）。</param>
        /// <param name="excelTable">Excel表格。</param>
        protected abstract void ToFile(IModel model, FileInfoData infoData, TWriteData writeData, ExcelTable excelTable);
    }
}
