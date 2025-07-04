using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;
using ExtenderApp.ViewModels;
using System.IO;

namespace ExtenderApp.Torrent
{
    public class TorrentMainViewModel : ExtenderAppViewModel
    {
        public TorrentMainViewModel(TorrentFileForamtter torrentFileForamtter, LinkClient<IUdpLinker, UdpTrackerParser> client, LocalTorrentInfo localTorrentInfo, IServiceStore serviceStore) : base(serviceStore)
        {
            //var tracker = new Tracker(client, new Uri("udp://tracker.opentrackr.org:1337/announce"));
            //// 这里可以添加对 tracker 的操作，例如发送请求、处理响应等
            //var radom = new Random();
            //tracker.Connection(radom.Next());
            //var trackerRequest = new TrackerRequest
            //{
            //    Id = localTorrentInfo.Id,
            //    Port = (ushort)localTorrentInfo.Port,
            //    Uploaded = 0,
            //    Downloaded = 0,
            //    Left = 0,
            //    Compact = "1",
            //    NoPeerId = "1",
            //    Hash = new InfoHash(provider.ComputeHash<SHA1>("66978df95e79085bc055193e57d8921e39697d80"), HashValue.Empty),
            //    Event = (byte)AnnounceEventType.None,
            //    TransactionId = radom.Next()
            //};
            //tracker.OnConnection += t => t.AnnounceAsync(trackerRequest);

            var torrentFile = torrentFileForamtter.Decode(File.ReadAllBytes("E:\\迅雷下载\\G奶尤物｜易鳴夫妻｜奶昔吖｜唯美性愛檔 穿性感情趣制服舔逗雞巴乳交各種體位速插波濤洶湧垂涎欲滴 720p\\E8FF39F2A378FE05D004BE5FED5A679022AF264F.torrent"));
            var path = _serviceStore.PathService.CreateFolderPathForAppRootFolder("test");
            torrentFile.FileInfoNode.CreateFileOrFolder(path);
        }
    }
}
