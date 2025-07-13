using System.IO;
using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 表示一个Torrent Peer的类。
    /// </summary>
    public class TorrentPeer
    {
        /// <summary>
        /// 与Peer进行通信的LinkClient。
        /// </summary>
        private readonly LinkClient<ITcpLinker, BTMessageParser> _linkClient;

        /// <summary>
        /// Peer的地址。
        /// </summary>
        public PeerAddress Address { get; }

        /// <summary>
        /// Peer的ID。
        /// </summary>
        public PeerId RemotePeerId { get; private set; }

        /// <summary>
        /// Torrent的InfoHash。
        /// </summary>
        public InfoHash Hash { get; }

        /// <summary>
        /// 表示当前Peer是否被对方Choked。
        /// </summary>
        public bool IsChoked { get; private set; } = true;

        /// <summary>
        /// 表示当前Peer是否对对方感兴趣。
        /// </summary>
        public bool IsInterested { get; private set; } = false;

        /// <summary>
        /// 表示对方是否对当前Peer感兴趣。
        /// </summary>
        public bool IsRemoteInterested { get; private set; } = false;

        /// <summary>
        /// 初始化TorrentPeer实例。
        /// </summary>
        /// <param name="linkClient">与Peer进行通信的LinkClient。</param>
        /// <param name="hash">Torrent的InfoHash。</param>
        /// <param name="address">Peer的地址。</param>
        /// <param name="localPeerId">当前Peer的ID。</param>
        /// <exception cref="InvalidOperationException">如果LinkClient未连接，则抛出此异常。</exception>
        public TorrentPeer(LinkClient<ITcpLinker, BTMessageParser> linkClient, InfoHash hash, PeerAddress address, PeerId localPeerId)
        {
            if (!linkClient.Connected)
                throw new InvalidOperationException("在创建TorrentPeer实例之前，必须确保LinkClient已连接。");

            _linkClient = linkClient;
            Address = address;
            Hash = hash;

            BTMessageParser parser = _linkClient.Parser;
            parser.OnHandshake += OnHandshake;
            parser.OnChoke += OnChoke;
            parser.OnUnchoke += OnUnchoke;
            parser.OnInterested += OnInterested;
            parser.OnNotInterested += OnNotInterested;
            parser.OnHave += OnHave;
            parser.OnBitField += OnBitField;
            parser.OnRequest += OnRequest;
            parser.OnPiece += OnPiece;
            parser.OnCancel += OnCancel;
            parser.OnPort += OnPort;
            parser.OnUnknown += OnUnknown;

            SendHandshake(hash, localPeerId);
        }

        /// <summary>
        /// 向对方发送握手信息。
        /// </summary>
        /// <param name="hash">Torrent的InfoHash。</param>
        /// <param name="localPeerId">当前Peer的ID。</param>
        public void SendHandshake(InfoHash hash, PeerId localPeerId)
        {
            var databuffer = DataBuffer<InfoHash, PeerId>.GetDataBuffer();
            databuffer.Item1 = hash;
            databuffer.Item2 = localPeerId;
            _linkClient.Send(databuffer);
            databuffer.Release();
        }

        /// <summary>
        /// 处理握手消息。
        /// </summary>
        /// <param name="hash">对方的InfoHash。</param>
        /// <param name="id">对方的PeerId。</param>
        /// <exception cref="InvalidDataException">如果握手信息中的InfoHash或PeerId为空，或者InfoHash不匹配，则抛出此异常。</exception>
        private void OnHandshake(InfoHash hash, PeerId id)
        {
            if (hash.IsEmpty || id.IsEmpty)
            {
                throw new InvalidDataException("握手信息中的InfoHash或PeerId不能为空。");
            }

            if (hash != Hash)
            {
                throw new InvalidDataException($"握手信息中的InfoHash不匹配。预期: {Hash.GetSha1orSha256().ToHexString()}, 实际: {hash.GetSha1orSha256().ToHexString()}");
            }
            RemotePeerId = id;
        }

        /// <summary>
        /// 处理Choke消息。
        /// </summary>
        private void OnChoke()
        {
            IsChoked = true;
        }

        /// <summary>
        /// 处理Unchoke消息。
        /// </summary>
        private void OnUnchoke()
        {
            IsChoked = false;
        }

        /// <summary>
        /// 处理Interested消息。
        /// </summary>
        private void OnInterested()
        {
            IsRemoteInterested = true;
        }

        /// <summary>
        /// 处理NotInterested消息。
        /// </summary>
        private void OnNotInterested()
        {
            IsRemoteInterested = false;
        }

        /// <summary>
        /// 处理Port消息。
        /// </summary>
        /// <param name="port">对方的DHT端口。</param>
        private void OnPort(ushort port)
        {
            Console.WriteLine($"[Peer] 对方 DHT 端口: {port}");
            // 可用于 DHT 扩展
        }

        /// <summary>
        /// 处理Cancel消息。
        /// </summary>
        /// <param name="pieceIndex">取消请求的分片索引。</param>
        /// <param name="begin">取消请求的起始偏移。</param>
        /// <param name="length">取消请求的长度。</param>
        private void OnCancel(int pieceIndex, int begin, int length)
        {
            Console.WriteLine($"[Peer] 对方取消请求分片 {pieceIndex}，偏移 {begin}，长度 {length}。");
            // 这里可以实现取消上传逻辑
        }

        /// <summary>
        /// 处理Request消息。
        /// </summary>
        /// <param name="pieceIndex">请求的分片索引。</param>
        /// <param name="begin">请求的起始偏移。</param>
        /// <param name="length">请求的长度。</param>
        private void OnRequest(int pieceIndex, int begin, int length)
        {
            Console.WriteLine($"[Peer] 对方请求分片 {pieceIndex}，偏移 {begin}，长度 {length}。");
            // 这里可以实现上传逻辑
        }

        /// <summary>
        /// 处理Piece消息。
        /// </summary>
        /// <param name="pieceIndex">收到的分片索引。</param>
        /// <param name="begin">收到的起始偏移。</param>
        /// <param name="data">收到的数据。</param>
        private void OnPiece(int pieceIndex, int begin, byte[] data)
        {
            Console.WriteLine($"[Peer] 收到分片 {pieceIndex}，偏移 {begin}，数据长度 {data.Length}。");
            // 这里可以实现数据写入逻辑
        }

        /// <summary>
        /// 处理BitField消息。
        /// </summary>
        /// <param name="bitfield">对方的BitField。</param>
        private void OnBitField(byte[] bitfield)
        {
            //_remoteBitField = bitfield;
            //_remotePieces.Clear();
            //for (int i = 0; i < bitfield.Length * 8; i++)
            //{
            //    int byteIndex = i / 8;
            //    int bitIndex = 7 - (i % 8);
            //    if (byteIndex < bitfield.Length && ((bitfield[byteIndex] >> bitIndex) & 1) == 1)
            //    {
            //        _remotePieces.Add(i);
            //    }
            //}
            //Console.WriteLine($"[Peer] 收到对方 bitfield，拥有 {_remotePieces.Count} 个分片。");
        }

        /// <summary>
        /// 处理Have消息。
        /// </summary>
        /// <param name="pieceIndex">对方拥有的分片索引。</param>
        private void OnHave(int pieceIndex)
        {
            //_remotePieces.Add(pieceIndex);
        }

        /// <summary>
        /// 处理未知类型的消息。
        /// </summary>
        /// <param name="message">未知类型的消息。</param>
        private void OnUnknown(BTMessage message)
        {
            Console.WriteLine($"[Peer] 收到未知类型消息: {message.Id}");
        }
    }
}
