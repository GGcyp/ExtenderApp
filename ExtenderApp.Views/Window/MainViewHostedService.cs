using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;

namespace ExtenderApp.Views
{
    internal class MainViewHostedService : BackgroundService
    {
        private readonly IMainWindow _mainWindow;
        private readonly INavigationService _navigationService;

        public MainViewHostedService(IMainWindow mainWindow, INavigationService service)
        {
            _mainWindow = mainWindow;
            _navigationService = service;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mainView = _navigationService.NavigateTo(typeof(IMainView), string.Empty, null) as IMainView;
            _mainWindow.ShowView(mainView);
            _mainWindow.Show();
            return Task.CompletedTask;
        }
    }
}
