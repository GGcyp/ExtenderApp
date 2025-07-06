using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;
using ExtenderApp.ViewModels;
using System.IO;

namespace ExtenderApp.Torrent
{
    public class TorrentMainViewModel : ExtenderAppViewModel
    {
        public TorrentMainViewModel(TorrentProvider provider, IServiceStore serviceStore) : base(serviceStore)
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

            //var torrentFile = torrentFileForamtter.Decode();
            //var path = _serviceStore.PathService.CreateFolderPathForAppRootFolder("test");
            //torrentFile.FileInfoNode.CreateFileOrFolder(path);
            var torrent = provider.GetTorrent(File.ReadAllBytes("E:\\迅雷下载\\587383C5492E191CEA4CA9AF068356D9A8E391FE.torrent"));
            Task.Run(async () =>
            {
                await Task.Delay(10000);
                torrent.AnnounceAsync();
            });
        }
    }
}
