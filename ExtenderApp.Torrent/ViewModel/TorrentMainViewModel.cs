using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Torrent
{
    public class TorrentMainViewModel : ExtenderAppViewModel<TorrentMainView, TorrentModel>
    {
        public TorrentMainViewModel(TorrentProvider provider, IBinaryParser binaryParser, IFileOperateProvider fileOperateProvider, IServiceStore serviceStore) : base(serviceStore)
        {
            //var torrent = provider.GetTorrent(File.ReadAllBytes("E:\\迅雷下载\\5A8F9BB08F1BE7DE41D87E5DE5B60E3961393AAC.torrent"));
            var torrent = provider.GetTorrent("E:\\迅雷下载\\SONE-436\\8CB1CA00ADC5F94559CD95CF48E693ADD5776B3A.torrent");
            torrent.TorrentPeers.OnPeerAdded += TorrentPeers_OnPeerAdded;
            Task.Run(async () =>
            {
                await Task.Delay(10000);
                torrent.AnnounceAsync();
            });

            //Model.Downloads = new();
            //LoadModel();
            //Model.Downloads = new();
            //Model.Downloads.Add(torrent2.InfoNodeParent);

            //var temp1 = binaryParser.Serialize(torrent2.DownParent);
            //var temp2 = binaryParser.Compression(torrent2.DownParent, Data.CompressionType.Lz4Block);
            //Info($"正常序列化：{temp1.Length} 压缩后：{temp2.Length}");
            //var temp3 = binaryParser.Decompression<TorrentFileDownInfoNodeParent>(temp2);
            //Info($"序列化前：{torrent2.DownParent.TorrentFileInfo} {torrent2.DownParent.Hash} {torrent2.DownParent.ParentPath} {torrent2.DownParent.PieceLength}");
            //Info($"反序列化：{temp3.TorrentFileInfo} {temp3.Hash} {temp3.ParentPath} {temp3.PieceLength}");

            //foreach (var item in Model.Downloads)
            //{
            //    var torrent = provider.GetTorrent(item);
            //}

            //SaveModel();
            //torrent2.AnnounceAsync();
        }

        private void TorrentPeers_OnPeerAdded(TorrentPeer obj)
        {
            Debug(obj.Address);
        }
    }
}
