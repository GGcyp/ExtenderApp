using ExtenderApp.Common;

namespace ExtenderApp.Abstract
{
    public interface IModelConverterExecutor
    {
        /// <summary>
        /// 执行模型转换操作
        /// </summary>
        /// <param name="model">待转换的模型对象</param>
        /// <param name="infoData">文件信息数据对象</param>
        void Execute(IModel model, FileInfoData infoData);
    }
}
