using System.Buffers;
using System.Diagnostics;
using System.IO;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Torrent
{
    /// <summary>
    /// 表示一个Torrent Peer的类。
    /// </summary>
    public class TorrentPeer : DisposableObject
    {
        /// <summary>
        /// 与Peer进行通信的LinkClient。
        /// </summary>
        private readonly LinkClient<ITcpLinker, BTMessageParser> _linkClient;

        /// <summary>
        /// 私有只读属性，表示 TorrentFileInfoNodeParent 对象
        /// </summary>
        private readonly TorrentFileInfoNodeParent _nodeParent;

        /// <summary>
        /// 私有只读属性，表示 IFileOperateProvider 对象
        /// </summary>
        private readonly IFileOperateProvider _fileOperateProvider;

        /// <summary>
        /// 私有只读属性，表示 PeerId 对象
        /// </summary>
        private readonly PeerId _localPeerId;

        /// <summary>
        /// Peer的地址。
        /// </summary>
        public PeerAddress Address { get; }

        /// <summary>
        /// 获取远程对等点的信息。
        /// </summary>
        /// <returns>返回包含远程对等点地址和ID的<see cref="PeerInfo"/>对象。</returns>
        public PeerInfo RemotePeerInfo => new PeerInfo(Address, RemotePeerId);

        /// <summary>
        /// Peer的ID。
        /// </summary>
        public PeerId RemotePeerId { get; private set; }

        /// <summary>
        /// Torrent的InfoHash。
        /// </summary>
        public InfoHash Hash { get; private set; }

        /// <summary>
        /// 获取远程位字段数据
        /// </summary>
        /// <returns>返回远程位字段数据</returns>
        public BitFieldData? RemoteBitField { get; private set; }

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
        /// 移除回调委托。
        /// </summary>
        /// <value>
        /// 一个类型为 <see cref="Action{T}"/> 的委托，表示当需要移除某个节点时调用的回调方法。
        /// 其中，<typeparamref name="T"/> 是 <see cref="PeerInfo"/> 类型。
        /// </value>
        public Action<PeerInfo>? RemoveCallback { get; set; }

        public event Action<PeerInfo, InfoHash, TorrentPeer>? OnHandshake;

        /// <summary>
        /// 初始化TorrentPeer实例。
        /// </summary>
        /// <param name="linkClient">与Peer进行通信的LinkClient。</param>
        /// <param name="hash">Torrent的InfoHash。</param>
        /// <param name="address">Peer的地址。</param>
        /// <param name="localPeerId">当前Peer的ID。</param>
        /// <exception cref="InvalidOperationException">如果LinkClient未连接，则抛出此异常。</exception>
        public TorrentPeer(LinkClient<ITcpLinker, BTMessageParser> linkClient, PeerAddress address, PeerId localPeerId, TorrentFileInfoNodeParent parent, IFileOperateProvider provider)
        {
            if (!linkClient.Connected)
                throw new InvalidOperationException("在创建TorrentPeer实例之前，必须确保LinkClient已连接。");

            _linkClient = linkClient;
            _nodeParent = parent;
            _localPeerId = localPeerId;
            _fileOperateProvider = provider;
            Address = address;

            BTMessageParser parser = _linkClient.Parser;
            parser.OnHandshake += PrivateOnHandshake;
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
            linkClient.OnErrored += OnErrored;
        }

        /// <summary>
        /// 向对方发送握手信息。
        /// </summary>
        /// <param name="hash">Torrent的InfoHash。</param>
        public void SendHandshake(InfoHash hash)
        {
            Hash = hash;
            var databuffer = DataBuffer<InfoHash, PeerId>.GetDataBuffer();
            databuffer.Item1 = hash;
            databuffer.Item2 = _localPeerId;
            _linkClient.Send(databuffer);
            databuffer.Release();
        }

        /// <summary>
        /// 处理握手消息。
        /// </summary>
        /// <param name="hash">对方的InfoHash。</param>
        /// <param name="id">对方的PeerId。</param>
        /// <exception cref="InvalidDataException">如果握手信息中的InfoHash或PeerId为空，或者InfoHash不匹配，则抛出此异常。</exception>
        private void PrivateOnHandshake(InfoHash hash, PeerId id)
        {
            if (hash.IsEmpty || id.IsEmpty)
            {
                throw new InvalidDataException("握手信息中的InfoHash或PeerId不能为空。");
            }
            RemotePeerId = id;
            OnHandshake?.Invoke(new PeerInfo(Address, id), hash, this);

            byte[] bytes = ArrayPool<byte>.Shared.Rent(_nodeParent.LocalBiteField.Length);
            _nodeParent.LocalBiteField.ToBytes(bytes);
            _linkClient.Send(BTMessage.CreateBitField(_nodeParent.LocalBiteField.Length, bytes));
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
            Debug.Print($"[Peer] 对方 DHT 端口: {port}");
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
            //Console.WriteLine($"[Peer] 对方取消请求分片 {pieceIndex}，偏移 {begin}，长度 {length}。");
            // 这里可以实现取消上传逻辑
            Debug.Print($"[Peer] 对方取消请求分片 {pieceIndex}，偏移 {begin}，长度 {length}。");
        }

        /// <summary>
        /// 处理Request消息。
        /// </summary>
        /// <param name="pieceIndex">请求的分片索引。</param>
        /// <param name="begin">请求的起始偏移。</param>
        /// <param name="length">请求的长度。</param>
        private void OnRequest(int pieceIndex, int begin, int length)
        {
            if (!_nodeParent.LocalBiteField.Get(pieceIndex))
            {
                return; // 如果本地没有这个分片，则不处理请求
            }

            DataBuffer<LinkClient, BTMessage> dataBuffer = DataBuffer<LinkClient, BTMessage>.GetDataBuffer();
            dataBuffer.Item1 = _linkClient;
            dataBuffer.Item2 = BTMessage.CreateRequest(pieceIndex, begin, length);
            dataBuffer.SetProcessAction<byte[]>(PrivateOnRequest);
            _nodeParent.PieceData.GetPieceAsync(pieceIndex, begin, length, _fileOperateProvider, dataBuffer.Process);
            Interlocked.Add(ref _nodeParent.Uploaded, length);
        }

        /// <summary>
        /// 处理Piece消息。
        /// </summary>
        /// <param name="pieceIndex">收到的分片索引。</param>
        /// <param name="begin">收到的起始偏移。</param>
        /// <param name="data">收到的数据。</param>
        private void OnPiece(int pieceIndex, int begin, int length, byte[] data)
        {
            if (_nodeParent.LocalBiteField.Get(pieceIndex))
            {
                return; // 如果本地已经有这个分片，则不处理
            }

            // 将收到的数据写入本地文件
            _nodeParent.PieceData.SetPiece(pieceIndex, begin, length, data, _fileOperateProvider);
            Interlocked.Add(ref _nodeParent.Downloaded, length);
            Interlocked.Add(ref _nodeParent.Left, -length);
        }

        /// <summary>
        /// 处理BitField消息。
        /// </summary>
        /// <param name="bitfield">对方的BitField。</param>
        private void OnBitField(byte[] bitfield)
        {
            if (bitfield == null)
            {
                RemoteBitField = new BitFieldData(bitfield);
                return;
            }

            // 处理BitField数据
            RemoteBitField.And(bitfield);
        }

        /// <summary>
        /// 处理Have消息。
        /// </summary>
        /// <param name="pieceIndex">对方拥有的分片索引。</param>
        private void OnHave(int pieceIndex)
        {
            RemoteBitField.Set(pieceIndex);
        }

        /// <summary>
        /// 发送一个“我有”消息
        /// </summary>
        /// <param name="pieceIndex">分片的索引</param>
        public void Have(int pieceIndex)
        {
            _linkClient.Send(BTMessage.CreateHave(pieceIndex));
        }

        /// <summary>
        /// 处理未知类型的消息。
        /// </summary>
        /// <param name="message">未知类型的消息。</param>
        private void OnUnknown(BTMessage message)
        {
            throw new NotSupportedException($"未知的BT消息类型: {message.Id}。请检查协议实现。");
        }

        /// <summary>
        /// 当发生错误时调用此方法。
        /// </summary>
        /// <param name="ex">异常信息。</param>
        /// <remarks>
        /// 在此方法中可以实现错误处理逻辑，例如重连或记录日志。
        /// </remarks>
        private void OnErrored(Exception ex)
        {
            // 这里可以实现错误处理逻辑，比如重连或记录日志
            RemoveCallback?.Invoke(new PeerInfo(Address, RemotePeerId));
        }

        /// <summary>
        /// 私有方法，处理请求数据
        /// </summary>
        /// <param name="dataBuffer">包含LinkClient和BTMessage的数据缓冲区</param>
        /// <param name="bytes">接收到的字节数据</param>
        private void PrivateOnRequest(DataBuffer<LinkClient, BTMessage> dataBuffer, byte[] bytes)
        {
            var linkClient = dataBuffer.Item1;
            int pieceIndex = dataBuffer.Item2.PieceIndex;
            int begin = dataBuffer.Item2.Begin;
            int length = dataBuffer.Item2.Length;
            if (bytes == null || bytes.Length == 0)
            {
                return; // 如果没有数据，则不处理
            }
            linkClient.Send(BTMessage.CreatePiece(pieceIndex, begin, length, bytes));
        }

        protected override void Dispose(bool disposing)
        {
            _linkClient.Close();
            _linkClient.Dispose();
        }
    }
}
