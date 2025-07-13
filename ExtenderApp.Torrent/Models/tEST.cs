//using System;
//using System.Buffers.Binary;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net.Sockets;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;
//using ExtenderApp.Data;

//namespace ExtenderApp.Torrent.Models
//{
//    /// <summary>
//    /// BitTorrent Peer 连接
//    /// </summary>
//    public class PeerConnection : IDisposable
//    {
//        private readonly TcpClient _client;
//        private readonly NetworkStream _stream;
//        private readonly Handshake _handshake;
//        private readonly TorrentFileManager _fileManager;
//        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
//        private readonly Queue<BTMessageEncoder> _sendQueue = new Queue<BTMessageEncoder>();
//        private bool _isChoked = true;
//        private bool _isInterested = false;
//        private BitFieldData _peerBitField;
//        private Task _receiveTask;
//        private Task _sendTask;

//        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
//        public event EventHandler<BlockReceivedEventArgs> BlockReceived;
//        public event EventHandler Disconnected;

//        public PeerConnection(TcpClient client, Handshake handshake, TorrentFileManager fileManager)
//        {
//            _client = client;
//            _stream = client.GetStream();
//            _handshake = handshake;
//            _fileManager = fileManager;
//        }

//        public async Task ConnectAsync()
//        {
//            //发送握手消息
//            byte[] handshakeData = _handshake.Encode();
//            await _stream.WriteAsync(handshakeData, 0, handshakeData.Length);

//            //接收握手消息
//            byte[] receiveBuffer = new byte[68];
//            int bytesRead = await _stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);
//            if (bytesRead != 68)
//                throw new InvalidDataException("握手消息长度不正确");

//            Handshake peerHandshake = Handshake.Decode(receiveBuffer);
//            if (!peerHandshake.Hash.SequenceEqual(_handshake.Hash))
//                throw new InvalidDataException("InfoHash不匹配");

//            Console.WriteLine("与Peer握手成功");

//            //启动接收和发送任务
//           _receiveTask = ReceiveMessagesAsync();
//            _sendTask = ProcessSendQueueAsync();
//        }

//        private async Task ReceiveMessagesAsync()
//        {
//            try
//            {
//                byte[] lengthBuffer = new byte[4];
//                while (!_cts.Token.IsCancellationRequested)
//                {
//                    //读取长度前缀
//                    int bytesRead = await ReadFullyAsync(lengthBuffer, 0, 4);
//                    if (bytesRead == 0)
//                        break; // 连接关闭

//                    int messageLength = BinaryPrimitives.ReadInt32BigEndian(lengthBuffer);
//                    if (messageLength < 0)
//                        throw new InvalidDataException("无效的消息长度");

//                    if (messageLength == 0)
//                    {
//                        //保持活跃消息
//                       MessageReceived?.Invoke(this, new MessageReceivedEventArgs(new KeepAliveMessage()));
//                        continue;
//                    }

//                    //读取消息ID和数据
//                    byte[] messageBuffer = new byte[messageLength];
//                    await ReadFullyAsync(messageBuffer, 0, messageLength);

//                    //解析消息
//                   BTMessageEncoder message = BTMessageEncoder.Decode(messageBuffer);
//                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));

//                    //处理特定消息
//                    switch (message)
//                    {
//                        case BitFieldMessage bitFieldMsg:
//                            _peerBitField = new BitFieldData(bitFieldMsg.BitField, _fileManager.PieceCount);
//                            //检查是否有感兴趣的分片
//                           _isInterested = HasInterestingPieces();
//                            if (_isInterested)
//                                EnqueueMessage(new InterestedMessage());
//                            break;
//                        case HaveMessage haveMsg:
//                            _peerBitField[haveMsg.PieceIndex] = true;
//                            //检查是否对新分片感兴趣
//                            if (!_isInterested && _fileManager.BitField[haveMsg.PieceIndex] == false)
//                            {
//                                _isInterested = true;
//                                EnqueueMessage(new InterestedMessage());
//                            }
//                            break;
//                        case UnchokeMessage _:
//                            _isChoked = false;
//                            //开始请求数据
//                            RequestPieces();
//                            break;
//                        case ChokeMessage _:
//                            _isChoked = true;
//                            break;
//                        case PieceMessage pieceMsg:
//                            //保存接收到的数据块
//                           await _fileManager.WriteBlockAsync(pieceMsg.PieceIndex, pieceMsg.Begin, pieceMsg.Block);
//                            BlockReceived?.Invoke(this, new BlockReceivedEventArgs(
//                                pieceMsg.PieceIndex, pieceMsg.Begin, pieceMsg.Block.Length));

//                            //请求更多数据
//                            RequestPieces();
//                            break;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"接收消息时发生错误: {ex.Message}");
//            }
//            finally
//            {
//                Disconnect();
//            }
//        }

//        private async Task<int> ReadFullyAsync(byte[] buffer, int offset, int count)
//        {
//            int totalBytesRead = 0;
//            while (totalBytesRead < count)
//            {
//                int bytesRead = await _stream.ReadAsync(buffer, offset + totalBytesRead, count - totalBytesRead);
//                if (bytesRead == 0)
//                    return totalBytesRead; // 连接关闭
//                totalBytesRead += bytesRead;
//            }
//            return totalBytesRead;
//        }

//        private async Task ProcessSendQueueAsync()
//        {
//            try
//            {
//                while (!_cts.Token.IsCancellationRequested)
//                {
//                    BTMessageEncoder message = null;
//                    lock (_sendQueue)
//                    {
//                        if (_sendQueue.Count > 0)
//                            message = _sendQueue.Dequeue();
//                    }

//                    if (message != null)
//                    {
//                        byte[] messageData = message.Encode();
//                        await _stream.WriteAsync(messageData, 0, messageData.Length);
//                    }
//                    else
//                    {
//                        await Task.Delay(100, _cts.Token);
//                    }
//                }
//            }
//            catch (OperationCanceledException)
//            {
//                //正常取消
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"发送消息时发生错误: {ex.Message}");
//            }
//        }

//        public void EnqueueMessage(BTMessageEncoder message)
//        {
//            lock (_sendQueue)
//            {
//                _sendQueue.Enqueue(message);
//            }
//        }

//        private bool HasInterestingPieces()
//        {
//            if (_peerBitField == null)
//                return false;

//            for (int i = 0; i < _fileManager.PieceCount; i++)
//            {
//                if (_peerBitField[i] && !_fileManager.BitField[i])
//                    return true;
//            }
//            return false;
//        }

//        private void RequestPieces()
//        {
//            if (_isChoked || _peerBitField == null)
//                return;

//            //选择要请求的分片（简化版：选择第一个未下载的分片）
//            int pieceIndex = _fileManager.BitField.FirstFalse();
//            if (pieceIndex >= 0 && _peerBitField[pieceIndex])
//            {
//                int pieceSize = _fileManager.GetPieceSize(pieceIndex);
//                int blockSize = 16 * 1024; // 16KB块大小

//                for (int begin = 0; begin < pieceSize; begin += blockSize)
//                {
//                    int length = Math.Min(blockSize, pieceSize - begin);
//                    EnqueueMessage(new RequestMessage(pieceIndex, begin, length));
//                }
//            }
//        }

//        public void Disconnect()
//        {
//            _cts.Cancel();
//            _receiveTask?.Wait();
//            _sendTask?.Wait();
//            _stream?.Close();
//            _client?.Close();
//            Disconnected?.Invoke(this, EventArgs.Empty);
//        }

//        public void Dispose()
//        {
//            Disconnect();
//            _cts.Dispose();
//        }
//    }

//    /// <summary>
//    /// 消息接收事件参数
//    /// </summary>
//    public class MessageReceivedEventArgs : EventArgs
//    {
//        public BTMessageEncoder Message { get; }

//        public MessageReceivedEventArgs(BTMessageEncoder message)
//        {
//            Message = message;
//        }
//    }

//    /// <summary>
//    /// 数据块接收事件参数
//    /// </summary>
//    public class BlockReceivedEventArgs : EventArgs
//    {
//        public int PieceIndex { get; }
//        public int Begin { get; }
//        public int Length { get; }

//        public BlockReceivedEventArgs(int pieceIndex, int begin, int length)
//        {
//            PieceIndex = pieceIndex;
//            Begin = begin;
//            Length = length;
//        }
//    }

//}


//using System;
//using System.Buffers.Binary;
//using System.Collections.Generic;
//using System.IO;
//using System.Net;
//using System.Net.Sockets;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BitTorrentProtocol
//{
//    / <summary>
//    / UDP Tracker 客户端
//    / </summary>
//    public class UdpTrackerClient : IDisposable
//    {
//        private readonly UdpClient _client;
//        private readonly IPEndPoint _trackerEndpoint;
//        private readonly Random _random = new Random();
//        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
//        private bool _disposed;

//        UDP Tracker 协议常量
//        / <summary>
//        / 固定初始连接ID
//        / </summary>
//        private const long ConnectionId = 0x41727101980L;

//        / <summary>
//        / 连接动作
//        / </summary>
//        private const int ActionConnect = 0;

//        / <summary>
//        / 宣告动作
//        / </summary>
//        private const int ActionAnnounce = 1;

//        / <summary>
//        / 抓取动作
//        / </summary>
//        private const int ActionScrape = 2;

