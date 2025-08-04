using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Services;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.ViewModels;
using ExtenderApp.Torrents.Views;


namespace ExtenderApp.Torrents
{
    class TorrentStartup : PluginEntityStartup
    {
        public override Type StartType => typeof(TorrentMainView);

        public override void AddService(IServiceCollection services)
        {
            services.AddTransient<TorrentMainView>();
            services.AddTransient<TorrentFileInfoView>();
            services.AddTransient<TorrentDownloadListView>();
            services.AddTransient<TorrentDownloadCompletedListView>();

            services.AddTransient<TorrentMainViewModel>();
            services.AddTransient<TorrentFileInfoViewModel>();
            services.AddTransient<TorrentDownloadListViewModel>();
            services.AddTransient<TorrentDownloadCompletedListViewModel>();
        }

        public override void ConfigureBinaryFormatterStore(IBinaryFormatterStore store)
        {
            store.Add<TorrentModel, TorrentModelFormatter>();
        }
    }
}
