using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Services
{
    internal class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IScopeExecutor _scopeExecutor;

        public NavigationService(IServiceProvider serviceProvider, IScopeExecutor scopeExecutor)
        {
            _serviceProvider = serviceProvider;
            _scopeExecutor = scopeExecutor;
        }

        public IView NavigateTo(Type targetViewType, string scope, IView? oldView)
        {
            IView? newView = string.IsNullOrEmpty(scope) ?
                _serviceProvider.GetRequiredService(targetViewType) as IView
                : _scopeExecutor.GetServiceProvider(scope)?.GetRequiredService(targetViewType) as IView;

            ArgumentNullException.ThrowIfNull(newView, string.Format("没有找到要转换的视图：{0}", targetViewType.Name));

            oldView?.Exit(newView.ViewInfo);
            newView.Enter(oldView is null ? default : oldView.ViewInfo);

            return newView;
        }
    }
}
