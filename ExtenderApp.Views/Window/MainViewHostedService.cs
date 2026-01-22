using ExtenderApp.Abstract;
using ExtenderApp.Common;

namespace ExtenderApp.Views
{
    /// <summary>
    /// 主视图托管后台服务。 负责在应用启动后创建主窗口、导航到主视图并显示窗口。
    /// </summary>
    /// <remarks>
    /// - 本服务通常在宿主启动阶段由托管容器启动。 <br/>
    /// - WPF 窗口与视图应在 UI（STA/Dispatcher）线程上创建和显示；
    /// 若宿主在后台线程调用 <see
    /// cref="ExecuteAsync(CancellationToken)"/>，
    /// 需确保外部已切换到 UI 线程或在此处显式调度到 UI 线程后再进行 UI 操作。
    /// </remarks>
    internal class MainViewHostedService : StartupExecute
    {
        private readonly IMainWindowService _mainWindowService;

        /// <summary>
        /// 使用所需服务初始化 <see
        /// cref="MainViewHostedService"/> 的新实例。
        /// </summary>
        /// <param name="mainWindowService">主窗口服务实例。</param>
        /// <param name="navigationService">导航服务实例。</param>
        public MainViewHostedService(IMainWindowService mainWindowService)
        {
            _mainWindowService = mainWindowService;
        }

        public override ValueTask ExecuteAsync()
        {
            var mainWindow = _mainWindowService.CreateMainWindow();
            mainWindow.Show();
            return ValueTask.CompletedTask;
        }
    }
}