//        / <summary>
//        / 错误动作
//        / </summary>
//        private const int ActionError = 3;

//        超时设置
//        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(15);

//        public UdpTrackerClient(string trackerHost, int trackerPort)
//        {
//            _trackerEndpoint = new IPEndPoint(IPAddress.Parse(trackerHost), trackerPort);
//            _client = new UdpClient();
//        }

//        / <summary>
//        / 向Tracker发送Announce请求并获取Peers
//        / </summary>
//        public async Task<TrackerResponse> AnnounceAsync(
//            byte[] infoHash,
//            string peerId,
//            int port,
//            long uploaded,
//            long downloaded,
//            long left,
//            TrackerEvent @event = TrackerEvent.None)
//        {
//            if (_disposed)
//                throw new ObjectDisposedException(nameof(UdpTrackerClient));

//            1.获取连接ID
//            var connectionId = await GetConnectionIdAsync(_cts.Token);

//            2.构建Announce请求
//            var transactionId = GenerateTransactionId();
//            var announceRequest = BuildAnnounceRequest(
//                connectionId,
//                transactionId,
//                infoHash,
//                peerId,
//                port,
//                uploaded,
//                downloaded,
//                left,
//                @event);

//            3.发送请求并接收响应
//            var responseData = await SendRequestAsync(announceRequest, transactionId, _cts.Token);

//            4.解析响应
//            return ParseAnnounceResponse(responseData);
//        }

//        private async Task<long> GetConnectionIdAsync(CancellationToken ct)
//        {
//            var transactionId = GenerateTransactionId();
//            var connectRequest = BuildConnectRequest(transactionId);

//            var responseData = await SendRequestAsync(connectRequest, transactionId, ct);

//            解析连接响应
//            if (responseData.Length < 16)
//                throw new InvalidDataException("Invalid connect response");

//            var action = BinaryPrimitives.ReadInt32BigEndian(responseData.AsSpan(0, 4));
//            if (action != ActionConnect)
//                throw new InvalidDataException($"Unexpected action: {action}");

//            var responseTransactionId = BinaryPrimitives.ReadInt32BigEndian(responseData.AsSpan(4, 4));
//            if (responseTransactionId != transactionId)
//                throw new InvalidDataException($"Transaction ID mismatch: {responseTransactionId} != {transactionId}");

//            return BinaryPrimitives.ReadInt64BigEndian(responseData.AsSpan(8, 8));
//        }

//        private byte[] BuildConnectRequest(int transactionId)
//        {
//            var buffer = new byte[16];
//            BinaryPrimitives.WriteInt64BigEndian(buffer.AsSpan(0, 8), ConnectionId);
//            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(8, 4), ActionConnect);
//            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(12, 4), transactionId);
//            return buffer;
//        }

//        private byte[] BuildAnnounceRequest(
//            long connectionId,
//            int transactionId,
//            byte[] infoHash,
//            string peerId,
//            int port,
//            long uploaded,
//            long downloaded,
//            long left,
//            TrackerEvent @event)
//        {
//            if (infoHash.Length != 20)
//                throw new ArgumentException("Info hash must be 20 bytes", nameof(infoHash));

//            if (peerId.Length != 20)
//                throw new ArgumentException("Peer ID must be 20 bytes", nameof(peerId));

//            var buffer = new byte[98];
//            BinaryPrimitives.WriteInt64BigEndian(buffer.AsSpan(0, 8), connectionId);
//            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(8, 4), ActionAnnounce);
//            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(12, 4), transactionId);

//            复制infoHash
//            Array.Copy(infoHash, 0, buffer, 16, 20);

//            复制peerId
//            Array.Copy(Encoding.ASCII.GetBytes(peerId), 0, buffer, 36, 20);

//            上传 / 下载 / 剩余字节数
//            BinaryPrimitives.WriteInt64BigEndian(buffer.AsSpan(56, 8), downloaded);
//            BinaryPrimitives.WriteInt64BigEndian(buffer.AsSpan(64, 8), uploaded);
//            BinaryPrimitives.WriteInt64BigEndian(buffer.AsSpan(72, 8), left);

//            事件
//            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(80, 4), (int)@event);

//            IP地址（0表示使用发送端IP）
//            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(84, 4), 0);

//            随机数
//            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(88, 4), _random.Next());

//            下载器数（-1表示不关心）
//            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(92, 4), -1);

//            端口
//            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(96, 2), (ushort)port);

//            return buffer;
//        }

//        private async Task<byte[]> SendRequestAsync(byte[] request, int expectedTransactionId, CancellationToken ct)
//        {
//            UDP是不可靠的，需要实现重试机制
//            int retries = 3;
//            TimeSpan waitTime = TimeSpan.FromSeconds(1);

//            while (retries > 0 && !ct.IsCancellationRequested)
//            {
//                try
//                {
//                    发送请求
//                   await _client.SendAsync(request, request.Length, _trackerEndpoint);

//                    接收响应（带超时控制）
//                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
//                    timeoutCts.CancelAfter(_timeout);

//                    var receiveTask = _client.ReceiveAsync();
//                    var delayTask = Task.Delay(_timeout, timeoutCts.Token);

//                    var completedTask = await Task.WhenAny(receiveTask, delayTask);

//                    if (completedTask == receiveTask)
//                    {
//                        接收成功，获取响应数据
//                       var response = await receiveTask;
//                        var responseData = response.Buffer;

//                        验证响应长度
//                        if (responseData.Length < 8)
//                            throw new InvalidDataException("Invalid response length");

//                        验证action和transactionId
//                       var action = BinaryPrimitives.ReadInt32BigEndian(responseData.AsSpan(0, 4));
//                        var transactionId = BinaryPrimitives.ReadInt32BigEndian(responseData.AsSpan(4, 4));

//                        if (action == ActionError)
//                        {
//                            var errorMessage = Encoding.ASCII.GetString(responseData, 8, responseData.Length - 8);
//                            throw new InvalidOperationException($"Tracker error: {errorMessage}");
//                        }

//                        if (transactionId != expectedTransactionId)
//                        {
//                            忽略错误的transactionId，继续等待正确的响应
//                            continue;
//                        }

//                        return responseData;
//                    }
//                }
//                catch (OperationCanceledException) when (ct.IsCancellationRequested)
//                {
//                    throw;
//                }
//                catch (Exception ex)
//                {
//                    retries--;
//                    if (retries == 0)
//                        throw new InvalidOperationException("Failed to communicate with tracker", ex);

//                    指数退避
//                   await Task.Delay(waitTime, ct);
//                    waitTime = TimeSpan.FromSeconds(waitTime.TotalSeconds * 2);
//                }
//            }

//            throw new TimeoutException("Failed to receive response from tracker after multiple attempts");
//        }

//        private TrackerResponse ParseAnnounceResponse(byte[] responseData)
//        {
//            if (responseData.Length < 20)
//                throw new InvalidDataException("Invalid announce response");

//            var action = BinaryPrimitives.ReadInt32BigEndian(responseData.AsSpan(0, 4));
//            if (action != ActionAnnounce)
//                throw new InvalidDataException($"Unexpected action: {action}");

//            var interval = BinaryPrimitives.ReadInt32BigEndian(responseData.AsSpan(8, 4));
//            var leechers = BinaryPrimitives.ReadInt32BigEndian(responseData.AsSpan(12, 4));
//            var seeders = BinaryPrimitives.ReadInt32BigEndian(responseData.AsSpan(16, 4));

//            解析Peers列表
//           var peers = new List<Peer>();
//            var peerData = responseData.AsSpan(20);

//            每个Peer占6字节(4字节IP + 2字节端口)
//            if (peerData.Length % 6 != 0)
//                throw new InvalidDataException("Invalid peer data length");

//            for (int i = 0; i < peerData.Length; i += 6)
//            {
//                var ipBytes = peerData.Slice(i, 4).ToArray();
//                var ipAddress = new IPAddress(ipBytes);

//                var port = BinaryPrimitives.ReadUInt16BigEndian(peerData.Slice(i + 4, 2));

//                peers.Add(new Peer
//                {
//                    Ip = ipAddress.ToString(),
//                    Port = port
//                });
//            }

//            return new TrackerResponse
//            {
//                Interval = interval,
//                Complete = seeders,
//                Incomplete = leechers,
//                Peers = peers
//            };
//        }

//        private int GenerateTransactionId()
//        {
//            return _random.Next();
//        }

//        public void Dispose()
//        {
//            if (_disposed)
//                return;

//            _cts.Cancel();
//            _client.Dispose();
//            _disposed = true;
//        }
//    }

//    / <summary>
//    / Tracker事件类型
//    / </summary>
//    public enum TrackerEvent
//    {
//        None = 0,
//        Completed = 1,
//        Started = 2,
//        Stopped = 3
//    }

//    / <summary>
//    / Tracker响应
//    / </summary>
//    public class TrackerResponse
//    {
//        public int Interval { get; set; } // 下次请求间隔(秒)
//        public int Complete { get; set; } // 种子数
//        public int Incomplete { get; set; } // 下载者数
//        public List<Peer> Peers { get; set; } = new List<Peer>();
//    }

//    / <summary>
//    / Peer信息
//    / </summary>
//    public class Peer
//    {
//        public string Ip { get; set; }
//        public int Port { get; set; }
//    }
//}

