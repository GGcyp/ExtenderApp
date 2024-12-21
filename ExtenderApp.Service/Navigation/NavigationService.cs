using ExtenderApp.Abstract;

namespace ExtenderApp.Service
{
    internal class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogingService _loggingService;

        public NavigationService(IServiceProvider serviceProvider, ILogingService logService)
        {
            _serviceProvider = serviceProvider;
            _loggingService = logService;
        }

        public IView NavigateTo(Type targetViewType, IView oldView)
        {
            IView newView = _serviceProvider.GetService(targetViewType) as IView;
            if (newView is null)
            {
                _loggingService.Error(string.Format("无法切换到视图从{0}切换到视图{1}", oldView.ViewInfo.ViewName, targetViewType?.Name), nameof(INavigationService), new ArgumentNullException());
                //ArgumentNullException.ThrowIfNull(newView, string.Format("not found the view {0}", targetViewType.Name));
                return newView;
            }


            oldView?.Exit(newView.ViewInfo);
            newView.Enter(oldView is null ? default : oldView.ViewInfo);

            _loggingService.Info(string.Format("成功切换到视图{0}", targetViewType.Name), nameof(INavigationService));

            return newView;
        }
    }
}
