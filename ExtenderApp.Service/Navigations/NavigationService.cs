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

        public event EventHandler? CurrentViewChanged;

        public IView? CurrentView { get; private set; }

        /// <summary>
        /// 导航服务构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供程序</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="pluginService">插件服务</param>
        /// <param name="serviceScopeStore">服务作用域存储</param>
        public NavigationService(IServiceProvider serviceProvider, ILogger<INavigationService> logger, IServiceScopeStore serviceScopeStore)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _serviceScopeStore = serviceScopeStore;
        }

        public void NavigateTo(Type view, string scope)
        {
            CurrentView = GetView(view, scope);
            CurrentViewChanged?.Invoke(this, EventArgs.Empty);
        }

        private IView? GetView(Type viewType, string? scope)
        {
            IView? newView = string.IsNullOrEmpty(scope) ?
                _serviceProvider.GetRequiredService(viewType) as IView
                : _serviceScopeStore.Get(scope)?.GetRequiredService(viewType) as IView;

            return newView ?? _serviceProvider.GetRequiredService(viewType) as IView;
        }
    }
}