//using System;
//using System.Buffers.Binary;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using ExtenderApp.Common.Networks;
//using ExtenderApp.Torrent;

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;

//namespace BitTorrentProtocol
//{
//    / <summary>
//    / 种子文件（.torrent）处理类
//    / </summary>
//    public class TorrentFile
//    {
//        文件元数据
//        / <summary>
//        / 获取Torrent文件的评论。
//        / </summary>
//        / <value>包含评论的字符串。</value>
//        public string Comment { get; private set; }

//        / <summary>
//        / 获取创建Torrent文件的程序或工具的名称。
//        / </summary>
//        / <value>包含创建程序名称的字符串。</value>
//        public string CreatedBy { get; private set; }

//        / <summary>
//        / 获取Torrent文件的创建日期。
//        / </summary>
//        / <value>表示创建日期的DateTime对象。</value>
//        public DateTime CreationDate { get; private set; }

//        信息字典

//        / <summary>
//        / 名称
//        / </summary>
//        public string Name { get; private set; }

//        / <summary>
//        / 分片长度
//        / </summary>
//        public long PieceLength { get; private set; }

//        / <summary>
//        / 分片数组
//        / </summary>
//        public byte[] Pieces { get; private set; }

//        / <summary>
//        / 是否为单文件
//        / </summary>
//        / <returns>如果文件长度大于0，则为单文件，返回true；否则为false</returns>
//        public bool IsSingleFile => FileLength > 0;

//        / <summary>
//        / 文件长度
//        / </summary>
//        public long FileLength { get; private set; }

//        / <summary>
//        / 获取Torrent文件信息列表
//        / </summary>
//        public List<TorrentFileInfo> Files { get; private set; }

//        InfoHash(20字节)
//        / <summary>
//        / 种子哈希值
//        / </summary>
//        public InfoHash Hash { get; private set; }

//        / <summary>
//        / 从URL下载并解析种子文件
//        / </summary>
//        public static async Task<TorrentFile> FromUrlAsync(string torrentUrl)
//        {
//            using var httpClient = new HttpClient();
//            var torrentData = await httpClient.GetByteArrayAsync(torrentUrl);
//            return Parse(torrentData);
//        }

//        / <summary>
//        / 从本地文件解析种子文件
//        / </summary>
//        public static TorrentFile FromFile(string filePath)
//        {
//            var torrentData = File.ReadAllBytes(filePath);
//            return Parse(torrentData);
//        }

//        / <summary>
//        / 解析种子文件内容
//        / </summary>
//        private static TorrentFile Parse(byte[] torrentData)
//        {
//            解析B编码数据
//           var decoder = new BencodeDecoder();
//            var torrentDict = decoder.Decode(torrentData) as Dictionary<string, object>;

//            if (torrentDict == null)
//                throw new InvalidDataException("Invalid torrent file format");

//            var torrent = new TorrentFile();

//            解析基本信息
//            if (torrentDict.TryGetValue("announce", out var announceObj))
//                torrent.Announce = announceObj.ToString();

//            if (torrentDict.TryGetValue("comment", out var commentObj))
//                torrent.Comment = commentObj.ToString();

//            if (torrentDict.TryGetValue("created by", out var createdByObj))
//                torrent.CreatedBy = createdByObj.ToString();

//            if (torrentDict.TryGetValue("creation date", out var creationDateObj))
//            {
//                var unixTime = Convert.ToInt64(creationDateObj);
//                torrent.CreationDate = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
//            }

//            解析announce - list
//            torrent.AnnounceList = new List<string>();
//            if (torrentDict.TryGetValue("announce-list", out var announceListObj))
//            {
//                var tierList = announceListObj as List<object>;
//                if (tierList != null)
//                {
//                    foreach (var tier in tierList)
//                    {
//                        var urls = tier as List<object>;
//                        if (urls != null)
//                        {
//                            foreach (var url in urls)
//                            {
//                                torrent.AnnounceList.Add(url.ToString());
//                            }
//                        }
//                    }
//                }
//            }

//            解析info字典
//            if (!torrentDict.TryGetValue("info", out var infoObj))
//                throw new InvalidDataException("Missing 'info' dictionary in torrent file");

//            var infoDict = infoObj as Dictionary<string, object>;
//            if (infoDict == null)
//                throw new InvalidDataException("Invalid 'info' dictionary format");

//            计算info_hash(SHA - 1 of info dict)
//            using var sha1 = SHA1.Create();
//            var infoBytes = EncodeInfoDictionary(infoDict);
//            torrent.Hash = sha1.ComputeHash(infoBytes);

//            解析info字典内容
//            if (infoDict.TryGetValue("name", out var nameObj))
//                torrent.Name = nameObj.ToString();

//            if (infoDict.TryGetValue("piece length", out var pieceLengthObj))
//                torrent.PieceLength = Convert.ToInt64(pieceLengthObj);

//            if (infoDict.TryGetValue("pieces", out var piecesObj))
//                torrent.Pieces = piecesObj as byte[];

//            判断单文件 / 多文件模式
//            torrent.IsSingleFile = infoDict.ContainsKey("length");

//            if (torrent.IsSingleFile)
//            {
//                单文件模式
//                if (infoDict.TryGetValue("length", out var lengthObj))
//                    torrent.FileLength = Convert.ToInt64(lengthObj);
//            }
//            else
//            {
//                多文件模式
//                torrent.Files = new List<TorrentFileInfo>();
//                if (infoDict.TryGetValue("files", out var filesObj))
//                {
//                    var filesList = filesObj as List<object>;
//                    if (filesList != null)
//                    {
//                        foreach (var fileObj in filesList)
//                        {
//                            var fileDict = fileObj as Dictionary<string, object>;
//                            if (fileDict != null)
//                            {
//                                var fileInfo = new TorrentFileInfo();

//                                if (fileDict.TryGetValue("length", out var lengthObj))
//                                    fileInfo.Length = Convert.ToInt64(lengthObj);

//                                if (fileDict.TryGetValue("path", out var pathObj))
//                                {
//                                    var pathList = pathObj as List<object>;
//                                    if (pathList != null)
//                                    {
//                                        fileInfo.Path = pathList.Select(p => p.ToString()).ToList();
//                                        fileInfo.FullPath = string.Join(Path.DirectorySeparatorChar, fileInfo.Path);
//                                    }
//                                }

//                                torrent.Files.Add(fileInfo);
//                            }
//                        }
//                    }
//                }
//            }

//            return torrent;
//        }

//        / <summary>
//        / 重新编码info字典用于计算InfoHash
//        / </summary>
//        private static byte[] EncodeInfoDictionary(Dictionary<string, object> infoDict)
//        {
//            var encoder = new BencodeEncoder();
//            return encoder.Encode(infoDict);
//        }
//    }

//    / <summary>
//    / 种子文件中的文件信息
//    / </summary>
//    public class TorrentFileInfo
//    {
//        public long Length { get; set; }
//        public List<string> Path { get; set; }
//        public string FullPath { get; set; }
//    }

//    / <summary>
//    / B编码解码器（简化实现）
//    / </summary>
//    internal class BencodeDecoder
//    {
//        private int _position;

//        public object Decode(byte[] data)
//        {
//            _position = 0;
//            return DecodeValue(data);
//        }

//        private object DecodeValue(byte[] data)
//        {
//            if (_position >= data.Length)
//                return null;

//            char type = (char)data[_position];

//            switch (type)
//            {
//                case 'd': // 字典
//                    return DecodeDictionary(data);
//                case 'l': // 列表
//                    return DecodeList(data);
//                case 'i': // 整数
//                    return DecodeInteger(data);
//                case '0':
//                case '1':
//                case '2':
//                case '3':
//                case '4':
//                case '5':
//                case '6':
//                case '7':
//                case '8':
//                case '9': // 字符串
//                    return DecodeString(data);
//                default:
//                    throw new InvalidDataException($"Invalid Bencode type: {type}");
//            }
//        }

//        private string DecodeString(byte[] data)
//        {
//            int colonPos = Array.IndexOf(data, (byte)':', _position);
//            if (colonPos == -1)
//                throw new InvalidDataException("Missing colon in string");

//            int length = int.Parse(Encoding.ASCII.GetString(data, _position, colonPos - _position));
//            _position = colonPos + 1;

//            string result = Encoding.UTF8.GetString(data, _position, length);
//            _position += length;
//            return result;
//        }

//        private long DecodeInteger(byte[] data)
//        {
//            _position++; // 跳过 'i'
//            int endPos = Array.IndexOf(data, (byte)'e', _position);
//            if (endPos == -1)
//                throw new InvalidDataException("Missing 'e' in integer");

//            long result = long.Parse(Encoding.ASCII.GetString(data, _position, endPos - _position));
//            _position = endPos + 1;
//            return result;
//        }

//        private List<object> DecodeList(byte[] data)
//        {
//            _position++; // 跳过 'l'
//            var list = new List<object>();

//            while ((char)data[_position] != 'e')
//            {
//                list.Add(DecodeValue(data));
//            }

//            _position++; // 跳过 'e'
//            return list;
//        }

//        private Dictionary<string, object> DecodeDictionary(byte[] data)
//        {
//            _position++; // 跳过 'd'
//            var dict = new Dictionary<string, object>();

