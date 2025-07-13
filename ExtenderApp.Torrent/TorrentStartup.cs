using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;
using ExtenderApp.Services;
using ExtenderApp.Torrent.Models.InfoHashs;
using ExtenderApp.Torrent.Models.Torrents.TorrentDowns;


namespace ExtenderApp.Torrent
{
    class TorrentStartup : PluginEntityStartup
    {
        private readonly string ClientPrefix = "-EX0001-";

        public override Type StartType => typeof(TorrentMainView);

        public override void AddService(IServiceCollection services)
        {
            services.AddTransient<TorrentMainViewModel>();
            services.AddTransient<TorrentMainView>();

            services.AddSingleton<HttpTrackerParser>();
            services.AddSingleton<UdpTrackerParser>();
            services.AddTransient<BTMessageParser>();

            services.AddSingleton<TorrentProvider>();
            services.AddSingleton<TorrentPeerProvider>();
            services.AddSingleton<TrackerProvider>();
            services.AddSingleton<TorrentSender>();
            services.AddSingleton<TorrentFileFormatter>();

            services.AddSingleton(new LocalTorrentInfo
            {
                Port = 6881,
                Id = PeerId.CreateId()
            });
        }

        public override void ConfigureBinaryFormatterStore(IBinaryFormatterStore store)
        {
            store.Add<TorrentFileDownInfoNode, TorrentFileDownInfoNodeFormatter>();
            store.Add<TorrentFileDownInfoNodeParent, TorrentFileDownInfoNodeParentFormatter>();
            store.Add<InfoHash, InfoHashForamtter>();
        }
    }
}
