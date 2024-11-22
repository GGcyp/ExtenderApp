using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Common.File;

namespace ExtenderApp.Models.Converters
{
    /// <summary>
    /// 模型转换器执行器类
    /// </summary>
    /// <typeparam name="TModel">模型类型</typeparam>
    public class ModelConverterExecutor<TModel> : IModelConverterExecutor
    {
        /// <summary>
        /// 模型转换策略存储对象
        /// </summary>
        private readonly IModelConvertPolicyStore _converterPolicyStore;

        /// <summary>
        /// 文件访问提供程序对象
        /// </summary>
        private readonly IFileParserProvider _fileAccessProvider;

        /// <summary>
        /// 初始化模型转换器执行器对象
        /// </summary>
        /// <param name="converterPolicyStore">模型转换策略存储对象</param>
        /// <param name="fileAccessProvider">文件访问提供程序对象</param>
        public ModelConverterExecutor(IModelConvertPolicyStore<TModel> converterPolicyStore, IFileParserProvider fileAccessProvider)
        {
            _converterPolicyStore = converterPolicyStore;
            _fileAccessProvider = fileAccessProvider;
        }

        /// <summary>
        /// 执行模型转换操作
        /// </summary>
        /// <param name="model">待转换的模型对象</param>
        /// <param name="infoData">文件信息数据对象</param>
        public void Execute(IModel model, FileInfoData infoData)
        {
            var policy = _converterPolicyStore.Find(infoData.Extension);

            //var modelConverter = new ModelConverter(policy, model, infoData);

            //_fileAccessProvider.FileOperation(infoData, modelConverter.Convert);
        }
    }
}
