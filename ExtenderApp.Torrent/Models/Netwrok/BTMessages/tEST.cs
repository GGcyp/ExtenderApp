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
//            // 发送握手消息
//            byte[] handshakeData = _handshake.Encode();
//            await _stream.WriteAsync(handshakeData, 0, handshakeData.Length);

//            // 接收握手消息
//            byte[] receiveBuffer = new byte[68];
//            int bytesRead = await _stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);
//            if (bytesRead != 68)
//                throw new InvalidDataException("握手消息长度不正确");

//            Handshake peerHandshake = Handshake.Decode(receiveBuffer);
//            if (!peerHandshake.Hash.SequenceEqual(_handshake.Hash))
//                throw new InvalidDataException("InfoHash不匹配");

//            Console.WriteLine("与Peer握手成功");

//            // 启动接收和发送任务
//            _receiveTask = ReceiveMessagesAsync();
//            _sendTask = ProcessSendQueueAsync();
//        }

//        private async Task ReceiveMessagesAsync()
//        {
//            try
//            {
//                byte[] lengthBuffer = new byte[4];
//                while (!_cts.Token.IsCancellationRequested)
//                {
//                    // 读取长度前缀
//                    int bytesRead = await ReadFullyAsync(lengthBuffer, 0, 4);
//                    if (bytesRead == 0)
//                        break; // 连接关闭

//                    int messageLength = BinaryPrimitives.ReadInt32BigEndian(lengthBuffer);
//                    if (messageLength < 0)
//                        throw new InvalidDataException("无效的消息长度");

//                    if (messageLength == 0)
//                    {
//                        // 保持活跃消息
//                        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(new KeepAliveMessage()));
//                        continue;
//                    }

//                    // 读取消息ID和数据
//                    byte[] messageBuffer = new byte[messageLength];
//                    await ReadFullyAsync(messageBuffer, 0, messageLength);

//                    // 解析消息
//                    BTMessageEncoder message = BTMessageEncoder.Decode(messageBuffer);
//                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));

//                    // 处理特定消息
//                    switch (message)
//                    {
//                        case BitFieldMessage bitFieldMsg:
//                            _peerBitField = new BitFieldData(bitFieldMsg.BitField, _fileManager.PieceCount);
//                            // 检查是否有感兴趣的分片
//                            _isInterested = HasInterestingPieces();
//                            if (_isInterested)
//                                EnqueueMessage(new InterestedMessage());
//                            break;
//                        case HaveMessage haveMsg:
//                            _peerBitField[haveMsg.PieceIndex] = true;
//                            // 检查是否对新分片感兴趣
//                            if (!_isInterested && _fileManager.BitField[haveMsg.PieceIndex] == false)
//                            {
//                                _isInterested = true;
//                                EnqueueMessage(new InterestedMessage());
//                            }
//                            break;
//                        case UnchokeMessage _:
//                            _isChoked = false;
//                            // 开始请求数据
//                            RequestPieces();
//                            break;
//                        case ChokeMessage _:
//                            _isChoked = true;
//                            break;
//                        case PieceMessage pieceMsg:
//                            // 保存接收到的数据块
//                            await _fileManager.WriteBlockAsync(pieceMsg.PieceIndex, pieceMsg.Begin, pieceMsg.Block);
//                            BlockReceived?.Invoke(this, new BlockReceivedEventArgs(
//                                pieceMsg.PieceIndex, pieceMsg.Begin, pieceMsg.Block.Length));

//                            // 请求更多数据
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
//                // 正常取消
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

//            // 选择要请求的分片（简化版：选择第一个未下载的分片）
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