//            while ((char)data[_position] != 'e')
//            {
//                string key = DecodeString(data);
//                object value = DecodeValue(data);
//                dict[key] = value;
//            }

//            _position++; // 跳过 'e'
//            return dict;
//        }
//    }

//    / <summary>
//    / B编码编码器（简化实现）
//    / </summary>
//    internal class BencodeEncoder
//    {
//        public byte[] Encode(object value)
//        {
//            using var stream = new MemoryStream();
//            EncodeValue(stream, value);
//            return stream.ToArray();
//        }

//        private void EncodeValue(Stream stream, object value)
//        {
//            if (value is string str)
//            {
//                EncodeString(stream, str);
//            }
//            else if (value is int || value is long)
//            {
//                EncodeInteger(stream, Convert.ToInt64(value));
//            }
//            else if (value is List<object> list)
//            {
//                EncodeList(stream, list);
//            }
//            else if (value is Dictionary<string, object> dict)
//            {
//                EncodeDictionary(stream, dict);
//            }
//            else if (value is byte[] bytes)
//            {
//                EncodeBytes(stream, bytes);
//            }
//            else
//            {
//                throw new NotSupportedException($"Unsupported type: {value.GetType()}");
//            }
//        }

//        private void EncodeString(Stream stream, string value)
//        {
//            var bytes = Encoding.UTF8.GetBytes(value);
//            EncodeBytes(stream, bytes);
//        }

//        private void EncodeBytes(Stream stream, byte[] bytes)
//        {
//            var lengthBytes = Encoding.ASCII.GetBytes(bytes.Length.ToString());
//            stream.Write(lengthBytes, 0, lengthBytes.Length);
//            stream.WriteByte((byte)':');
//            stream.Write(bytes, 0, bytes.Length);
//        }

//        private void EncodeInteger(Stream stream, long value)
//        {
//            stream.WriteByte((byte)'i');
//            var bytes = Encoding.ASCII.GetBytes(value.ToString());
//            stream.Write(bytes, 0, bytes.Length);
//            stream.WriteByte((byte)'e');
//        }

//        private void EncodeList(Stream stream, List<object> list)
//        {
//            stream.WriteByte((byte)'l');
//            foreach (var item in list)
//            {
//                EncodeValue(stream, item);
//            }
//            stream.WriteByte((byte)'e');
//        }

//        private void EncodeDictionary(Stream stream, Dictionary<string, object> dict)
//        {
//            stream.WriteByte((byte)'d');
//            foreach (var kvp in dict.OrderBy(kvp => kvp.Key))
//            {
//                EncodeString(stream, kvp.Key);
//                EncodeValue(stream, kvp.Value);
//            }
//            stream.WriteByte((byte)'e');
//        }
//    }
//}

//namespace BitTorrentProtocol
//{


//    / <summary>
//    / BitTorrent下载器
//    / </summary>
//    public class TorrentDownloader : IDisposable
//    {
//        private readonly TorrentFile _torrent;
//        private readonly string _downloadPath;
//        private readonly string _peerId;
//        private readonly UdpTrackerClient _trackerClient;
//        private readonly List<PeerConnection> _peers = new List<PeerConnection>();
//        private readonly Dictionary<int, Piece> _pieces = new Dictionary<int, Piece>();
//        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
//        private readonly object _lock = new object();

//        统计信息
//        public long DownloadedBytes { get; private set; }
//        public long UploadedBytes { get; private set; }
//        public int ConnectedPeers => _peers.Count(p => p.IsConnected);

//        public TorrentDownloader(TorrentFile torrent, string downloadPath)
//        {
//            _torrent = torrent;
//            _downloadPath = downloadPath;
//            _peerId = GeneratePeerId();
//            _trackerClient = new UdpTrackerClient(
//                new Uri(_torrent.Announce).Host,
//                new Uri(_torrent.Announce).Port);

//            初始化分片信息
//            InitializePieces();

//            创建下载目录
//            EnsureDirectoryExists();
//        }

//        生成唯一的Peer ID
//        private string GeneratePeerId()
//        {
//        格式: -< 客户端ID >< 版本 > -< 随机数 >
//            return $"-BT1000-{Guid.NewGuid().ToString("N").Substring(0, 12)}";
//        }

//        初始化分片信息
//        private void InitializePieces()
//        {
//            int pieceCount = _torrent.Pieces.Length / 20; // 每个哈希20字节

//            for (int i = 0; i < pieceCount; i++)
//            {
//                byte[] hash = new byte[20];
//                Array.Copy(_torrent.Pieces, i * 20, hash, 0, 20);

//                最后一个分片可能小于标准大小
//                long pieceSize = (i == pieceCount - 1)
//                    ? (_torrent.IsSingleFile ? _torrent.FileLength : _torrent.Files.Sum(f => f.Length)) - (long)i * _torrent.PieceLength
//                    : _torrent.PieceLength;

//                _pieces[i] = new Piece
//                {
//                    Index = i,
//                    Size = pieceSize,
//                    Hash = hash,
//                    Blocks = new List<Block>(),
//                    State = PieceState.Missing
//                };

//                初始化块信息
//                InitializeBlocks(_pieces[i]);
//            }
//        }

//        初始化分片内的块信息
//        private void InitializeBlocks(Piece piece)
//        {
//            const int blockSize = 16 * 1024; // 16KB
//            int blockCount = (int)Math.Ceiling((double)piece.Size / blockSize);

//            for (int i = 0; i < blockCount; i++)
//            {
//                long currentBlockSize = (i == blockCount - 1)
//                    ? piece.Size - (long)i * blockSize
//                    : blockSize;

//                piece.Blocks.Add(new Block
//                {
//                    PieceIndex = piece.Index,
//                    Begin = (long)i * blockSize,
//                    Length = currentBlockSize,
//                    State = BlockState.Missing
//                });
//            }
//        }

//        确保下载目录存在
//        private void EnsureDirectoryExists()
//        {
//            if (_torrent.IsSingleFile)
//            {
//                string directory = Path.GetDirectoryName(_downloadPath);
//                if (!Directory.Exists(directory))
//                {
//                    Directory.CreateDirectory(directory);
//                }
//            }
//            else
//            {
//                if (!Directory.Exists(_downloadPath))
//                {
//                    Directory.CreateDirectory(_downloadPath);
//                }
//            }
//        }

//        / <summary>
//        / 开始下载
//        / </summary>
//        public async Task StartDownloadAsync()
//        {
//            try
//            {
//                1.连接Tracker获取Peer列表
//                var peers = await GetPeersFromTrackersAsync(_cts.Token);

//                2.连接到Peers
//                await ConnectToPeersAsync(peers, _cts.Token);

//                3.启动下载监控
//                StartDownloadMonitoring(_cts.Token);

//                4.等待下载完成
//                await WaitForCompletion(_cts.Token);
//            }
//            catch (OperationCanceledException)
//            {
//                Console.WriteLine("下载已取消");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"下载错误: {ex.Message}");
//            }
//        }

//        / <summary>
//        / 从所有Tracker获取Peers
//        / </summary>
//        private async Task<List<Peer>> GetPeersFromTrackersAsync(CancellationToken ct)
//        {
//            var allPeers = new List<Peer>();

//            优先使用主Tracker
//            try
//            {
//                var response = await _trackerClient.AnnounceAsync(
//                    _torrent.Hash,
//                    _peerId,
//                    6881, // 监听端口
//                    UploadedBytes,
//                    DownloadedBytes,
//                    GetRemainingBytes(),
//                    TrackerEvent.Started);

//                allPeers.AddRange(response.Peers);
//                Console.WriteLine($"从主Tracker获取了 {response.Peers.Count} 个Peers");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"主Tracker连接失败: {ex.Message}");
//            }

//            尝试从备用Trackers获取
//            foreach (var trackerUrl in _torrent.AnnounceList.Skip(1))
//            {
//                try
//                {
//                    if (ct.IsCancellationRequested) break;

//                    var trackerClient = new UdpTrackerClient(
//                        new Uri(trackerUrl).Host,
//                        new Uri(trackerUrl).Port);

//                    var response = await trackerClient.AnnounceAsync(
//                        _torrent.Hash,
//                        _peerId,
//                        6881,
//                        UploadedBytes,
//                        DownloadedBytes,
//                        GetRemainingBytes(),
//                        TrackerEvent.Started);

//                    allPeers.AddRange(response.Peers);
//                    Console.WriteLine($"从备用Tracker {trackerUrl} 获取了 {response.Peers.Count} 个Peers");
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"备用Tracker {trackerUrl} 连接失败: {ex.Message}");
//                }
//            }

//            去重
//            return allPeers.DistinctBy(p => $"{p.Ip}:{p.Port}").ToList();
//        }

//        / <summary>
//        / 连接到多个Peer
//        / </summary>
//        private async Task ConnectToPeersAsync(List<Peer> peers, CancellationToken ct)
//        {
//            限制同时连接的Peer数量
//            const int maxConnections = 50;
//            var connectionTasks = new List<Task>();

//            foreach (var peer in peers.Take(maxConnections))
//            {
//                if (ct.IsCancellationRequested) break;

