using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 主窗口工厂接口，用于创建主窗口实例。
    /// </summary>
    internal class MainWindowService : IMainWindowService
    {
        private readonly IServiceProvider _serviceProvider;

        public IMainWindow? CurrentMainWindow { get; private set; }

        public MainWindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IMainWindow CreateMainWindow()
        {
            if (CurrentMainWindow != null)
                return CurrentMainWindow;

            CurrentMainWindow = _serviceProvider.GetRequiredService<IMainWindow>();
            CurrentMainWindow.Closed += CurrentMainWindow_Closed;
            return CurrentMainWindow;
        }

        private void CurrentMainWindow_Closed(object? sender, EventArgs e)
        {
            CurrentMainWindow = null;
        }
    }
}