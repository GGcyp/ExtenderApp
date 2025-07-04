using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    public class Torrent
    {
        public TorrentFile TorrentFile { get; }

        public List<Tracker> Trackers { get; }

        public List<TorrentPeer> Peers { get; }


        public Torrent(TorrentFile torrentFile)
        {
            TorrentFile = torrentFile;
            Peers = new();
            Trackers = new();
        }
    }
}