//                connectionTasks.Add(Task.Run(async () =>
//                {
//                    try
//                    {
//                        var peerConnection = new PeerConnection(peer, _torrent.Hash, _peerId);
//                        await peerConnection.ConnectAsync(ct);

//                        lock (_lock)
//                        {
//                            if (peerConnection.IsConnected)
//                            {
//                                _peers.Add(peerConnection);
//                                Console.WriteLine($"成功连接到Peer: {peer.Ip}:{peer.Port}");

//                                开始与Peer交换数据
//                                StartPeerCommunication(peerConnection, ct);
//                            }
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        Console.WriteLine($"连接Peer {peer.Ip}:{peer.Port} 失败: {ex.Message}");
//                    }
//                }, ct));
//            }

//            await Task.WhenAll(connectionTasks);
//        }

//        / <summary>
//        / 开始与Peer的通信
//        / </summary>
//        private async void StartPeerCommunication(PeerConnection peer, CancellationToken ct)
//        {
//            try
//            {
//                1.发送感兴趣消息
//                await peer.SendInterestedAsync(ct);

//                2.处理接收到的消息
//                while (peer.IsConnected && !ct.IsCancellationRequested)
//                {
//                    var message = await peer.ReceiveMessageAsync(ct);
//                    if (message == null) continue;

//                    处理不同类型的消息
//                   await ProcessPeerMessage(peer, message, ct);
//                }
//            }
//            catch (OperationCanceledException)
//            {
//                正常取消
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"与Peer {peer.PeerInfo.Ip}:{peer.PeerInfo.Port} 通信错误: {ex.Message}");
//                peer.Disconnect();
//            }
//        }

//        / <summary>
//        / 处理来自Peer的消息
//        / </summary>
//        private async Task ProcessPeerMessage(PeerConnection peer, PeerMessage message, CancellationToken ct)
//        {
//            switch (message.MessageId)
//            {
//                case PeerMessageId.Choke:
//                    peer.IsChoking = true;
//                    Console.WriteLine($"Peer {peer.PeerInfo.Ip}:{peer.PeerInfo.Port} 已阻塞我们");
//                    break;

//                case PeerMessageId.Unchoke:
//                    peer.IsChoking = false;
//                    Console.WriteLine($"Peer {peer.PeerInfo.Ip}:{peer.PeerInfo.Port} 已解除阻塞");

//                    请求块
//                    RequestBlocksFromPeer(peer, ct);
//                    break;

//                case PeerMessageId.Interested:
//                    peer.IsInterested = true;
//                    Console.WriteLine($"Peer {peer.PeerInfo.Ip}:{peer.PeerInfo.Port} 对我们感兴趣");
//                    break;

//                case PeerMessageId.NotInterested:
//                    peer.IsInterested = false;
//                    Console.WriteLine($"Peer {peer.PeerInfo.Ip}:{peer.PeerInfo.Port} 对我们不感兴趣");
//                    break;

//                case PeerMessageId.Have:
//                    var haveMessage = message as HaveMessage;
//                    if (haveMessage != null)
//                    {
//                        peer.HasPiece(haveMessage.PieceIndex);
//                        Console.WriteLine($"Peer {peer.PeerInfo.Ip}:{peer.PeerInfo.Port} 有分片 {haveMessage.PieceIndex}");

//                        如果我们需要这个分片，发送感兴趣消息
//                        if (!IsPieceComplete(haveMessage.PieceIndex))
//                        {
//                            await peer.SendInterestedAsync(ct);
//                        }
//                    }
//                    break;

//                case PeerMessageId.Bitfield:
//                    var bitfieldMessage = message as BitfieldMessage;
//                    if (bitfieldMessage != null)
//                    {
//                        peer.SetBitfield(bitfieldMessage.Bitfield);
//                        Console.WriteLine($"从Peer {peer.PeerInfo.Ip}:{peer.PeerInfo.Port} 接收Bitfield");

//                        请求块
//                        RequestBlocksFromPeer(peer, ct);
//                    }
//                    break;

//                case PeerMessageId.Piece:
//                    var pieceMessage = message as PieceMessage;
//                    if (pieceMessage != null)
//                    {
//                        处理接收到的数据块
//                       await HandleReceivedPiece(peer, pieceMessage, ct);
//                    }
//                    break;

//                case PeerMessageId.Request:
//                    处理来自Peer的请求（上传逻辑）
//                    var requestMessage = message as RequestMessage;
//                    if (requestMessage != null && !peer.IsChoking)
//                    {
//                        await SendBlockToPeer(peer, requestMessage, ct);
//                    }
//                    break;

//                case PeerMessageId.Cancel:
//                    处理取消请求
//                    break;
//            }
//        }

//        / <summary>
//        / 从Peer请求数据块
//        / </summary>
//        private void RequestBlocksFromPeer(PeerConnection peer, CancellationToken ct)
//        {
//            if (peer.IsChoking || !peer.IsConnected) return;

//            lock (_lock)
//            {
//                选择要请求的分片和块
//               var blockToRequest = SelectBlockToRequest(peer);
//                if (blockToRequest != null)
//                {
//                    try
//                    {
//                        标记为正在下载
//                        blockToRequest.State = BlockState.Downloading;

//                        发送请求
//                        peer.SendRequestAsync(blockToRequest.PieceIndex, blockToRequest.Begin, blockToRequest.Length, ct)
//                            .ContinueWith(t =>
//                            {
//                                if (t.IsFaulted)
//                                {
//                                    Console.WriteLine($"请求块失败: {t.Exception.Message}");
//                                    blockToRequest.State = BlockState.Missing;
//                                }
//                            }, ct);
//                    }
//                    catch (Exception ex)
//                    {
//                        Console.WriteLine($"请求块异常: {ex.Message}");
//                        blockToRequest.State = BlockState.Missing;
//                    }
//                }
//            }
//        }

//        / <summary>
//        / 选择要请求的块（基于稀有度优先）
//        / </summary>
//        private Block SelectBlockToRequest(PeerConnection peer)
//        {
//            1.查找我们没有的且Peer有的分片
//            foreach (var piece in _pieces.Values)
//            {
//                if (piece.State == PieceState.Complete) continue;
//                if (!peer.HasPiece(piece.Index)) continue;

//                2.查找该分片下未下载的块
//                var missingBlock = piece.Blocks.FirstOrDefault(b => b.State == BlockState.Missing);
//                if (missingBlock != null)
//                {
//                    return missingBlock;
//                }
//            }

//            return null;
//        }

//        / <summary>
//        / 处理接收到的分片数据块
//        / </summary>
//        private async Task HandleReceivedPiece(PeerConnection peer, PieceMessage message, CancellationToken ct)
//        {
//            lock (_lock)
//            {
//                查找对应的块
//                if (!_pieces.TryGetValue(message.PieceIndex, out var piece))
//                {
//                    Console.WriteLine($"收到未知分片 {message.PieceIndex} 的数据");
//                    return;
//                }

//                var block = piece.Blocks.FirstOrDefault(b =>
//                    b.Begin == message.Begin &&
//                    b.Length == message.Block.Length);

//                if (block == null)
//                {
//                    Console.WriteLine($"收到未知块 (分片 {message.PieceIndex}, 偏移 {message.Begin}) 的数据");
//                    return;
//                }

//                更新块状态
//                block.State = BlockState.Completed;
//                block.Data = message.Block;

//                更新下载统计
//               DownloadedBytes += message.Block.Length;

//                Console.WriteLine($"从Peer {peer.PeerInfo.Ip}:{peer.PeerInfo.Port} 下载块: 分片 {message.PieceIndex}, 偏移 {message.Begin}, 大小 {message.Block.Length}");
//            }

//            检查分片是否完整
//           await CheckPieceCompletion(message.PieceIndex, ct);

//            请求更多块
//            RequestBlocksFromPeer(peer, ct);
//        }

//        / <summary>
//        / 检查分片是否完成下载并验证
//        / </summary>
//        private async Task CheckPieceCompletion(int pieceIndex, CancellationToken ct)
//        {
//            lock (_lock)
//            {
//                if (!_pieces.TryGetValue(pieceIndex, out var piece))
//                    return;

//                检查所有块是否都已下载
//                if (piece.Blocks.All(b => b.State == BlockState.Completed))
//                {
//                    合并所有块的数据
//                   var pieceData = new byte[piece.Size];
//                    int offset = 0;

//                    foreach (var block in piece.Blocks.OrderBy(b => b.Begin))
//                    {
//                        Array.Copy(block.Data, 0, pieceData, offset, block.Length);
//                        offset += (int)block.Length;
//                    }

//                    验证哈希
//                    using var sha1 = SHA1.Create();
//                    var computedHash = sha1.ComputeHash(pieceData);

//                    if (computedHash.SequenceEqual(piece.Hash))
//                    {
//                        piece.State = PieceState.Complete;
//                        Console.WriteLine($"分片 {pieceIndex} 验证通过");

//                        保存到文件
//                        SavePieceToFile(pieceIndex, pieceData);

//                        向所有Peer广播我们拥有这个分片
//                        BroadcastHaveMessage(pieceIndex, ct);
//                    }
//                    else
//                    {
//                        piece.State = PieceState.Missing;
//                        重置所有块状态
//                        foreach (var block in piece.Blocks)
//                        {
//                            block.State = BlockState.Missing;
//                            block.Data = null;
//                        }
//                        Console.WriteLine($"分片 {pieceIndex} 哈希验证失败，将重新下载");
//                    }
//                }
//            }
//        }

