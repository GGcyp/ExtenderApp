using MainApp.Abstract;

namespace MainApp.Models.Converters
{
    /// <summary>
    /// 模型转换策略存储工厂的静态内部类
    /// </summary>
    internal static class ModelConvertPolicyStoreFactory
    {
        /// <summary>
        /// 创建一个模型转换策略存储的实例
        /// </summary>
        /// <typeparam name="TModel">模型类型</typeparam>
        /// <returns>模型转换策略存储的实例</returns>
        public static IModelConvertPolicyStore<TModel> CreateStore<TModel>()
        {
            return new ModelConvertPolicyStore<TModel>();
        }
    }
}
