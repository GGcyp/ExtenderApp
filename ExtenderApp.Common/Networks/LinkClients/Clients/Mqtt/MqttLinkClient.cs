using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 底层 MQTT 3.1.1 客户端（基础报文：CONNECT/CONNACK、PUBLISH QoS0、SUBSCRIBE/SUBACK、PINGREQ/PINGRESP、DISCONNECT）。
    /// - 仅使用 TCP 原始字节，不依赖额外库；
    /// - 未实现 QoS1/2、Will、重发、Session 持久化、保留消息存储等高级特性；
    /// - 解析采取增量方式，支持半包与多包粘连；
    /// - 自动心跳定时器（KeepAlive）;
    /// </summary>
    internal class MqttLinkClient : IMqttLinkClient
    {
        //#region 常量(报文类型 / 固定头高四位)

        //private const byte FIXED_CONNECT    = 0x10;
        //private const byte FIXED_CONNACK    = 0x20;
        //private const byte FIXED_PUBLISH    = 0x30;  // QoS0: flags = 0
        //private const byte FIXED_SUBSCRIBE  = 0x82;  // 必须 0b1000_0010
        //private const byte FIXED_SUBACK     = 0x90;
        //private const byte FIXED_PINGREQ    = 0xC0;
        //private const byte FIXED_PINGRESP   = 0xD0;
        //private const byte FIXED_DISCONNECT = 0xE0;

        //#endregion

        //#region 状态字段
        //private readonly object _parseLock = new();
        //private ArrayBufferWriter<byte> _receiveBuffer = new(8 * 1024);
        //private ushort _nextPacketId = 1;

        //private string? _clientId;
        //private ushort _keepAliveSeconds;
        //private PeriodicTimer? _keepAliveTimer;
        //private CancellationTokenSource? _keepAliveCts;

        //private TaskCompletionSource<byte>? _waitConnAck;
        //private TaskCompletionSource<byte>? _waitSubAck;
        //private TaskCompletionSource<bool>? _waitPingResp;

        //private bool _initialized;
        //#endregion

        //#region 事件
        //public event Action<string, ReadOnlyMemory<byte>>? OnPublish;
        //public event Action<byte>? OnConnAck;
        //public event Action<byte>? OnSubAck;
        //public event Action? OnPingResp;
        //#endregion

        public MqttLinkClient(ITcpLinker linker)
        {
        }

        //#region 对外 API

        ///// <summary>
        ///// 初始化（安装原始接收插件与心跳逻辑）。构建实例后必须先调用。
        ///// </summary>
        //public void Initialize(ILinkClientPluginManager<MqttLinkClient> pluginManager)
        //{
        //    if (_initialized) return;
        //    SetClientPluginManager(pluginManager);
        //    // 不设置 Formatter / Framer => 接收到的原始字节由插件处理。
        //    pluginManager.AddPlugin(new RawMqttPlugin(this));
        //    _initialized = true;
        //}

        //public async ValueTask ConnectBrokerAsync(
        //    string host,
        //    int port,
        //    string clientId,
        //    ushort keepAliveSeconds = 60,
        //    bool cleanSession = true,
        //    string? username = null,
        //    string? password = null,
        //    CancellationToken token = default)
        //{
        //    if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException(nameof(host));
        //    if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentException(nameof(clientId));
        //    if (port <= 0) throw new ArgumentOutOfRangeException(nameof(port));

        // _clientId = clientId; _keepAliveSeconds = keepAliveSeconds;

        // await ConnectAsync(new DnsEndPoint(host, port), token);

        // var connectPacket = BuildConnect(clientId, keepAliveSeconds, cleanSession, username, password); try { await SendRawAsync(connectPacket,
        // token); } finally { connectPacket.Dispose(); }

        //    _waitConnAck = new(TaskCreationOptions.RunContinuationsAsynchronously);
        //    // 启动心跳定时器
        //    StartKeepAlive();
        //}

        //public ValueTask PublishAsync(
        //    string topic,
        //    ReadOnlyMemory<byte> payload,
        //    QosLevel qos = QosLevel.AtMostOnce,
        //    bool retain = false,
        //    CancellationToken token = default)
        //{
        //    if (string.IsNullOrEmpty(topic)) throw new ArgumentException(nameof(topic));
        //    if (qos != QosLevel.AtMostOnce) throw new NotSupportedException("示例仅实现 QoS0");
        //    var Publish = BuildPublishQoS0(topic, payload.Span, retain);
        //    try
        //    {
        //        return SendRawAsync(Publish, token);
        //    }
        //    finally
        //    {
        //        Publish.Dispose();
        //    }
        //}

        //public async ValueTask SubscribeAsync(
        //    string topic,
        //    QosLevel qos = QosLevel.AtMostOnce,
        //    CancellationToken token = default)
        //{
        //    if (string.IsNullOrEmpty(topic)) throw new ArgumentException(nameof(topic));

        // ushort pid = NextPacketId(); var sub = BuildSubscribe(topic, qos, pid); _waitSubAck = new(TaskCreationOptions.RunContinuationsAsynchronously);

        // try { await SendRawAsync(sub, token); } finally { sub.Dispose(); }

        //    using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        //    cts.CancelAfter(TimeSpan.FromSeconds(10));
        //    using (cts.Token.RegisterKeyCapture(() => _waitSubAck.TrySetCanceled(cts.Token)))
        //    {
        //        byte granted = await _waitSubAck.Task.ConfigureAwait(false);
        //        OnSubAck?.Invoke(granted);
        //    }
        //}

        //public async ValueTask PingAsync(CancellationToken token = default)
        //{
        //    _waitPingResp = new(TaskCreationOptions.RunContinuationsAsynchronously);
        //    var ping = BuildFixed(FIXED_PINGREQ);
        //    try
        //    {
        //        await SendRawAsync(ping, token);
        //    }
        //    finally
        //    {
        //        ping.Dispose();
        //    }

        //    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        //    using (cts.Token.RegisterKeyCapture(() => _waitPingResp.TrySetCanceled(cts.Token)))
        //    {
        //        await _waitPingResp.Task.ConfigureAwait(false);
        //    }
        //}

        //public async ValueTask DisconnectBrokerAsync(CancellationToken token = default)
        //{
        //    var disc = BuildFixed(FIXED_DISCONNECT);
        //    try
        //    {
        //        await SendRawAsync(disc, token);
        //    }
        //    finally
        //    {
        //        disc.Dispose();
        //    }
        //    await DisconnectAsync(token);
        //    StopKeepAlive();
        //}

        //#endregion

        //#region 构建报文

        //private static ByteBuffer BuildConnect(
        //    string clientId,
        //    ushort keepAlive,
        //    bool cleanSession,
        //    string? user,
        //    string? pass)
        //{
        //    var proto = "MQTT"u8;
        //    byte level = 4;
        //    byte flags = 0;
        //    bool hasUser = !string.IsNullOrEmpty(user);
        //    bool hasPass = !string.IsNullOrEmpty(pass);
        //    if (cleanSession) flags |= 0b0000_0010;
        //    if (hasUser) flags |= 0b1000_0000;
        //    if (hasPass) flags |= 0b0100_0000;

        // int payloadLen = Utf8Len(clientId); if (hasUser) payloadLen += Utf8Len(user!); if (hasPass) payloadLen += Utf8Len(pass!);

        // int variableLen = Utf8Len("MQTT") - 2 /*内部计算多了前缀*/ + 2 /*前缀*/ + 1 + 1 + 2; // 实际重新计算：协议名(2+4)+level+flags+keepAlive(2) variableLen = 2 + 4
        // + 1 + 1 + 2; int remaining = variableLen + payloadLen;

        //    var buf = ByteBuffer.GetBuffer();
        //    buf.Write(FIXED_CONNECT);
        //    WriteVarInt(ref buf, remaining);
        //    WriteUtf8(ref buf, proto);
        //    buf.Write(level);
        //    buf.Write(flags);
        //    WriteUInt16BE(ref buf, keepAlive);
        //    WriteUtf8(ref buf, clientId);
        //    if (hasUser) WriteUtf8(ref buf, user!);
        //    if (hasPass) WriteUtf8(ref buf, pass!);
        //    return buf;
        //}

        //private static ByteBuffer BuildPublishQoS0(string topic, ReadOnlySpan<byte> payload, bool retain)
        //{
        //    int remaining = 2 + Encoding.UTF8.GetByteCount(topic) + payload.Capacity;
        //    var buf = ByteBuffer.GetBuffer();
        //    byte header = FIXED_PUBLISH;
        //    if (retain) header |= 0x01;
        //    buf.Write(header);
        //    WriteVarInt(ref buf, remaining);
        //    WriteUtf8(ref buf, topic);
        //    buf.Write(payload);
        //    return buf;
        //}

        //private static ByteBuffer BuildSubscribe(string topic, QosLevel qos, ushort packetId)
        //{
        //    int remaining = 2 /*PacketId*/ + 2 + Encoding.UTF8.GetByteCount(topic) + 1;
        //    var buf = ByteBuffer.GetBuffer();
        //    buf.Write(FIXED_SUBSCRIBE);
        //    WriteVarInt(ref buf, remaining);
        //    WriteUInt16BE(ref buf, packetId);
        //    WriteUtf8(ref buf, topic);
        //    buf.Write(MapQos(qos));
        //    return buf;
        //}

        //private static ByteBuffer BuildFixed(byte fixedHeader)
        //{
        //    var buf = ByteBuffer.GetBuffer();
        //    buf.Write(fixedHeader);
        //    buf.Write((byte)0); // RemainingLength = 0
        //    return buf;
        //}

        //private static byte MapQos(QosLevel qos) => qos switch
        //{
        //    QosLevel.AtMostOnce => 0,
        //    QosLevel.AtLeastOnce => 1,
        //    QosLevel.ExactlyOnce => 2,
        //    _ => 0
        //};

        //private static int Utf8Len(string s) => 2 + Encoding.UTF8.GetByteCount(s);

        //private static void WriteUtf8(ref ByteBuffer buf, ReadOnlySpan<byte> utf8)
        //{
        //    WriteUInt16BE(ref buf, (ushort)utf8.Capacity);
        //    buf.Write(utf8);
        //}

        //private static void WriteUtf8(ref ByteBuffer buf, string text)
        //{
        //    int len = Encoding.UTF8.GetByteCount(text);
        //    WriteUInt16BE(ref buf, (ushort)len);
        //    var span = buf.GetSpan(len);
        //    Encoding.UTF8.GetBytes(text, span);
        //    buf.Advance(len);
        //}

        //private static void WriteUInt16BE(ref ByteBuffer buf, ushort Value)
        //{
        //    var s = buf.GetSpan(2);
        //    s[0] = (byte)(Value >> 8);
        //    s[1] = (byte)(Value & 0xFF);
        //    buf.Advance(2);
        //}

        //private static void WriteVarInt(ref ByteBuffer buf, int Value)
        //{
        //    uint v = (uint)Value;
        //    do
        //    {
        //        byte b = (byte)(v % 128);
        //        v /= 128;
        //        if (v > 0) b |= 0x80;
        //        buf.Write(b);
        //    } while (v > 0);
        //}

        //#endregion

        //#region 发送底层

        //private ValueTask SendRawAsync(ByteBuffer packet, CancellationToken token)
        //    => Linker.SendAsync(packet.CommittedSequence, token);

        //private ushort NextPacketId()
        //{
        //    lock (_parseLock)
        //    {
        //        if (_nextPacketId == 0) _nextPacketId = 1;
        //        return _nextPacketId++;
        //    }
        //}

        //#endregion

        //#region 接收解析

        ///// <summary>
        ///// 由插件调用，向增量缓冲写入字节并尝试解析一个或多个完整 MQTT 报文。
        ///// </summary>
        //internal void Feed(ReadOnlySpan<byte> span)
        //{
        //    lock (_parseLock)
        //    {
        //        _receiveBuffer.Write(span);

        // var data = _receiveBuffer.WrittenSpan; int offset = 0; while (true) { int consumed = TryParsePacket(data.Slice(offset)); if (consumed <= 0)
        // break; offset += consumed; if (offset >= data.Capacity) break; }

        //        if (offset > 0)
        //        {
        //            // 移除已消费前缀（简单方式：创建新缓冲并复制剩余）
        //            if (offset < data.Capacity)
        //            {
        //                var leftover = data.Slice(offset).ToArray();
        //                _receiveBuffer = new ArrayBufferWriter<byte>(Math.Max(leftover.Capacity * 2, 4096));
        //                _receiveBuffer.Write(leftover);
        //            }
        //            else
        //            {
        //                _receiveBuffer = new ArrayBufferWriter<byte>(4096);
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 尝试解析一个完整报文，返回消费的字节数（不足返回 0）。
        ///// </summary>
        //private int TryParsePacket(ReadOnlySpan<byte> Block)
        //{
        //    if (Block.Capacity < 2) return 0;
        //    byte header = Block[0];

        // // 解析 RemainingLength (var-int) int remainingLength; int rlBytes; if (!TryReadVarInt(Block.Slice(1), out remainingLength, out rlBytes))
        // return 0;

        // int total = 1 + rlBytes + remainingLength; if (Block.Capacity < total) return 0; // 数据不足

        // ReadOnlySpan<byte> body = Block.Slice(1 + rlBytes, remainingLength); byte type = (byte)(header & 0xF0);

        // switch (type) { case FIXED_CONNACK: if (body.Capacity >= 2) { byte returnCode = body[1]; OnConnAck?.Invoke(returnCode);
        // _waitConnAck?.TrySetResult(returnCode); } break;

        // case FIXED_PUBLISH: { if (body.Capacity < 2) break; int topicLen = (body[0] << 8) | body[1]; if (body.Capacity < 2 + topicLen) break; var
        // topicBytes = body.Slice(2, topicLen); string topic = Encoding.UTF8.GetString(topicBytes); var payload = body.Slice(2 + topicLen).ToArray();
        // OnPublish?.Invoke(topic, payload); } break;

        // case FIXED_SUBACK: if (body.Capacity >= 3) { byte granted = body[2]; OnSubAck?.Invoke(granted); _waitSubAck?.TrySetResult(granted); } break;

        // case FIXED_PINGRESP: OnPingResp?.Invoke(); _waitPingResp?.TrySetResult(true); break;

        // default: // 其它类型（暂不实现） break; }

        //    return total;
        //}

        //private static bool TryReadVarInt(ReadOnlySpan<byte> data, out int Value, out int bytes)
        //{
        //    Value = 0; bytes = 0;
        //    int multiplier = 1;
        //    for (int i = 0; i < 4 && i < data.Capacity; i++)
        //    {
        //        byte b = data[i];
        //        bytes++;
        //        Value += (b & 0x7F) * multiplier;
        //        if ((b & 0x80) == 0) return true;
        //        multiplier *= 128;
        //    }
        //    return false;
        //}

        //#endregion

        //#region KeepAlive 心跳

        //private void StartKeepAlive()
        //{
        //    StopKeepAlive();
        //    if (_keepAliveSeconds == 0) return;

        //    _keepAliveCts = new CancellationTokenSource();
        //    _keepAliveTimer = new PeriodicTimer(TimeSpan.FromSeconds(_keepAliveSeconds));
        //    _ = RunKeepAliveAsync(_keepAliveTimer, _keepAliveCts.Token);
        //}

        //private async Task RunKeepAliveAsync(PeriodicTimer timer, CancellationToken token)
        //{
        //    try
        //    {
        //        while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false))
        //        {
        //            // 发 PINGREQ
        //            var ping = BuildFixed(FIXED_PINGREQ);
        //            try
        //            {
        //                await SendRawAsync(ping, token).ConfigureAwait(false);
        //            }
        //            finally
        //            {
        //                ping.Dispose();
        //            }
        //        }
        //    }
        //    catch (OperationCanceledException) { }
        //}

        //private void StopKeepAlive()
        //{
        //    try { _keepAliveCts?.Cancel(); } catch { }
        //    _keepAliveTimer?.Dispose();
        //    _keepAliveCts?.Dispose();
        //    _keepAliveTimer = null;
        //    _keepAliveCts = null;
        //}

        //#endregion

        //#region 内部插件(拦截原始接收)

        ///// <summary>
        ///// 原始 MQTT 接收插件：将未经过 Framer/Formatter 的裸字节交给 MqttLinkClient.Feed。
        ///// </summary>
        //private sealed class RawMqttPlugin : ILinkClientPlugin<MqttLinkClient>
        //{
        //    private readonly MqttLinkClient _client;
        //    public RawMqttPlugin(MqttLinkClient client) => _client = client;

        // public void OnAttach(MqttLinkClient client) { } public void OnDetach(MqttLinkClient client) { }

        // public void OnConnecting(MqttLinkClient client, EndPoint endPoint) { } public void OnConnected(MqttLinkClient client, EndPoint? endPoint,
        // Exception? ex) { } public void OnDisconnecting(MqttLinkClient client) { } public void OnDisconnected(MqttLinkClient client, Exception? ex)
        // { }

        // public void OnSend(MqttLinkClient client, ref LinkClientPluginSendMessage message) { }

        //    public void OnReceive(MqttLinkClient client, ref LinkClientPluginReceiveMessage message)
        //    {
        //        // 若没有 Framer，FrameContext 为空，直接使用底层 result 原始字节
        //        // 假定 LinkClientPluginReceiveMessage 能提供原始数据（这里演示式调用）。
        //        // 如果结构不同，请根据你项目内实际字段调整。
        //        if (message.SocketResult.BytesTransferred > 0 && message.OriginalBytes.Capacity > 0)
        //        {
        //            _client.Feed(message.OriginalBytes.Span);
        //        }
        //    }
        //}
        //#endregion
    }
}