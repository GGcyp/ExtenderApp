using ExtenderApp.Abstract;

namespace ExtenderApp.Services
{
    internal class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IView NavigateTo(Type targetViewType, IView oldView)
        {
            IView newView = _serviceProvider.GetService(targetViewType) as IView;
            ArgumentNullException.ThrowIfNull(newView, string.Format("not found the view {0}", targetViewType.Name));

            oldView?.Exit(newView.ViewInfo);
            newView.Enter(oldView is null ? default : oldView.ViewInfo);

            return newView;
        }
    }
}
