using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Models.Converters
{
    /// <summary>
    /// 提供针对<see cref="IModelConvertPolicyStore"/>接口的扩展方法。
    /// </summary>
    public static class ModelConvertPolicyStoreExtensions
    {
        /// <summary>
        /// 为服务集合添加<see cref="IModelConvertPolicyStore"/>的实例，并允许配置它。
        /// </summary>
        /// <typeparam name="TModel">实现<see cref="IModel"/>接口的模型类型。</typeparam>
        /// <param name="services">要添加服务的服务集合。</param>
        /// <param name="action">一个用于配置<see cref="IModelConvertPolicyStore"/>的委托。</param>
        /// <returns>返回配置后的服务集合。</returns>
        public static IServiceCollection AddModelConvertPolicyStore<TModel>(this IServiceCollection services, Action<IModelConvertPolicyStore> action) where TModel : IModel
        {
            var policyStore = ModelConvertPolicyStoreFactory.CreateStore<TModel>();
            action?.Invoke(policyStore);
            services.AddSingleton<IModelConvertPolicyStore<TModel>>(policyStore);
            return services;
        }

        /// <summary>
        /// 向模型转换器策略存储中添加新的模型转换器策略<typeparamref name="TPolicy"/>。
        /// </summary>
        /// <typeparam name="TPolicy">模型转换器策略类型，需要实现<see cref="IModelConvertPolicy"/>接口并且拥有无参构造函数。</typeparam>
        /// <param name="store">模型转换器策略存储实例。</param>
        /// <returns>添加策略后的模型转换器策略存储实例。</returns>
        /// <exception cref="ArgumentNullException">如果<paramref name="store"/>为null，则抛出此异常。</exception>
        public static IModelConvertPolicyStore AddModelConvertPolicy<TPolicy>(this IModelConvertPolicyStore store) where TPolicy : IModelConvertPolicy, new()
        {
            if (store == null) throw new ArgumentNullException("store");
            if (typeof(TPolicy).IsAbstract) throw new ArgumentNullException(typeof(TPolicy).FullName);
            store.Add(new TPolicy());
            return store;
        }
    }
}
