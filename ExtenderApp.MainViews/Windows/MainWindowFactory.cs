using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.MainViews.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.MainViews.Windows
{
    internal class MainWindowFactory : IMainWindowFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MainWindowFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IMainWindow CreateMainWindow()
        {
            var viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            return new MainViewWindow(viewModel);
        }
    }
}