//        / <summary>
//        / 将分片保存到文件
//        / </summary>
//        private void SavePieceToFile(int pieceIndex, byte[] pieceData)
//        {
//            try
//            {
//                if (_torrent.IsSingleFile)
//                {
//                    单文件模式
//                    using var stream = new FileStream(_downloadPath, FileMode.OpenOrCreate, FileAccess.Write);
//                    long offset = (long)pieceIndex * _torrent.PieceLength;
//                    stream.Seek(offset, SeekOrigin.Begin);
//                    stream.Write(pieceData, 0, pieceData.Length);
//                }
//                else
//                {
//                    多文件模式
//                    long pieceOffset = (long)pieceIndex * _torrent.PieceLength;
//                    long remaining = pieceData.Length;
//                    int dataOffset = 0;

//                    foreach (var file in _torrent.Files)
//                    {
//                        if (remaining <= 0) break;

//                        如果文件偏移量大于分片偏移量，跳过
//                        if (file.Length <= pieceOffset)
//                        {
//                            pieceOffset -= file.Length;
//                            continue;
//                        }

//                        计算要写入的字节数
//                        long writeLength = Math.Min(remaining, file.Length - pieceOffset);

//                        构建完整文件路径
//                        string filePath = Path.Combine(_downloadPath, file.FullPath);
//                        string directory = Path.GetDirectoryName(filePath);

//                        确保目录存在
//                        if (!Directory.Exists(directory))
//                        {
//                            Directory.CreateDirectory(directory);
//                        }

//                        写入文件
//                        using var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
//                        stream.Seek(pieceOffset, SeekOrigin.Begin);
//                        stream.Write(pieceData, dataOffset, (int)writeLength);

//                        remaining -= writeLength;
//                        dataOffset += (int)writeLength;
//                        pieceOffset = 0;
//                    }
//                }

//                Console.WriteLine($"分片 {pieceIndex} 已保存到文件");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"保存分片 {pieceIndex} 失败: {ex.Message}");
//            }
//        }

//        / <summary>
//        / 向所有Peer广播我们拥有某个分片
//        / </summary>
//        private void BroadcastHaveMessage(int pieceIndex, CancellationToken ct)
//        {
//            lock (_lock)
//            {
//                foreach (var peer in _peers.Where(p => p.IsConnected))
//                {
//                    try
//                    {
//                        peer.SendHaveAsync(pieceIndex, ct)
//                            .ContinueWith(t =>
//                            {
//                                if (t.IsFaulted)
//                                {
//                                    Console.WriteLine($"向Peer {peer.PeerInfo.Ip}:{peer.Port} 发送Have消息失败: {t.Exception.Message}");
//                                }
//                            }, ct);
//                    }
//                    catch (Exception ex)
//                    {
//                        Console.WriteLine($"广播Have消息异常: {ex.Message}");
//                    }
//                }
//            }
//        }

//        / <summary>
//        / 响应Peer的块请求（上传逻辑）
//        / </summary>
//        private async Task SendBlockToPeer(PeerConnection peer, RequestMessage request, CancellationToken ct)
//        {
//            try
//            {
//                lock (_lock)
//                {
//                    if (!_pieces.TryGetValue(request.PieceIndex, out var piece) ||
//                        piece.State != PieceState.Complete)
//                    {
//                        return; // 我们没有这个分片
//                    }

//                    从文件中读取块数据
//                    byte[] blockData = new byte[request.Length];

//                    if (_torrent.IsSingleFile)
//                    {
//                        using var stream = new FileStream(_downloadPath, FileMode.Open, FileAccess.Read);
//                        long offset = (long)request.PieceIndex * _torrent.PieceLength + request.Begin;
//                        stream.Seek(offset, SeekOrigin.Begin);
//                        stream.Read(blockData, 0, (int)request.Length);
//                    }
//                    else
//                    {
//                        多文件模式下的读取逻辑（类似SavePieceToFile的反向操作）
//                         此处简化处理，实际实现需要根据分片和文件的映射关系读取
//                    }

//                    发送数据块
//                    peer.SendPieceAsync(request.PieceIndex, request.Begin, blockData, ct)
//                        .ContinueWith(t =>
//                        {
//                            if (t.IsCompletedSuccessfully)
//                            {
//                                UploadedBytes += request.Length;
//                                Console.WriteLine($"向Peer {peer.PeerInfo.Ip}:{peer.Port} 上传块: 分片 {request.PieceIndex}, 偏移 {request.Begin}, 大小 {request.Length}");
//                            }
//                        }, ct);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"向Peer {peer.PeerInfo.Ip}:{peer.Port} 发送块失败: {ex.Message}");
//            }
//        }

//        / <summary>
//        / 获取剩余需要下载的字节数
//        / </summary>
//        private long GetRemainingBytes()
//        {
//            long totalSize = _torrent.IsSingleFile ? _torrent.FileLength : _torrent.Files.Sum(f => f.Length);
//            return totalSize - DownloadedBytes;
//        }

//        / <summary>
//        / 检查分片是否已完成下载
//        / </summary>
//        private bool IsPieceComplete(int pieceIndex)
//        {
//            return _pieces.TryGetValue(pieceIndex, out var piece) &&
//                   piece.State == PieceState.Complete;
//        }

//        / <summary>
//        / 启动下载监控
//        / </summary>
//        private void StartDownloadMonitoring(CancellationToken ct)
//        {
//            Task.Run(async () =>
//            {
//                try
//                {
//                    while (!ct.IsCancellationRequested)
//                    {
//                        await Task.Delay(5000, ct); // 每5秒更新一次

//                        double progress = (double)DownloadedBytes /
//                            (_torrent.IsSingleFile ? _torrent.FileLength : _torrent.Files.Sum(f => f.Length)) * 100;

//                        Console.WriteLine($"下载进度: {progress:F2}%, 已连接Peer: {ConnectedPeers}, 下载速度: {GetDownloadSpeed()} KB/s");
//                    }
//                }
//                catch (OperationCanceledException)
//                {
//                    正常取消
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"下载监控异常: {ex.Message}");
//                }
//            }, ct);
//        }

//        / <summary>
//        / 计算下载速度（KB/s）
//        / </summary>
//        private double GetDownloadSpeed()
//        {
//            实际实现需要记录时间窗口内的下载量变化
//            此处简化处理
//            return new Random().NextDouble() * 1000; // 示例值
//        }

//        / <summary>
//        / 等待下载完成
//        / </summary>
//        private async Task WaitForCompletion(CancellationToken ct)
//        {
//            while (!ct.IsCancellationRequested)
//            {
//                lock (_lock)
//                {
//                    if (_pieces.Values.All(p => p.State == PieceState.Complete))
//                    {
//                        Console.WriteLine("下载完成!");

//                        通知Tracker下载完成
//                        NotifyTrackerDownloadCompleted(ct);
//                        return;
//                    }
//                }

//                await Task.Delay(1000, ct);
//            }
//        }

//        / <summary>
//        / 通知Tracker下载完成
//        / </summary>
//        private async void NotifyTrackerDownloadCompleted(CancellationToken ct)
//        {
//            try
//            {
//                await _trackerClient.AnnounceAsync(
//                    _torrent.Hash,
//                    _peerId,
//                    6881,
//                    UploadedBytes,
//                    DownloadedBytes,
//                    0, // 剩余字节数为0，表示完成
//                    TrackerEvent.Completed);

//                Console.WriteLine("已通知Tracker下载完成");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"通知Tracker失败: {ex.Message}");
//            }
//        }

//        / <summary>
//        / 停止下载
//        / </summary>
//        public void StopDownload()
//        {
//            _cts.Cancel();

//            断开所有Peer连接
//            lock (_lock)
//            {
//                foreach (var peer in _peers)
//                {
//                    peer.Disconnect();
//                }
//                _peers.Clear();
//            }

//            Console.WriteLine("下载已停止");
//        }

//        public void Dispose()
//        {
//            _cts.Cancel();
//            _cts.Dispose();

//            lock (_lock)
//            {
//                foreach (var peer in _peers)
//                {
//                    peer.Dispose();
//                }
//                _peers.Clear();
//            }

//            _trackerClient.Dispose();
//        }
//    }

//    分片状态枚举
//    public enum PieceState
//    {
//        Missing,    // 未下载
//        Downloading, // 正在下载
//        Complete    // 已完成
//    }

//    块状态枚举
//    public enum BlockState
//    {
//        Missing,    // 未下载
//        Downloading, // 正在下载
//        Completed   // 已完成
//    }

//    分片信息
//    public class Piece
//    {
//        public int Index { get; set; }
//        public long Size { get; set; }
//        public byte[] Hash { get; set; }
//        public List<Block> Blocks { get; set; }
//        public PieceState State { get; set; }
//    }

//    块信息
//    public class Block
//    {
//        public int PieceIndex { get; set; }
//        public long Begin { get; set; }
//        public long Length { get; set; }
//        public BlockState State { get; set; }
//        public byte[] Data { get; set; }
//    }

