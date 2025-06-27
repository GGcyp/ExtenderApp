using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    public class TorrentPeer : Peer<ITcpLinker, TorrentLinkParser, BTMessage>
    {
        public TorrentPeer(LinkClient<ITcpLinker, TorrentLinkParser> linkClient, PeerInfo peerInfo)
            : base(linkClient, peerInfo)
        {
        }
    }
}
