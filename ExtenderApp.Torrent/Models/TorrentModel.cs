using System.Collections.Concurrent;
using ExtenderApp.Data;


namespace ExtenderApp.Torrent
{
    public class TorrentModel
    {
        public List<TorrentFileInfoNodeParent> Downloads { get; set; }
        public HashSet<string> Tracker { get; set; }
    }
}