//    Peer连接类（简化实现）
//    public class PeerConnection : IDisposable
//    {
//        private readonly TcpClient _client;
//        private readonly NetworkStream _stream;
//        private readonly Peer _peerInfo;
//        private readonly byte[] _infoHash;
//        private readonly string _peerId;
//        private readonly bool[] _bitfield;

//        public Peer PeerInfo => _peerInfo;
//        public int Port => _client.Client.LocalEndPoint is IPEndPoint ep ? ep.Port : 0;
//        public bool IsConnected { get; private set; }
//        public bool IsChoking { get; set; } = true;
//        public bool IsInterested { get; set; } = false;

//        public PeerConnection(Peer peerInfo, byte[] infoHash, string peerId)
//        {
//            _peerInfo = peerInfo;
//            _infoHash = infoHash;
//            _peerId = peerId;
//            _client = new TcpClient();
//            _bitfield = new bool[1024]; // 假设最多1024个分片
//        }

//        public async Task ConnectAsync(CancellationToken ct)
//        {
//            try
//            {
//                await _client.ConnectAsync(IPAddress.Parse(_peerInfo.Ip), _peerInfo.Port, ct);
//                _stream = _client.GetStream();
//                IsConnected = true;

//                发送握手消息
//               await SendHandshakeAsync(ct);

//                接收握手响应
//               await ReceiveHandshakeAsync(ct);
//            }
//            catch (Exception ex)
//            {
//                IsConnected = false;
//                throw new InvalidOperationException($"连接Peer失败: {ex.Message}", ex);
//            }
//        }

//        发送握手消息
//        private async Task SendHandshakeAsync(CancellationToken ct)
//        {
//            var handshake = new byte[68];
//            handshake[0] = 19; // Protocol length

//            协议标识
//            Array.Copy(Encoding.ASCII.GetBytes("BitTorrent protocol"), 0, handshake, 1, 19);

//            保留位（8字节）
//             此处简化处理

//             InfoHash
//            Array.Copy(_infoHash, 0, handshake, 28, 20);

//            PeerID
//            Array.Copy(Encoding.ASCII.GetBytes(_peerId), 0, handshake, 48, 20);

//            await _stream.WriteAsync(handshake, ct);
//        }

//        接收握手响应
//        private async Task ReceiveHandshakeAsync(CancellationToken ct)
//        {
//            var buffer = new byte[68];
//            int bytesRead = await _stream.ReadAsync(buffer, ct);

//            if (bytesRead != 68)
//                throw new InvalidDataException("握手响应长度不正确");

//            验证协议标识
//            string protocol = Encoding.ASCII.GetString(buffer, 1, 19);
//            if (protocol != "BitTorrent protocol")
//                throw new InvalidDataException("不支持的协议");

//            验证InfoHash
//            byte[] receivedInfoHash = new byte[20];
//            Array.Copy(buffer, 28, receivedInfoHash, 0, 20);

//            if (!receivedInfoHash.SequenceEqual(_infoHash))
//                throw new InvalidDataException("InfoHash不匹配");

//            Console.WriteLine($"与Peer {_peerInfo.Ip}:{_peerInfo.Port} 握手成功");
//        }

//        接收消息
//        public async Task<PeerMessage> ReceiveMessageAsync(CancellationToken ct)
//        {
//            try
//            {
//                读取消息长度（4字节，大端序）
//                var lengthBuffer = new byte[4];
//                int bytesRead = await _stream.ReadAsync(lengthBuffer, ct);

//                if (bytesRead == 0)
//                {
//                    连接关闭
//                    Disconnect();
//                    return null;
//                }

//                if (bytesRead != 4)
//                    throw new InvalidDataException("消息长度读取不完整");

//                int messageLength = BinaryPrimitives.ReadInt32BigEndian(lengthBuffer);

//                if (messageLength == 0)
//                {
//                    保持活跃消息（keep - alive）
//                    return null;
//                }

//                读取消息ID（1字节）
//                var idBuffer = new byte[1];
//                await _stream.ReadAsync(idBuffer, ct);
//                var messageId = (PeerMessageId)idBuffer[0];

//                读取消息负载
//               var payloadLength = messageLength - 1;
//                var payloadBuffer = new byte[payloadLength];

//                if (payloadLength > 0)
//                {
//                    await _stream.ReadAsync(payloadBuffer, ct);
//                }

//                解析消息
//                return ParseMessage(messageId, payloadBuffer);
//            }
//            catch (OperationCanceledException)
//            {
//                Disconnect();
//                return null;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"接收消息异常: {ex.Message}");
//                Disconnect();
//                return null;
//            }
//        }

//        解析消息
//        private PeerMessage ParseMessage(PeerMessageId messageId, byte[] payload)
//        {
//            switch (messageId)
//            {
//                case PeerMessageId.Choke:
//                    return new ChokeMessage();

//                case PeerMessageId.Unchoke:
//                    return new UnchokeMessage();

//                case PeerMessageId.Interested:
//                    return new InterestedMessage();

//                case PeerMessageId.NotInterested:
//                    return new NotInterestedMessage();

//                case PeerMessageId.Have:
//                    int pieceIndex = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(0, 4));
//                    return new HaveMessage { PieceIndex = pieceIndex };

//                case PeerMessageId.Bitfield:
//                    return new BitfieldMessage { Bitfield = payload };

//                case PeerMessageId.Piece:
//                    int index = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(0, 4));
//                    int begin = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(4, 4));
//                    byte[] block = new byte[payload.Length - 8];
//                    Array.Copy(payload, 8, block, 0, block.Length);
//                    return new PieceMessage { PieceIndex = index, Begin = begin, Block = block };

//                case PeerMessageId.Request:
//                    int reqIndex = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(0, 4));
//                    int reqBegin = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(4, 4));
//                    int reqLength = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(6, 4));
//                    return new RequestMessage { PieceIndex = reqIndex, Begin = reqBegin, Length = reqLength };

//                case PeerMessageId.Cancel:
//                    int cancelIndex = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(0, 4));
//                    int cancelBegin = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(4, 4));
//                    int cancelLength = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(6, 4));
//                    return new CancelMessage { PieceIndex = cancelIndex, Begin = cancelBegin, Length = cancelLength };

//                default:
//                    Console.WriteLine($"未知消息类型: {messageId}");
//                    return new PeerMessage { MessageId = messageId };
//            }
//        }

//        发送感兴趣消息
//        发送感兴趣消息
//        public async Task SendInterestedAsync(CancellationToken ct)
//        {
//            var message = new byte[5];
//            BinaryPrimitives.WriteInt32BigEndian(message.AsSpan(0, 4), 1); // 消息长度
//            message[4] = (byte)PeerMessageId.Interested; // 消息ID

//            await _stream.WriteAsync(message, ct);
//        }

//        发送不感兴趣消息
//        public async Task SendNotInterestedAsync(CancellationToken ct)
//        {
//            var message = new byte[5];
//            BinaryPrimitives.WriteInt32BigEndian(message.AsSpan(0, 4), 1); // 消息长度
//            message[4] = (byte)PeerMessageId.NotInterested; // 消息ID

//            await _stream.WriteAsync(message, ct);
//        }

//        发送Have消息
//        public async Task SendHaveAsync(int pieceIndex, CancellationToken ct)
//        {
//            var message = new byte[9];
//            BinaryPrimitives.WriteInt32BigEndian(message.AsSpan(0, 4), 5); // 消息长度
//            message[4] = (byte)PeerMessageId.Have; // 消息ID
//            BinaryPrimitives.WriteInt32BigEndian(message.AsSpan(5, 4), pieceIndex); // 分片索引

//            await _stream.WriteAsync(message, ct);
//        }

//        发送请求消息
//        public async Task SendRequestAsync(int pieceIndex, long begin, long length, CancellationToken ct)
//        {
//            var message = new byte[17];
//            BinaryPrimitives.WriteInt32BigEndian(message.AsSpan(0, 4), 13); // 消息长度
//            message[4] = (byte)PeerMessageId.Request; // 消息ID
//            BinaryPrimitives.WriteInt32BigEndian(message.AsSpan(5, 4), pieceIndex); // 分片索引
//            BinaryPrimitives.WriteInt32BigEndian(message.AsSpan(9, 4), (int)begin); // 偏移量
//            BinaryPrimitives.WriteInt32BigEndian(message.AsSpan(13, 4), (int)length); // 长度

//            await _stream.WriteAsync(message, ct);
//        }

//        发送数据块
//        public async Task SendPieceAsync(int pieceIndex, long begin, byte[] block, CancellationToken ct)
//        {
//            var message = new byte[9 + block.Length];
//            BinaryPrimitives.WriteInt32BigEndian(message.AsSpan(0, 4), 5 + block.Length); // 消息长度
//            message[4] = (byte)PeerMessageId.Piece; // 消息ID
//            BinaryPrimitives.WriteInt32BigEndian(message.AsSpan(5, 4), pieceIndex); // 分片索引
//            BinaryPrimitives.WriteInt32BigEndian(message.AsSpan(9, 4), (int)begin); // 偏移量
//            Array.Copy(block, 0, message, 13, block.Length); // 数据块

//            await _stream.WriteAsync(message, ct);
//        }

//        设置Bitfield
//        public void SetBitfield(byte[] bitfield)
//        {
//            for (int i = 0; i < bitfield.Length * 8; i++)
//            {
//                int byteIndex = i / 8;
//                int bitIndex = 7 - (i % 8);
//                _bitfield[i] = (bitfield[byteIndex] & (1 << bitIndex)) != 0;
//            }
//        }

