using AppHost.Extensions.Hosting;
using MainApp.Abstract;

namespace MainApp.Views
{
    internal class MainViewHostedService : BackgroundService
    {
        private readonly IMainView _mainView;

        public MainViewHostedService(IMainView mainView)
        {
            _mainView = mainView;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _mainView.Show();
            return Task.CompletedTask;
        }
    }
}
