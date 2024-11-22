using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;

namespace ExtenderApp.Views
{
    internal class MainViewHostedService : BackgroundService
    {
        private readonly IMainWindow _mainWindow;
        private readonly IMainView _mainView;

        public MainViewHostedService(IMainWindow mainWindow, IMainView mainView)
        {
            _mainWindow = mainWindow;
            _mainView = mainView;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _mainWindow.View = _mainView;
            _mainWindow.Show();
            return Task.CompletedTask;
        }
    }
}