//        检查Peer是否有特定分片
//        public bool HasPiece(int pieceIndex)
//        {
//            if (pieceIndex < 0 || pieceIndex >= _bitfield.Length)
//                return false;

//            return _bitfield[pieceIndex];
//        }

//        断开连接
//        public void Disconnect()
//        {
//            IsConnected = false;
//            _stream?.Close();
//            _client?.Close();
//        }

//        public void Dispose()
//        {
//            Disconnect();
//            _stream?.Dispose();
//            _client?.Dispose();
//        }
//    }

//    Peer信息
//    public class Peer
//    {
//        public string Ip { get; set; }
//        public int Port { get; set; }
//    }

//    Tracker事件枚举
//    public enum TrackerEvent
//    {
//        None = 0,
//        Started = 1,
//        Stopped = 2,
//        Completed = 3
//    }

//    Peer消息ID枚举
//    public enum PeerMessageId
//    {
//        Choke = 0,
//        Unchoke = 1,
//        Interested = 2,
//        NotInterested = 3,
//        Have = 4,
//        Bitfield = 5,
//        Request = 6,
//        Piece = 7,
//        Cancel = 8,
//        Port = 9
//    }

//    基础消息类
//    public class PeerMessage
//    {
//        public PeerMessageId MessageId { get; set; }
//    }

//    Choke消息
//    public class ChokeMessage : PeerMessage
//    {
//        public ChokeMessage()
//        {
//            MessageId = PeerMessageId.Choke;
//        }
//    }

//    Unchoke消息
//    public class UnchokeMessage : PeerMessage
//    {
//        public UnchokeMessage()
//        {
//            MessageId = PeerMessageId.Unchoke;
//        }
//    }

//    Interested消息
//    public class InterestedMessage : PeerMessage
//    {
//        public InterestedMessage()
//        {
//            MessageId = PeerMessageId.Interested;
//        }
//    }

//    NotInterested消息
//    public class NotInterestedMessage : PeerMessage
//    {
//        public NotInterestedMessage()
//        {
//            MessageId = PeerMessageId.NotInterested;
//        }
//    }

//    Have消息
//    public class HaveMessage : PeerMessage
//    {
//        public HaveMessage()
//        {
//            MessageId = PeerMessageId.Have;
//        }

//        public int PieceIndex { get; set; }
//    }

//    Bitfield消息
//    public class BitfieldMessage : PeerMessage
//    {
//        public BitfieldMessage()
//        {
//            MessageId = PeerMessageId.Bitfield;
//        }

//        public byte[] Bitfield { get; set; }
//    }

//    Request消息
//    public class RequestMessage : PeerMessage
//    {
//        public RequestMessage()
//        {
//            MessageId = PeerMessageId.Request;
//        }

//        public int PieceIndex { get; set; }
//        public long Begin { get; set; }
//        public long Length { get; set; }
//    }

//    Piece消息
//    public class PieceMessage : PeerMessage
//    {
//        public PieceMessage()
//        {
//            MessageId = PeerMessageId.Piece;
//        }

//        public int PieceIndex { get; set; }
//        public long Begin { get; set; }
//        public byte[] Block { get; set; }
//    }

//    Cancel消息
//    public class CancelMessage : PeerMessage
//    {
//        public CancelMessage()
//        {
//            MessageId = PeerMessageId.Cancel;
//        }

//        public int PieceIndex { get; set; }
//        public long Begin { get; set; }
//        public long Length { get; set; }
//    }
//}

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BitTorrentParser
{
    // Bencode解码工具类
    public static class BencodeDecoder
    {
        public static object Decode(byte[] data)
        {
            using var ms = new MemoryStream(data);
            return Decode(ms);
        }

        private static object Decode(MemoryStream ms)
        {
            int c = ms.ReadByte();
            if (c == -1) throw new FormatException("Unexpected end of data");

            switch (c)
            {
                case 'd': return DecodeDictionary(ms);
                case 'l': return DecodeList(ms);
                case 'i': return DecodeInteger(ms);
                default:
                    if (c >= '0' && c <= '9')
                    {
                        ms.Position--; // 回退一个字节以读取完整的字符串长度
                        return DecodeString(ms);
                    }
                    throw new FormatException($"Invalid bencode type: {(char)c}");
            }
        }

        private static Dictionary<string, object> DecodeDictionary(MemoryStream ms)
        {
            var dict = new Dictionary<string, object>();
            while (true)
            {
                int c = ms.ReadByte();
                if (c == -1) throw new FormatException("Unexpected end of dictionary");
                if (c == 'e') break;

                ms.Position--;
                string key = (string)Decode(ms);
                object value = Decode(ms);
                dict[key] = value;
            }
            return dict;
        }

        private static List<object> DecodeList(MemoryStream ms)
        {
            var list = new List<object>();
            while (true)
            {
                int c = ms.ReadByte();
                if (c == -1) throw new FormatException("Unexpected end of list");
                if (c == 'e') break;

                ms.Position--;
                list.Add(Decode(ms));
            }
            return list;
        }

        private static long DecodeInteger(MemoryStream ms)
        {
            var buffer = new List<byte>();
            int c;
            while ((c = ms.ReadByte()) != -1)
            {
                if (c == 'e') break;
                buffer.Add((byte)c);
            }
            return long.Parse(Encoding.ASCII.GetString(buffer.ToArray()));
        }

        private static string DecodeString(MemoryStream ms)
        {
            int length = 0;
            int c;
            while ((c = ms.ReadByte()) != -1)
            {
                if (c == ':') break;
                length = length * 10 + (c - '0');
            }

            var buffer = new byte[length];
            int bytesRead = ms.Read(buffer, 0, length);
            if (bytesRead != length) throw new FormatException("String length mismatch");

            return Encoding.UTF8.GetString(buffer);
        }
    }

    // 文件信息类
    public class TorrentFile
    {
        public string Path { get; set; }
        public long Length { get; set; }
        public long Offset { get; set; } // 文件在全局数据流中的起始偏移量
    }

    // 种子解析器
    public class TorrentParser
    {
        public string Announce { get; private set; }
        public string Name { get; private set; }
        public int PieceLength { get; private set; }
        public List<byte[]> PieceHashes { get; private set; } = new List<byte[]>();
        public List<TorrentFile> Files { get; private set; } = new List<TorrentFile>();
        public bool IsMultiFile { get; private set; }

        public void Parse(string torrentFilePath)
        {
            byte[] torrentData = File.ReadAllBytes(torrentFilePath);
            var torrentDict = (Dictionary<string, object>)BencodeDecoder.Decode(torrentData);

            // 解析基本信息
            Announce = (string)torrentDict["announce"];
            var infoDict = (Dictionary<string, object>)torrentDict["info"];
            Name = (string)infoDict["name"];
            PieceLength = (int)(long)infoDict["piece length"];

            // 解析分片哈希
            string pieces = (string)infoDict["pieces"];
            byte[] piecesBytes = Encoding.ASCII.GetBytes(pieces);
            for (int i = 0; i < piecesBytes.Length; i += 20)
            {
                byte[] hash = new byte[20];
                Array.Copy(piecesBytes, i, hash, 0, 20);
                PieceHashes.Add(hash);
            }

            // 解析文件信息
            if (infoDict.ContainsKey("files"))
            {
                IsMultiFile = true;
                var filesList = (List<object>)infoDict["files"];
                long currentOffset = 0;

                foreach (var fileObj in filesList)
                {
                    var fileDict = (Dictionary<string, object>)fileObj;
                    long length = (long)fileDict["length"];

                    // 构建文件路径
                    var pathList = (List<object>)fileDict["path"];
                    string fullPath = string.Join(Path.DirectorySeparatorChar,
                        pathList.ConvertAll(p => (string)p));

                    Files.Add(new TorrentFile
                    {
                        Path = fullPath,
                        Length = length,
                        Offset = currentOffset
                    });

                    currentOffset += length;
                }
            }
            else
            {
                IsMultiFile = false;
                long length = (long)infoDict["length"];
                Files.Add(new TorrentFile
                {
                    Path = Name,
                    Length = length,
                    Offset = 0
                });
            }
        }

        // 根据块索引获取对应的文件和位置
        public (TorrentFile file, long fileOffset, int pieceLength) GetFileForPiece(int pieceIndex)
        {
            long pieceOffset = (long)pieceIndex * PieceLength;
            long remaining = PieceLength;

            // 找到包含此块的文件
            foreach (var file in Files)
            {
                if (pieceOffset >= file.Offset + file.Length)
                    continue;

                long fileStart = Math.Max(pieceOffset, file.Offset);
                long fileOffset = fileStart - file.Offset;
                long available = file.Length - fileOffset;
                int pieceLen = (int)Math.Min(remaining, available);

                return (file, fileOffset, pieceLen);
            }

            throw new ArgumentException("Invalid piece index");
        }

        // 计算文件的SHA1哈希值
        public static byte[] CalculateFileHash(string filePath)
        {
            using var sha1 = SHA1.Create();
            using var stream = File.OpenRead(filePath);
            return sha1.ComputeHash(stream);
        }
    }
}
