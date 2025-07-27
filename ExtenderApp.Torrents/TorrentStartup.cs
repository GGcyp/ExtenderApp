using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Services;
using ExtenderApp.Torrents.Models;


namespace ExtenderApp.Torrents
{
    class TorrentStartup : PluginEntityStartup
    {
        public override Type StartType => typeof(TorrentMainView);

        public override void AddService(IServiceCollection services)
        {
            services.AddTransient<TorrentMainViewModel>();
            services.AddTransient<TorrentMainView>();
            services.AddTransient<TorrentFileInfoView>();
            services.AddTransient<TorrentFileInfoViewModel>();
        }

        public override void ConfigureBinaryFormatterStore(IBinaryFormatterStore store)
        {
            store.Add<TorrentModel, TorrentModelFormatter>();
        }
    }
}
