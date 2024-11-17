using AppHost.Extensions.Hosting;
using MainApp.Abstract;

namespace MainApp.Views
{
    internal class MainViewHostedService : BackgroundService
    {
        private readonly IMainWindow _mainWindow;

        public MainViewHostedService(IMainWindow mainView)
        {
            _mainWindow = mainView;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _mainWindow.Show();
            return Task.CompletedTask;
        }
    }
}
