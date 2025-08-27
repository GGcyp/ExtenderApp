using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;
using ExtenderApp.Services;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Models.Formatters;
using ExtenderApp.Torrents.ViewModels;
using ExtenderApp.Torrents.Views;


namespace ExtenderApp.Torrents
{
    class TorrentStartup : PluginEntityStartup
    {
        public override Type StartType => typeof(TorrentMainView);

        public override void AddService(IServiceCollection services)
        {
            services.AddTransient<TorrentAddView>();
            services.AddTransient<TorrentMainView>();
            services.AddTransient<TorrentAddFileInfoView>();
            services.AddTransient<TorrentDownloadListView>();
            services.AddTransient<TorrentDownloadStateView>();
            services.AddTransient<TorrentRecyclebinListView>();
            services.AddTransient<TorrentDownloadFileInfoView>();
            services.AddTransient<TorrentDownloadCompletedListView>();

            services.AddTransient<TorrentAddViewModel>();
            services.AddTransient<TorrentMainViewModel>();
            services.AddTransient<TorrentFileInfoViewModel>();
            services.AddTransient<TorrentAddFileInfoViewModel>();
            services.AddTransient<TorrentDownloadListViewModel>();
            services.AddTransient<TorrentDownloadStateViewModel>();
            services.AddTransient<TorrentDownloadCompletedListViewModel>();

            services.AddSingleton<TorrentLongingFactory>();
        }

        public override void ConfigureBinaryFormatterStore(IBinaryFormatterStore store)
        {
            store.AddLocalDataFormatter<TorrentModel, TorrentModelFormatter>();
            store.AddVersionData<TorrentFileInfoNode, TorrentFileInfoNodeFormatter_1>();
            store.AddVersionData<TorrentInfo, TorrentInfoFormatter_1>();
        }
    }
}
