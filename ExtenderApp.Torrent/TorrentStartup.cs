using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Data;
using ExtenderApp.Services;


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

            services.AddTransient<HttpTrackerParser>();
            services.AddSingleton<UdpTrackerParser>();
            services.AddTransient<BTMessageParser>();

            services.AddSingleton<TorrentProvider>();
            services.AddSingleton<TorrentFileForamtter>();

            services.AddSingleton(new LocalTorrentInfo
            {
                Port = 6881,
                Id = PeerId.CreateId()
            });
        }
    }
}
