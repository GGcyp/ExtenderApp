using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        /// 日志记录器
        /// </summary>
        private readonly ILogger<INavigationService> _logger;

        /// <summary>
        /// 作用域存储
        /// </summary>
        private readonly IServiceScopeStore _serviceScopeStore;

        /// <summary>
        /// 导航服务构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供程序</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="pluginService">插件服务</param>
        /// <param name="serviceScopeStore">服务作用域存储</param>
        public NavigationService(IServiceProvider serviceProvider, ILogger<INavigationService> logger, IPluginService pluginService, IServiceScopeStore serviceScopeStore)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _serviceScopeStore = serviceScopeStore;
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
                _logger.LogError(ex, "导航到视图时发生错误，目标视图类型：{0}，作用域：{1}，错误信息：{2}", targetViewType.Name, scope, ex.Message);
                return null!;
            }

            //oldView?.Exit(newView!.ViewInfo);
            //newView!.Enter(oldView is null ? default : oldView.ViewInfo);

            return newView;
        }

        public IWindow NavigateToWindow(Type targetViewType, string scope, IView? oldView)
        {
            IWindow? window = GetWindow(scope);

            IView? newView = GetView(targetViewType, scope);

            ArgumentNullException.ThrowIfNull(window, "没有找到要转换的窗口");
            ArgumentNullException.ThrowIfNull(newView, string.Format("没有找到要转换的视图：{0}", targetViewType.Name));

            //window.ShowView(newView);
            //newView.InjectWindow(window);

            //oldView?.Exit(newView.ViewInfo);
            //newView.Enter(oldView is null ? default : oldView.ViewInfo);

            return window;
        }

        private IWindow? GetWindow(string? scope)
        {
            IWindow? newWindow = string.IsNullOrEmpty(scope) ?
                _serviceProvider.GetRequiredService<IWindow>()
                : _serviceScopeStore.Get(scope)?.GetRequiredService<IWindow>();

            return newWindow ?? _serviceProvider.GetRequiredService<IWindow>();
        }

        private IView? GetView(Type targetViewType, string? scope)
        {
            IView? newView = string.IsNullOrEmpty(scope) ?
                _serviceProvider.GetRequiredService(targetViewType) as IView
                : _serviceScopeStore.Get(scope)?.GetRequiredService(targetViewType) as IView;

            return newView ?? _serviceProvider.GetRequiredService(targetViewType) as IView;
        }
    }
}