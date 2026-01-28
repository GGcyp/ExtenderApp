using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
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
        public ObservableCollection<PluginDetails>? PluginStore { get; set; }

        public RelayCommand<PluginDetails> OpenPluginCommand { get; set; }

        public PluginViewModle(MainViewNavigation mainViewNavigation)
        {
            OpenPluginCommand = new(OpenPlugin);
            _mainViewNavigation = mainViewNavigation;
        }

        public override void Inject(IServiceProvider serviceProvider)
        {
            base.Inject(serviceProvider);
            PluginStore = new(GetRequiredService<IPluginService>().GetAllPlugins());
        }

        public void OpenPlugin(PluginDetails? details)
        {
            if (details == null)
                return;

            OpenPluginAsync(details).ConfigureAwait(false);
        }

        private async Task OpenPluginAsync(PluginDetails details)
        {
            await LoadPluginAsync(details!);
            await SwitchToMainThreadAsync();
            NavigateTo(details!.StartupType!, details.PluginScopeName);
            _mainViewNavigation.NavigateToRun();
        }
    }
}