using System.Buffers;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Torrent
{
    /// <summary>
    /// BTMessageParser 类继承自 LinkParser 类，用于解析 BT 消息。
    /// </summary>
    public class BTMessageParser : LinkParser
    {
        /// <summary>
        /// BT 消息编码器实例。
        /// </summary>
        private readonly BTMessageEncoder _encoder;

        /// <summary>
        /// 标识是否已连接。
        /// </summary>
        private bool isConnected;

        /// <summary>
        /// 缓冲区，用于存储接收到的数据。
        /// </summary>
        private byte[]? buffer;

        /// <summary>
        /// 上次接收到的数据长度。
        /// </summary>
        private int lastReceivedLength;

        /// <summary>
        /// 握手事件，当接收到握手消息时触发。
        /// </summary>
        public event Action<InfoHash, PeerId>? OnHandshake;

        /// <summary>
        /// 阻止事件，当接收到阻止消息时触发。
        /// </summary>
        public event Action? OnChoke;

        /// <summary>
        /// 取消阻止事件，当接收到取消阻止消息时触发。
        /// </summary>
        public event Action? OnUnchoke;

        /// <summary>
        /// 感兴趣事件，当接收到感兴趣消息时触发。
        /// </summary>
        public event Action? OnInterested;

        /// <summary>
        /// 不感兴趣事件，当接收到不感兴趣消息时触发。
        /// </summary>
        public event Action? OnNotInterested;

        /// <summary>
        /// 已有消息事件，当接收到已有消息时触发。
        /// </summary>
        public event Action<int>? OnHave;

        /// <summary>
        /// 位字段消息事件，当接收到位字段消息时触发。
        /// </summary>
        public event Action<byte[]>? OnBitField;

        /// <summary>
        /// 请求消息事件，当接收到请求消息时触发。
        /// </summary>
        public event Action<int, int, int>? OnRequest;

        /// <summary>
        /// 数据片消息事件，当接收到数据片消息时触发。
        /// </summary>
        public event Action<int, int, int, byte[]>? OnPiece;

        /// <summary>
        /// 取消消息事件，当接收到取消消息时触发。
        /// </summary>
        public event Action<int, int, int>? OnCancel;

        /// <summary>
        /// 端口消息事件，当接收到端口消息时触发。
        /// </summary>
        public event Action<ushort>? OnPort;

        /// <summary>
        /// 未知消息事件，当接收到未知消息时触发。
        /// </summary>
        public event Action<BTMessage>? OnUnknown;

        /// <summary>
        /// 保持活动事件，当接收到保持活动消息时触发。
        /// </summary>
        public event Action? OnKeepAlive;

        public BTMessageParser(BTMessageEncoder encoder, SequencePool<byte> sequencePool) : base(sequencePool)
        {
            _encoder = encoder;
            isConnected = false;
        }

        public override void Serialize<T>(ref ExtenderBinaryWriter writer, T value)
        {
            switch (value)
            {
                case DataBuffer<InfoHash, PeerId> databuffer:
                    Handshake.Encode(ref writer, databuffer.Item1, databuffer.Item2);
                    break;
                case BTMessage message:
                    if (!isConnected)
                        throw new Exception("在发送消息之前，必须先完成握手。");
                    _encoder.Encode(ref writer, message);
                    break;
            }
            throw new NotSupportedException($"{typeof(T)}");
        }

        protected override void Receive(ref ExtenderBinaryReader reader)
        {
            if (isConnected)
            {
                // 处理已连接状态下的消息接收
                var message = _encoder.Decode(ref reader);
                switch (message.Id)
                {
                    case BTMessageType.Choke:
                        OnChoke?.Invoke();
                        break;
                    case BTMessageType.Unchoke:
                        OnUnchoke?.Invoke();
                        break;
                    case BTMessageType.Interested:
                        OnInterested?.Invoke();
                        break;
                    case BTMessageType.NotInterested:
                        OnNotInterested?.Invoke();
                        break;
                    case BTMessageType.Have:
                        OnHave?.Invoke(message.PieceIndex);
                        break;
                    case BTMessageType.BitField:
                        if (message.Data != null)
                            OnBitField?.Invoke(message.Data);
                        break;
                    case BTMessageType.Request:
                        OnRequest?.Invoke(message.PieceIndex, message.Begin, message.Length);
                        break;
                    case BTMessageType.Piece:
                        if (message.Data != null)
                            OnPiece?.Invoke(message.PieceIndex, message.Begin, message.Length, message.Data);
                        break;
                    case BTMessageType.Cancel:
                        OnCancel?.Invoke(message.PieceIndex, message.Begin, message.Length);
                        break;
                    case BTMessageType.Port:
                        OnPort?.Invoke(message.Port);
                        break;
                    case BTMessageType.KeepAlive:
                        OnKeepAlive?.Invoke();
                        break;
                    default:
                        OnUnknown?.Invoke(message);
                        break;
                }
            }
            else
            {
                if (reader.Remaining < Handshake.HandshakeLength)
                {
                    // 如果剩余字节不足68字节，说明握手消息不完整
                    if (buffer == null)
                    {
                        lastReceivedLength = 0;
                        lastReceivedLength += (int)reader.Remaining;
                        buffer = ArrayPool<byte>.Shared.Rent(68);
                        reader.TryCopyTo(buffer);
                        return;
                    }
                    else
                    {
                        // 如果buffer不为空，说明之前已经接收了一部分握手消息
                        if (lastReceivedLength + reader.Remaining > Handshake.HandshakeLength)
                        {
                            // 如果总长度超过68字节，抛出异常或处理错误
                            throw new InvalidOperationException("握手消息长度超过68字节");
                        }
                        else
                        {
                            // 将当前读取的内容追加到buffer中
                            reader.TryCopyTo(buffer.AsSpan(lastReceivedLength));
                            lastReceivedLength += (int)reader.Remaining;
                            if (lastReceivedLength != Handshake.HandshakeLength)
                            {
                                return;
                            }
                        }
                    }
                }

                // 处理握手消息
                buffer = buffer ?? ArrayPool<byte>.Shared.Rent(Handshake.HandshakeLength);
                Handshake.Decode(buffer, out var infoHash, out var id);
                isConnected = true;
                OnHandshake?.Invoke(infoHash, id);
            }
        }
    }
}
