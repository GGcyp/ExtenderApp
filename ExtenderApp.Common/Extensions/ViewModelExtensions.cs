using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 为 ViewModel 注册提供扩展方法的静态类。
    /// </summary>
    public static class ViewModelExtensions
    {
        /// <summary>
        /// 使用容器创建并初始化一个视图模型实例（内部辅助方法）。
        /// 此方法通过 <see cref="ActivatorUtilities.CreateInstance{T}"/> 创建实例，
        /// 然后调用 <see cref="IViewModel.Inject(IServiceProvider)"/> 注入当前 <see cref="IServiceProvider"/>。
        /// </summary>
        /// <typeparam name="T">要创建的视图模型类型，必须实现 <see cref="IViewModel"/>。</typeparam>
        /// <param name="serviceProvider">用于解析依赖项的服务提供者。</param>
        /// <returns>已创建并注入服务提供者的视图模型实例。</returns>
        private static T CreatViewModel<T>(IServiceProvider serviceProvider) where T : class, IViewModel
        {
            var viewModel = ActivatorUtilities.CreateInstance<T>(serviceProvider);
            viewModel.Inject(serviceProvider);
            return viewModel;
        }

        /// <summary>
        /// 将指定的视图模型类型注册到 <see cref="IServiceCollection"/> 中。
        /// 注册时使用工厂委托创建实例（通过 <see cref="CreatViewModel{T}"/>），
        /// 并允许指定生命周期（默认为 <see cref="ServiceLifetime.Transient"/>）。
        /// </summary>
        /// <typeparam name="TViewModel">要注册的视图模型类型，必须实现 <see cref="IViewModel"/>。</typeparam>
        /// <param name="services">要添加注册的 <see cref="IServiceCollection"/> 实例。</param>
        /// <param name="lifetime">注册生命周期，默认瞬态（Transient）。</param>
        /// <returns>返回传入的 <see cref="IServiceCollection"/> 以便链式调用。</returns>
        public static IServiceCollection AddViewModel<TViewModel>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TViewModel : class, IViewModel
        {
            switch (lifetime)
            {
                case ServiceLifetime.Transient:
                    services.AddTransient(CreatViewModel<TViewModel>);
                    break;

                case ServiceLifetime.Scoped:
                    services.AddScoped(CreatViewModel<TViewModel>);
                    break;

                case ServiceLifetime.Singleton:
                    services.AddSingleton(CreatViewModel<TViewModel>);
                    break;
            }
            return services;
        }
    }
}