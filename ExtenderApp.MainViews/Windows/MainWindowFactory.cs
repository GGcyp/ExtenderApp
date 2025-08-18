using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.MainViews.ViewModels;

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
            var viewModel = _serviceProvider.GetRequiredService<MianWindowViewModel>();
            return new MainViewWindow(viewModel);
        }
    }
}
