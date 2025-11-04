using System.Collections;
using ExtenderApp.Abstract;
using ExtenderApp.Torrents.Views;
using ExtenderApp.Views;
using ExtenderApp.Services;

namespace ExtenderApp.Torrents.Models
{
    internal class TorrentMainViewSettings : MainViewSettings<TorrentSettingsView>
    {
        public TorrentMainViewSettings(IServiceStore serviceStore) : base(serviceStore)
        {

        }

        public override void SettingNavigationConfig(IList list)
        {
            list.Add(CreateSettingsNavigationButton(View.engineSettings));
            list.Add(CreateSettingsNavigationButton(View.dhtSettings));
            list.Add(CreateSettingsNavigationButton(View.torrentSettings));
            list.Add(CreateSettingsNavigationButton(View.peerSettings));
            list.Add(CreateSettingsNavigationButton(View.saveSettings));
        }

        protected override TorrentSettingsView CreateSettingsView()
        {
            return ServiceStore.NavigationService.NavigateTo<TorrentSettingsView>(GetPluginDetails().PluginScopeName);
        }
    }
}
