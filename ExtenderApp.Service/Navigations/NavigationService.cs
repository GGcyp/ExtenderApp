using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;


namespace ExtenderApp.Services
{
    /// <summary>
    /// 导航服务类，实现了 INavigationService 接口
    /// </summary>
    internal class NavigationService : INavigationService
    {
        /// <summary>
        /// 服务提供程序
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 作用域执行器
        /// </summary>
        private readonly IScopeExecutor _scopeExecutor;

        private readonly ILogingService _logingService;

        /// <summary>
        /// 导航服务构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供程序</param>
        /// <param name="scopeExecutor">作用域执行器</param>
        public NavigationService(IServiceProvider serviceProvider, IScopeExecutor scopeExecutor, ILogingService logingService)
        {
            _serviceProvider = serviceProvider;
            _scopeExecutor = scopeExecutor;
            _logingService = logingService;
        }

        /// <summary>
        /// 导航到指定的视图类型
        /// </summary>
        /// <param name="targetViewType">目标视图类型</param>
        /// <param name="scope">作用域</param>
        /// <param name="oldView">旧视图</param>
        /// <returns>新视图</returns>
        public IView NavigateTo(Type targetViewType, string scope, IView? oldView)
        {
            IView? newView = null;
            try
            {
                newView = string.IsNullOrEmpty(scope) ?
                    _serviceProvider.GetRequiredService(targetViewType) as IView
                    : _scopeExecutor.GetServiceProvider(scope)?.GetRequiredService(targetViewType) as IView;
            }
            catch (Exception ex)
            {
                _logingService.Error(string.Format("导航到视图时发生错误，目标视图类型：{0}，作用域：{1}，错误信息：{2}", targetViewType.Name, scope, ex.Message), nameof(INavigationService), ex);
                throw;
            }


            oldView?.Exit(newView.ViewInfo);
            newView.Enter(oldView is null ? default : oldView.ViewInfo);

            return newView;
        }

        public IWindow NavigateToWindow(Type targetViewType, string scope, IView? oldView)
        {
            IWindow window = string.IsNullOrEmpty(scope) ?
                _serviceProvider.GetRequiredService<IWindow>()!
                : _scopeExecutor.GetServiceProvider(scope)?.GetRequiredService<IWindow>()!;

            IView? newView = string.IsNullOrEmpty(scope) ?
                _serviceProvider.GetRequiredService(targetViewType) as IView
                : _scopeExecutor.GetServiceProvider(scope)?.GetRequiredService(targetViewType) as IView;

            ArgumentNullException.ThrowIfNull(newView, string.Format("没有找到要转换的视图：{0}", targetViewType.Name));

            window.ShowView(newView);
            newView.InjectWindow(window);

            oldView?.Exit(newView.ViewInfo);
            newView.Enter(oldView is null ? default : oldView.ViewInfo);

            return window;
        }
    }
}
