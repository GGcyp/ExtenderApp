using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    public class TorrentPeer : Peer<ITcpLinker, BTMessageParser, BTMessage>
    {
        public TorrentPeer(LinkClient<ITcpLinker, BTMessageParser> linkClient, PeerInfo peerInfo)
            : base(linkClient, peerInfo)
        {
        }
    }
}
