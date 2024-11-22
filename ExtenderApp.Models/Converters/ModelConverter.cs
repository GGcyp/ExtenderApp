using ExtenderApp.Abstract;
using ExtenderApp.Common;

namespace ExtenderApp.Models.Converters
{
    /// <summary>
    /// 模型转换器类，实现了IModelConverter接口。
    /// </summary>
    internal class ModelConverter
    {
        /// <summary>
        /// 转换策略接口实例。
        /// </summary>
        private IModelConvertPolicy convertPolicy;

        /// <summary>
        /// 模型实例。
        /// </summary>
        private IModel model;

        /// <summary>
        /// 文件信息数据实例。
        /// </summary>
        private FileInfoData infoData;

        /// <summary>
        /// 读写数据的对象，可以是任何类型。
        /// </summary>
        private object readOrWriteData;

        public ModelConverter(IModelConvertPolicy convertPolicy, IModel model, FileInfoData infoData)
        {
            this.convertPolicy = convertPolicy;
            this.model = model;
            this.infoData = infoData;
            readOrWriteData = convertPolicy.CreateReadOrWriteData(infoData.FileAccess);
        }

        /// <summary>
        /// 开始转换模型。
        /// </summary>
        /// <param name="objects">转换模型需要的数据。</param>
        public void Convert(object objects)
        {
            convertPolicy?.Convert(model, infoData, readOrWriteData, objects);
        }
    }
}
