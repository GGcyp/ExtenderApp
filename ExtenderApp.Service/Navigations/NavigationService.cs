using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

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

        private readonly IPluginService _pluginService;

        private readonly ILogingService _logingService;

        /// <summary>
        /// 导航服务构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供程序</param>
        /// <param name="scopeExecutor">作用域执行器</param>
        public NavigationService(IServiceProvider serviceProvider, ILogingService logingService, IPluginService pluginService)
        {
            _serviceProvider = serviceProvider;
            _logingService = logingService;
            _pluginService = pluginService;
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
                newView = GetView(targetViewType, scope);
            }
            catch (Exception ex)
            {
                _logingService.Error(string.Format("导航到视图时发生错误，目标视图类型：{0}，作用域：{1}，错误信息：{2}", targetViewType.Name, scope, ex.Message), nameof(INavigationService), ex);
                return null;
            }

            oldView?.Exit(newView!.ViewInfo);
            newView!.Enter(oldView is null ? default : oldView.ViewInfo);

            return newView;
        }

        public IWindow NavigateToWindow(Type targetViewType, string scope, IView? oldView)
        {
            IWindow? window = GetWindow(scope);

            IView? newView = GetView(targetViewType, scope);

            ArgumentNullException.ThrowIfNull(window, "没有找到要转换的窗口");
            ArgumentNullException.ThrowIfNull(newView, string.Format("没有找到要转换的视图：{0}", targetViewType.Name));

            window.ShowView(newView);
            newView.InjectWindow(window);

            oldView?.Exit(newView.ViewInfo);
            newView.Enter(oldView is null ? default : oldView.ViewInfo);

            return window;
        }

        private IWindow? GetWindow(string? scope)
        {
            IWindow? newWindow = string.IsNullOrEmpty(scope) ?
                _serviceProvider.GetRequiredService<IWindow>()
                : _pluginService.GetPluginServiceProvider(scope)?.GetRequiredService<IWindow>();

            return newWindow ?? _serviceProvider.GetRequiredService<IWindow>();
        }

        private IView? GetView(Type targetViewType, string? scope)
        {
            IView? newView = string.IsNullOrEmpty(scope) ?
                _serviceProvider.GetRequiredService(targetViewType) as IView
                : _pluginService.GetPluginServiceProvider(scope)?.GetRequiredService(targetViewType) as IView;

            return newView ?? _serviceProvider.GetRequiredService(targetViewType) as IView;
        }
    }
}