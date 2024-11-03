using AppHost.Extensions.DependencyInjection;
using MainApp.Abstract;

namespace MainApp.Models.Converters.Extensions
{
    /// <summary>
    /// ModelConverterExecutor 的扩展类，提供添加模型转换器策略存储的方法。
    /// </summary>
    public static class ModelConverterExecutorExtensions
    {
        /// <summary>
        /// 为服务集合添加指定模型的模型转换器策略存储。
        /// </summary>
        /// <typeparam name="TModel">需要转换的模型类型，需要实现 IModel 接口。</typeparam>
        /// <param name="services">当前的服务集合。</param>
        /// <param name="action">对模型转换器策略存储进行配置的 Action 委托。</param>
        /// <returns>返回配置后的服务集合。</returns>
        public static IServiceCollection AddModelConverterExecutor<TModel>(this IServiceCollection services, Action<IModelConvertPolicyStore> action) where TModel : IModel
        {
            services.AddModelConvertPolicyStore<TModel>(action);
            services.AddTransient<ModelConverterExecutor<TModel>>();
            return services;
        }
    }
}
