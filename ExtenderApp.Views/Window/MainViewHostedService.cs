using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;

namespace ExtenderApp.Views
{
    internal class MainViewHostedService : BackgroundService
    {
        private readonly IMainWindowService _mainWindowService;
        private readonly INavigationService _navigationService;

        public MainViewHostedService(IMainWindowService mainWindowService, INavigationService navigationService)
        {
            _mainWindowService = mainWindowService;
            _navigationService = navigationService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mainWindow = _mainWindowService.CreateMainWindow();
            var mainView = _navigationService.NavigateTo(typeof(IMainView), string.Empty, null) as IMainView;
            mainWindow.ShowView(mainView);
            mainView.InjectWindow(mainWindow);
            mainWindow.Show();
            return Task.CompletedTask;
        }
    }
}
