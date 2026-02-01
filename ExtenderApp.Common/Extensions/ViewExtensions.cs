using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 为视图与视图模型注册提供扩展方法。
    /// </summary>
    public static class ViewExtensions
    {
        /// <summary>
        /// 将指定的视图类型 <typeparamref name="TView"/> 与视图模型类型 <typeparamref name="TViewModel"/> 一起注册为瞬态（Transient）。在解析视图实例时会自动从容器解析对应的视图模型并注入到视图中。
        /// </summary>
        /// <typeparam name="TView">要注册的视图类型，必须实现 <see cref="IView"/>。</typeparam>
        /// <typeparam name="TViewModel">要注册的视图模型类型，必须实现 <see cref="IViewModel"/>。</typeparam>
        /// <param name="services">要添加注册的 <see cref="IServiceCollection"/> 实例。</param>
        /// <returns>返回传入的 <see cref="IServiceCollection"/> 以便链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="services"/> 为 <c>null</c> 时抛出。</exception>
        public static IServiceCollection AddView<TView, TViewModel>(this IServiceCollection services)
            where TView : class, IView
            where TViewModel : class, IViewModel
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddTransient(p =>
            {
                var view = ActivatorUtilities.CreateInstance<TView>(p);
                var viewModel = p.GetRequiredService<TViewModel>();
                view.Inject(viewModel);
                return view;
            });
            return services;
        }

        /// <summary>
        /// 将视图接口 <typeparamref name="TIView"/>、具体视图实现 <typeparamref name="TView"/> 与视图模型类型 <typeparamref name="TViewModel"/>
        /// 按照接口映射注册为瞬态（Transient）。此方法会先注册具体视图类型 <typeparamref name="TView"/>，随后将接口解析绑定到具体视图实例并注入对应视图模型。
        /// </summary>
        /// <typeparam name="TIView">视图接口类型，必须实现 <see cref="IView"/>。</typeparam>
        /// <typeparam name="TView">具体视图实现类型，应继承或实现 <typeparamref name="TIView"/>。</typeparam>
        /// <typeparam name="TViewModel">视图模型类型，必须实现 <see cref="IViewModel"/>。</typeparam>
        /// <param name="services">要添加注册的 <see cref="IServiceCollection"/> 实例。</param>
        /// <returns>返回传入的 <see cref="IServiceCollection"/> 以便链式调用。</returns>
        /// <remarks>此扩展方法适用于你希望通过接口（ <typeparamref name="TIView"/>）解析视图实例时， 实际返回由容器创建的具体类型 <typeparamref name="TView"/> 的场景。</remarks>
        /// <exception cref="ArgumentNullException">当 <paramref name="services"/> 为 <c>null</c> 时抛出。</exception>
        public static IServiceCollection AddView<TIView, TView, TViewModel>(this IServiceCollection services)
            where TIView : class, IView
            where TView : class, TIView
            where TViewModel : class, IViewModel
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddTransient<TView>();

            services.AddTransient<TIView>(p =>
            {
                var view = p.GetRequiredService<TView>();
                var viewModel = p.GetRequiredService<TViewModel>();
                view.Inject(viewModel);
                return view;
            });
            return services;
        }
    }
}