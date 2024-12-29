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
            _mainWindow.ShowView(_navigationService.NavigateTo(typeof(IMainView), null));
            _mainWindow.Show();
            return Task.CompletedTask;
        }
    }
}
