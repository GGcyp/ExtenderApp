using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using ExtenderApp.Abstract;
using ExtenderApp.MainViews.Models;
using ExtenderApp.MainViews.Views;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.CutsceneViews;

namespace ExtenderApp.MainViews.ViewModels
{
    /// <summary>
    /// 主运行视图的视图模型，负责处理与主运行视图相关的逻辑和数据绑定。
    /// </summary>
    public class MainView_RunViewModel : ExtenderAppViewModel<MainView_RunView, MainModel>
    {
        private IMainViewSettings? currentMainViewSettings;

        /// <summary>
        /// 命令属性，用于绑定到视图中的按钮，当按钮被点击时，将执行 ToMainView 方法。
        /// </summary>
        public RelayCommand ToMainViewCommand { get; set; }

        /// <summary>
        /// 打开设置视图的命令属性。
        /// </summary>
        public RelayCommand OpenSettingsWindowCommand { get; set; }

        private int lastButtonHeight;
        public int ButtonHeight { get; set; }

        /// <summary>
        /// 构造函数，初始化视图模型并设置依赖注入的实例。
        /// </summary>
        /// <param name="model">MainModel 实例，提供视图模型所需的数据。</param>
        /// <param name="scope">IScopeExecutor 实例，用于执行与插件作用域相关的操作。</param>
        /// <param name="serviceStore">IServiceStore 实例，提供应用程序所需的服务。</param>
        public MainView_RunViewModel(MainModel model, IServiceStore serviceStore) : base(model, serviceStore)
        {
            ToMainViewCommand = new(ToMainView);
            OpenSettingsWindowCommand = new(OpenSettingsWindow);

            ButtonHeight = 40;

            var details = Model.CurrentPluginDetails;
        }

        /// <summary>
        /// 打开设置窗口的方法，显示当前插件的设置视图。
        /// </summary>
        private void OpenSettingsWindow()
        {
            if (currentMainViewSettings == null)
                return;

            var settingWindow = NavigateToWindow<SettingsView>()!;

            var view = settingWindow.CurrentView as SettingsView;
            var settingsViewModel = view!.GetViewModel<SettingsViewModel>()!;
            settingsViewModel.CurrentPluginSettingsView = currentMainViewSettings.SettingsView;
            settingsViewModel.SetMainViewSettings(currentMainViewSettings);
            currentMainViewSettings.SettingNavigationConfig(view.navigationBar.Children);
            settingsViewModel.InitMainViewSettings();

            settingWindow.Title = "全局设置";
            settingWindow.Height = 400;
            settingWindow.Width = 600;
            settingWindow.WindowStartupLocation = 2;
            settingWindow.Owner = MainWindow;
            settingWindow.Topmost = true;

            settingWindow.ShowDialog();
        }

        protected override void ProtectedInjectView(MainView_RunView view)
        {
            SetCollection(view.buttonStackPanel.Children);
        }

        /// <summary>
        /// 设置按钮集合，包括一个返回主菜单的按钮，并根据插件详细信息设置顶部视图设置。
        /// </summary>
        /// <param name="collection">UIElementCollection 实例，表示要添加按钮的集合。</param>
        public void SetCollection(UIElementCollection collection)
        {
            if (currentMainViewSettings == null)
                return;

            currentMainViewSettings.TopSetting(collection);
        }

        /// <summary>
        /// 导航到主视图的方法，同时处理过场动画的启动和结束。
        /// </summary>
        private void ToMainView()
        {
            // 导航到 CutsceneView 并设置为当前过场动画视图
            var cutscene = NavigateTo<CutsceneView>();
            Model.CurrentCutsceneView = cutscene;
            cutscene.Start();

            // 在后台线程中处理视图更新，以确保 UI 线程不会被阻塞
            Task.Run(async () =>
            {
                UnLoadPlugin(Model.CurrentPluginDetails);
                // 清除当前插件详细信息
                Model.CurrentPluginDetails = null;
                // 使用 DispatcherService 在 UI 线程上执行操作

                await ToMainThreadAsync();

                // 导航到 MainView 并设置为当前主视图
                Model.CurrentMainView = NavigateTo<MainView>(Model.CurrentView);

                // 当过场动画结束时，将当前过场动画视图设置为 null
                cutscene.End(() =>
                {
                    Model.CurrentCutsceneView = null;
                });
            });
        }
    }
}