using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExtenderApp.Torrent.Models
{
    /// <summary>
    /// 取消请求消息
    /// </summary>
    public class CancelMessage : BtMessage
    {
        public int PieceIndex { get; }
        public int Begin { get; }
        public int Length { get; }

        public CancelMessage(int pieceIndex, int begin, int length)
        {
            PieceIndex = pieceIndex;
            Begin = begin;
            Length = length;
            LengthPrefix = 13;
            MessageId = MessageType.Cancel;
        }

        public static CancelMessage Decode(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < 12)
                throw new InvalidDataException("Cancel消息数据不足");

            int pieceIndex = BinaryPrimitives.ReadInt32BigEndian(buffer);
            int begin = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(4));
            int length = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(8));
            return new CancelMessage(pieceIndex, begin, length);
        }

        public override byte[] Encode()
        {
            var buffer = new byte[17];
            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(0, 4), 13);
            buffer[4] = (byte)MessageId;
            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(5, 4), PieceIndex);
            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(9, 4), Begin);
            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(13, 4), Length);
            return buffer;
        }
    }

    /// <summary>
    /// 端口消息（用于DHT）
    /// </summary>
    public class PortMessage : BtMessage
    {
        public ushort ListenPort { get; }

        public PortMessage(ushort listenPort)
        {
            ListenPort = listenPort;
            LengthPrefix = 3;
            MessageId = MessageType.Port;
        }

        public static PortMessage Decode(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < 2)
                throw new InvalidDataException("Port消息数据不足");

            ushort port = BinaryPrimitives.ReadUInt16BigEndian(buffer);
            return new PortMessage(port);
        }

        public override byte[] Encode()
        {
            var buffer = new byte[7];
            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(0, 4), 3);
            buffer[4] = (byte)MessageId;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(5, 2), ListenPort);
            return buffer;
        }
    }

    /// <summary>
    /// 未知消息类型
    /// </summary>
    public class UnknownMessage : BtMessage
    {
        public byte Id { get; }
        public byte[] Data { get; }

        public UnknownMessage(byte id, byte[] data)
        {
            Id = id;
            Data = data;
            LengthPrefix = 1 + data.Length;
            MessageId = (MessageType)id;
        }

        public override byte[] Encode()
        {
            var buffer = new byte[5 + Data.Length];
            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(0, 4), 1 + Data.Length);
            buffer[4] = Id;
            Array.Copy(Data, 0, buffer, 5, Data.Length);
            return buffer;
        }
    }

    /// <summary>
    /// BitTorrent 握手消息
    /// </summary>
    public class Handshake
    {
        private const string ProtocolString = "BitTorrent protocol";
        private static readonly byte[] ProtocolBytes = Encoding.ASCII.GetBytes(ProtocolString);
        private static readonly byte[] ReservedBytes = new byte[8];

        public byte[] InfoHash { get; }
        public byte[] PeerId { get; }

        public Handshake(byte[] infoHash, byte[] peerId)
        {
            if (infoHash.Length != 20)
                throw new ArgumentException("InfoHash必须是20字节", nameof(infoHash));
            if (peerId.Length != 20)
                throw new ArgumentException("PeerId必须是20字节", nameof(peerId));

            InfoHash = infoHash;
            PeerId = peerId;
        }

        public byte[] Encode()
        {
            var buffer = new byte[68];
            buffer[0] = (byte)ProtocolBytes.Length;
            Array.Copy(ProtocolBytes, 0, buffer, 1, ProtocolBytes.Length);
            Array.Copy(ReservedBytes, 0, buffer, 20, ReservedBytes.Length);
            Array.Copy(InfoHash, 0, buffer, 28, InfoHash.Length);
            Array.Copy(PeerId, 0, buffer, 48, PeerId.Length);
            return buffer;
        }

        public static Handshake Decode(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < 68)
                throw new InvalidDataException("握手消息长度不足");

            int pstrlen = buffer[0];
            if (pstrlen != ProtocolBytes.Length)
                throw new InvalidDataException("不支持的协议版本");

            var pstr = buffer.Slice(1, pstrlen);
            if (!pstr.SequenceEqual(ProtocolBytes))
                throw new InvalidDataException("协议标识不匹配");

            var infoHash = buffer.Slice(28, 20).ToArray();
            var peerId = buffer.Slice(48, 20).ToArray();

            return new Handshake(infoHash, peerId);
        }
    }

    /// <summary>
    /// BitTorrent 文件管理器
    /// </summary>
    public class TorrentFileManager : IDisposable
    {
        private readonly string _downloadPath;
        private readonly long _fileSize;
        private readonly int _pieceLength;
        private readonly byte[][] _pieceHashes;
        private readonly FileStream _fileStream;
        private readonly bool _isMultiFile;
        private readonly List<TorrentFile> _files;
        private readonly BitFieldData _bitField;
        private readonly SHA1 _sha1 = SHA1.Create();

        public string DownloadPath => _downloadPath;
        public long FileSize => _fileSize;
        public int PieceLength => _pieceLength;
        public int PieceCount => _pieceHashes.Length;
        public BitFieldData BitField => _bitField;

        public TorrentFileManager(string downloadPath, long fileSize, int pieceLength,
            byte[][] pieceHashes, bool isMultiFile = false, List<TorrentFile> files = null)
        {
            _downloadPath = downloadPath;
            _fileSize = fileSize;
            _pieceLength = pieceLength;
            _pieceHashes = pieceHashes;
            _isMultiFile = isMultiFile;
            _files = files ?? new List<TorrentFile>();
            _bitField = new BitFieldData(pieceHashes.Length);

            // 创建文件或目录
            if (_isMultiFile)
            {
                if (!Directory.Exists(downloadPath))
                    Directory.CreateDirectory(downloadPath);
            }
            else
            {
                var directory = Path.GetDirectoryName(downloadPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                _fileStream = new FileStream(downloadPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
        }

        public async Task WriteBlockAsync(int pieceIndex, int begin, byte[] block)
        {
            if (_isMultiFile)
            {
                // 多文件模式：计算块在各文件中的位置
                long globalOffset = (long)pieceIndex * _pieceLength + begin;
                await WriteToMultiFilesAsync(globalOffset, block);
            }
            else
            {
                // 单文件模式：直接写入
                long offset = (long)pieceIndex * _pieceLength + begin;
                _fileStream.Seek(offset, SeekOrigin.Begin);
                await _fileStream.WriteAsync(block, 0, block.Length);
            }

            // 检查分片是否完整并验证哈希
            if (IsPieceComplete(pieceIndex))
            {
                if (await VerifyPieceAsync(pieceIndex))
                {
                    _bitField[pieceIndex] = true;
                    Console.WriteLine($"分片 {pieceIndex} 验证成功");
                }
                else
                {
                    Console.WriteLine($"分片 {pieceIndex} 验证失败，将重新下载");
                    // 分片验证失败，可选择删除已下载部分
                }
            }
        }

        private async Task<bool> VerifyPieceAsync(int pieceIndex)
        {
            byte[] pieceData = new byte[GetPieceSize(pieceIndex)];
            long offset = (long)pieceIndex * _pieceLength;

            if (_isMultiFile)
            {
                // 从多个文件中读取分片数据
                await ReadFromMultiFilesAsync(offset, pieceData);
            }
            else
            {
                // 从单个文件中读取分片数据
                _fileStream.Seek(offset, SeekOrigin.Begin);
                await _fileStream.ReadAsync(pieceData, 0, pieceData.Length);
            }

            // 计算SHA-1哈希
            byte[] computedHash = _sha1.ComputeHash(pieceData);
            return computedHash.SequenceEqual(_pieceHashes[pieceIndex]);
        }

        private int GetPieceSize(int pieceIndex)
        {
            long lastPieceSize = _fileSize % _pieceLength;
            return pieceIndex == PieceCount - 1 ? (int)lastPieceSize : _pieceLength;
        }

        private async Task WriteToMultiFilesAsync(long globalOffset, byte[] data)
        {
            long remaining = data.Length;
            long dataOffset = 0;

            foreach (var file in _files)
            {
                if (globalOffset >= file.Length)
                {
                    globalOffset -= file.Length;
                    continue;
                }

                long writeLength = Math.Min(remaining, file.Length - globalOffset);
                if (writeLength <= 0)
                    break;

                using (var stream = new FileStream(
                    Path.Combine(_downloadPath, file.Path),
                    FileMode.OpenOrCreate,
                    FileAccess.Write))
                {
                    stream.Seek(globalOffset, SeekOrigin.Begin);
                    await stream.WriteAsync(data, (int)dataOffset, (int)writeLength);
                }

                remaining -= writeLength;
                dataOffset += writeLength;
                globalOffset = 0;

                if (remaining <= 0)
                    break;
            }
        }

        private async Task ReadFromMultiFilesAsync(long globalOffset, byte[] buffer)
        {
            long remaining = buffer.Length;
            long bufferOffset = 0;

            foreach (var file in _files)
            {
                if (globalOffset >= file.Length)
                {
                    globalOffset -= file.Length;
                    continue;
                }

                long readLength = Math.Min(remaining, file.Length - globalOffset);
                if (readLength <= 0)
                    break;

                using (var stream = new FileStream(
                    Path.Combine(_downloadPath, file.Path),
                    FileMode.Open,
                    FileAccess.Read))
                {
                    stream.Seek(globalOffset, SeekOrigin.Begin);
                    await stream.ReadAsync(buffer, (int)bufferOffset, (int)readLength);
                }

                remaining -= readLength;
                bufferOffset += readLength;
                globalOffset = 0;

                if (remaining <= 0)
                    break;
            }
        }

        private bool IsPieceComplete(int pieceIndex)
        {
            // 实际实现中需要跟踪每个分片的下载进度
            // 简化版：假设只要调用了WriteBlockAsync就认为数据完整
            return true;
        }

        public byte[] GetPieceData(int pieceIndex)
        {
            int pieceSize = GetPieceSize(pieceIndex);
            byte[] pieceData = new byte[pieceSize];
            long offset = (long)pieceIndex * _pieceLength;

            if (_isMultiFile)
            {
                // 从多个文件中读取
                using (var memoryStream = new MemoryStream(pieceData))
                {
                    ReadFromMultiFilesAsync(offset, pieceData).Wait();
                }
            }
            else
            {
                // 从单个文件中读取
                _fileStream.Seek(offset, SeekOrigin.Begin);
                _fileStream.Read(pieceData, 0, pieceSize);
            }

            return pieceData;
        }

        public void Dispose()
        {
            _fileStream?.Dispose();
            _sha1?.Dispose();
        }
    }

    /// <summary>
    /// 多文件模式下的文件信息
    /// </summary>
    public class TorrentFile
    {
        public string Path { get; }
        public long Length { get; }
        public string Md5Sum { get; }

        public TorrentFile(string path, long length, string md5Sum = null)
        {
            Path = path;
            Length = length;
            Md5Sum = md5Sum;
        }
    }

    /// <summary>
    /// BitTorrent Peer 连接
    /// </summary>
    public class PeerConnection : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly Handshake _handshake;
        private readonly TorrentFileManager _fileManager;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Queue<BtMessage> _sendQueue = new Queue<BtMessage>();
        private bool _isChoked = true;
        private bool _isInterested = false;
        private BitFieldData _peerBitField;
        private Task _receiveTask;
        private Task _sendTask;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<BlockReceivedEventArgs> BlockReceived;
        public event EventHandler Disconnected;

        public PeerConnection(TcpClient client, Handshake handshake, TorrentFileManager fileManager)
        {
            _client = client;
            _stream = client.GetStream();
            _handshake = handshake;
            _fileManager = fileManager;
        }

        public async Task ConnectAsync()
        {
            // 发送握手消息
            byte[] handshakeData = _handshake.Encode();
            await _stream.WriteAsync(handshakeData, 0, handshakeData.Length);

            // 接收握手消息
            byte[] receiveBuffer = new byte[68];
            int bytesRead = await _stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);
            if (bytesRead != 68)
                throw new InvalidDataException("握手消息长度不正确");

            Handshake peerHandshake = Handshake.Decode(receiveBuffer);
            if (!peerHandshake.InfoHash.SequenceEqual(_handshake.InfoHash))
                throw new InvalidDataException("InfoHash不匹配");

            Console.WriteLine("与Peer握手成功");

            // 启动接收和发送任务
            _receiveTask = ReceiveMessagesAsync();
            _sendTask = ProcessSendQueueAsync();
        }

        private async Task ReceiveMessagesAsync()
        {
            try
            {
                byte[] lengthBuffer = new byte[4];
                while (!_cts.Token.IsCancellationRequested)
                {
                    // 读取长度前缀
                    int bytesRead = await ReadFullyAsync(lengthBuffer, 0, 4);
                    if (bytesRead == 0)
                        break; // 连接关闭

                    int messageLength = BinaryPrimitives.ReadInt32BigEndian(lengthBuffer);
                    if (messageLength < 0)
                        throw new InvalidDataException("无效的消息长度");

                    if (messageLength == 0)
                    {
                        // 保持活跃消息
                        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(new KeepAliveMessage()));
                        continue;
                    }

                    // 读取消息ID和数据
                    byte[] messageBuffer = new byte[messageLength];
                    await ReadFullyAsync(messageBuffer, 0, messageLength);

                    // 解析消息
                    BtMessage message = BtMessage.Decode(messageBuffer);
                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));

                    // 处理特定消息
                    switch (message)
                    {
                        case BitFieldMessage bitFieldMsg:
                            _peerBitField = new BitFieldData(bitFieldMsg.BitField, _fileManager.PieceCount);
                            // 检查是否有感兴趣的分片
                            _isInterested = HasInterestingPieces();
                            if (_isInterested)
                                EnqueueMessage(new InterestedMessage());
                            break;
                        case HaveMessage haveMsg:
                            _peerBitField[haveMsg.PieceIndex] = true;
                            // 检查是否对新分片感兴趣
                            if (!_isInterested && _fileManager.BitField[haveMsg.PieceIndex] == false)
                            {
                                _isInterested = true;
                                EnqueueMessage(new InterestedMessage());
                            }
                            break;
                        case UnchokeMessage _:
                            _isChoked = false;
                            // 开始请求数据
                            RequestPieces();
                            break;
                        case ChokeMessage _:
                            _isChoked = true;
                            break;
                        case PieceMessage pieceMsg:
                            // 保存接收到的数据块
                            await _fileManager.WriteBlockAsync(pieceMsg.PieceIndex, pieceMsg.Begin, pieceMsg.Block);
                            BlockReceived?.Invoke(this, new BlockReceivedEventArgs(
                                pieceMsg.PieceIndex, pieceMsg.Begin, pieceMsg.Block.Length));

                            // 请求更多数据
                            RequestPieces();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"接收消息时发生错误: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        private async Task<int> ReadFullyAsync(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                int bytesRead = await _stream.ReadAsync(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                    return totalBytesRead; // 连接关闭
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }

        private async Task ProcessSendQueueAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    BtMessage message = null;
                    lock (_sendQueue)
                    {
                        if (_sendQueue.Count > 0)
                            message = _sendQueue.Dequeue();
                    }

                    if (message != null)
                    {
                        byte[] messageData = message.Encode();
                        await _stream.WriteAsync(messageData, 0, messageData.Length);
                    }
                    else
                    {
                        await Task.Delay(100, _cts.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送消息时发生错误: {ex.Message}");
            }
        }

        public void EnqueueMessage(BtMessage message)
        {
            lock (_sendQueue)
            {
                _sendQueue.Enqueue(message);
            }
        }

        private bool HasInterestingPieces()
        {
            if (_peerBitField == null)
                return false;

            for (int i = 0; i < _fileManager.PieceCount; i++)
            {
                if (_peerBitField[i] && !_fileManager.BitField[i])
                    return true;
            }
            return false;
        }

        private void RequestPieces()
        {
            if (_isChoked || _peerBitField == null)
                return;

            // 选择要请求的分片（简化版：选择第一个未下载的分片）
            int pieceIndex = _fileManager.BitField.FirstFalse();
            if (pieceIndex >= 0 && _peerBitField[pieceIndex])
            {
                int pieceSize = _fileManager.GetPieceSize(pieceIndex);
                int blockSize = 16 * 1024; // 16KB块大小

                for (int begin = 0; begin < pieceSize; begin += blockSize)
                {
                    int length = Math.Min(blockSize, pieceSize - begin);
                    EnqueueMessage(new RequestMessage(pieceIndex, begin, length));
                }
            }
        }

        public void Disconnect()
        {
            _cts.Cancel();
            _receiveTask?.Wait();
            _sendTask?.Wait();
            _stream?.Close();
            _client?.Close();
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Disconnect();
            _cts.Dispose();
        }
    }

    /// <summary>
    /// 消息接收事件参数
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        public BtMessage Message { get; }

        public MessageReceivedEventArgs(BtMessage message)
        {
            Message = message;
        }
    }

    /// <summary>
    /// 数据块接收事件参数
    /// </summary>
    public class BlockReceivedEventArgs : EventArgs
    {
        public int PieceIndex { get; }
        public int Begin { get; }
        public int Length { get; }

        public BlockReceivedEventArgs(int pieceIndex, int begin, int length)
        {
            PieceIndex = pieceIndex;
            Begin = begin;
            Length = length;
        }
    }

}
