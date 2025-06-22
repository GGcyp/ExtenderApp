using ExtenderApp.Data;
using ExtenderApp.Torrent.Models.Peers;

namespace ExtenderApp.Torrent
{
    public class Peer
    {
        public PeerInfo PeerInfo { get; private set; }

        public LinkState Status { get; private set; }

    }
}
