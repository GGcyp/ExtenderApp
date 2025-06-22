using ExtenderApp.Services;


namespace ExtenderApp.Torrent
{
    class TorrentStartup : PluginEntityStartup
    {
        public override Type StartType => typeof(TorrentMainView);
    }
}
