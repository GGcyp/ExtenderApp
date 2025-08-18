

using ExtenderApp.Abstract;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 主窗口工厂接口，用于创建主窗口实例。
    /// </summary>
    internal class MainWindowService : IMainWindowService
    {
        /// <summary>
        /// 主窗口工厂实例，用于创建主窗口。
        /// </summary>
        private readonly IMainWindowFactory _mainWindowFactory;

        public IMainWindow? CurrentMainWindow { get; private set; }

        public MainWindowService(IMainWindowFactory mainWindowFactory)
        {
            _mainWindowFactory = mainWindowFactory;
        }

        public IMainWindow CreateMainWindow()
        {
            if (CurrentMainWindow != null)
                return CurrentMainWindow;

            CurrentMainWindow = _mainWindowFactory.CreateMainWindow();
            CurrentMainWindow.Closed += CurrentMainWindow_Closed;
            return CurrentMainWindow;
        }

        private void CurrentMainWindow_Closed(object? sender, EventArgs e)
        {
            CurrentMainWindow!.Closed -= CurrentMainWindow_Closed;
            CurrentMainWindow = null;
        }
    }
}
