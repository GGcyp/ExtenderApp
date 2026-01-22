using CommunityToolkit.Mvvm.Input;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Services;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainViews.ViewModels
{
    public class PluginViewModle : ExtenderAppViewModel
    {
        private readonly MainViewNavigation _mainViewNavigation;

        /// <summary>
        /// 选中的插件详情信息
        /// </summary>
        public PluginDetails? CurrentPluginDetails { get; set; }

        /// <summary>
        /// 插件仓库实例
        /// </summary>
        public PluginStore? PluginStore { get; set; }

        public RelayCommand<PluginDetails> OpenPluginCommand { get; set; }

        public PluginViewModle(IServiceStore serviceStore, PluginStore store, MainViewNavigation mainViewNavigation) : base(serviceStore)
        {
            OpenPluginCommand = new(OpenPlugin);
            PluginStore = store;
            _mainViewNavigation = mainViewNavigation;
        }

        public void OpenPlugin(PluginDetails? details)
        {
            Task.Run(async () =>
            {
                await LoadPluginAsync(details!);
                await ToMainThreadAsync();
                NavigateTo(details!.StartupType!, details.PluginScopeName);
                _mainViewNavigation.NavigateToRun();
            });
        }
    }
}