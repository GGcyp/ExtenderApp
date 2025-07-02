using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtenderApp.Torrent
{
    public class Torrent
    {
        public TorrentFile TorrentFile { get; set; }

        public Torrent(TorrentFile torrentFile)
        {
            TorrentFile = torrentFile;
        }
    }
}
