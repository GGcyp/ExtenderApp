using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Caches;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Networks;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// TrackerProvider 类，用于提供 Tracker 服务。
    /// </summary>
    public class TrackerProvider
    {
        /// <summary>
        /// 重试次数常量，值为3。
        /// </summary>
        private const int Retries = 3;

        /// <summary>
        /// UdpTrackerParser 对象，用于解析 UDP Tracker 数据。
        /// </summary>
        private readonly UdpTrackerParser _udpTrackerParser;

        /// <summary>
        /// LinkerClientFactory 对象，用于创建 LinkClient 对象。
        /// </summary>
        private readonly LinkerClientFactory _clientFactory;

        /// <summary>
        /// Tracker 对象的并发字典，以 Uri 为键。
        /// </summary>
        private readonly ConcurrentDictionary<Uri, Tracker> _tracker;

        /// <summary>
        /// URL 字符串与 Uri 的并发字典。
        /// </summary>
        private readonly ConcurrentDictionary<string, Uri> _urls;

        /// <summary>
        /// 请求字典，使用 EvictionCache 存储 LinkClient、TrackerRequest 和 CancellationTokenSource 的 DataBuffer 对象。
        /// </summary>
        private readonly EvictionCache<int, DataBuffer<LinkClient, TrackerRequest, CancellationTokenSource>> _requestDict;

        /// <summary>
        /// 对等请求字典，使用 EvictionCache 存储 CancellationTokenSource 对象。
        /// </summary>
        private readonly EvictionCache<int, CancellationTokenSource> _peerRequestDict;

        /// <summary>
        /// 无法连接的字符串的 HashSet。
        /// </summary>
        private readonly HashSet<string> _unconnectableHashSet;

        /// <summary>
        /// CancellationTokenSource 对象，用于取消操作。
        /// </summary>
        private readonly CancellationTokenSource _cts;

        /// <summary>
        /// 当移除 Uri 时调用的回调方法。
        /// </summary>
        private readonly Action<Uri> _removeCallback;

        /// <summary>
        /// 发送 Tracker 请求时调用的回调方法。
        /// </summary>
        private readonly Action<LinkClient, TrackerRequest> _sendTrackerRequestAction;

        /// <summary>
        /// 超时时间跨度。
        /// </summary>
        private readonly TimeSpan _timeout;

        /// <summary>
        /// 用于生成随机数的 Random 对象。
        /// </summary>
        private readonly Random _random;

        public TrackerProvider(LinkerClientFactory linkerClientFactory, UdpTrackerParser udpTrackerParser)
        {
            _clientFactory = linkerClientFactory;
            _udpTrackerParser = udpTrackerParser;

            _tracker = new();
            _urls = new();
            _random = new();
            _unconnectableHashSet = new();
            _cts = new();
            _sendTrackerRequestAction = SendTrackerRequest;

            var interval = TimeSpan.FromSeconds(50);
            _peerRequestDict = new();
            _peerRequestDict.ChangeInterval(interval);
            _requestDict = new();
            _requestDict.ChangeInterval(interval);

            _timeout = TimeSpan.FromSeconds(15);

            _removeCallback = u => Remove(u);
            _udpTrackerParser.OnReceiveConnectionId += ReceiveConnectionId;
            _udpTrackerParser.OnReceivePeer += OnReceivePeer;
        }

        /// <summary>
        /// 根据指定的URI获取对应的Tracker对象。
        /// </summary>
        /// <param name="uri">要获取Tracker对象的URI。</param>
        /// <returns>返回对应的Tracker对象。</returns>
        public Tracker? GetTracker(string uri)
        {
            if (_unconnectableHashSet.Contains(uri))
                return null;

            Tracker? tracker = null;
            if (_urls.TryGetValue(uri, out var resultUri))
            {
                tracker = GetTracker(resultUri);
                if (tracker != null)
                    return tracker;

                _urls.Remove(uri, out resultUri);
                return null;
            }


            lock (_urls)
            {
                if (_urls.TryGetValue(uri, out resultUri))
                {
                    tracker = GetTracker(resultUri);
                    if (tracker != null)
                        return tracker;

                    _urls.Remove(uri, out resultUri);
                    return null;
                }

                resultUri = new Uri(uri);
                tracker = GetTracker(resultUri);
                if (tracker != null)
                {
                    _urls.TryAdd(uri, resultUri);
                    return tracker;
                }

                _unconnectableHashSet.Add(uri);
            }
            return tracker;
        }

        /// <summary>
        /// 根据指定的URL获取对应的Tracker对象。
        /// </summary>
        /// <param name="uri">要获取Tracker对象的URL。</param>
        /// <returns>返回对应的Tracker对象。</returns>
        public Tracker? GetTracker(Uri uri)
        {
            if (uri.Scheme != "udp" || uri == null)
                return null;

            if (_tracker.TryGetValue(uri, out var resultTracker))
                return resultTracker;

            lock (_tracker)
            {
                if (_tracker.TryGetValue(uri, out resultTracker))
                    return resultTracker;

                var ipEndPoint = GetHostIPEndPoint(uri);
                if (ipEndPoint == null)
                    return null;

                LinkClient linkClient = GetLinkClient(uri);

                resultTracker = new Tracker(linkClient, ipEndPoint, uri, _removeCallback, _sendTrackerRequestAction);
                _tracker.TryAdd(uri, resultTracker);
            }
            return resultTracker;
        }

        /// <summary>
        /// 根据URI移除对应的Tracker对象
        /// </summary>
        /// <param name="uri">URI字符串</param>
        /// <returns>移除的Tracker对象，若不存在则返回null</returns>
        public Tracker? Remove(string uri)
        {
            if (!_urls.Remove(uri, out var targetUri))
                return null;

            return Remove(targetUri);
        }

        /// <summary>
        /// 根据Uri移除对应的Tracker对象
        /// </summary>
        /// <param name="uri">Uri对象</param>
        /// <returns>移除的Tracker对象</returns>
        public Tracker? Remove(Uri uri)
        {
            _tracker.Remove(uri, out var tracker);
            return tracker;
        }

        /// <summary>
        /// 发送Tracker请求
        /// </summary>
        /// <param name="linkClient">LinkClient对象</param>
        /// <param name="trackerRequest">Tracker请求对象</param>
        private void SendTrackerRequest(LinkClient linkClient, TrackerRequest trackerRequest)
        {
            int transactionId = 0;
            while (true)
            {
                transactionId = _random.Next();
                if (!_requestDict.TryGet(transactionId, out var value))
                {
                    break;
                }
            }

            var dataBuffer = DataBuffer<LinkClient, TrackerRequest, CancellationTokenSource>.GetDataBuffer();
            dataBuffer.Item1 = linkClient;
            dataBuffer.Item2 = trackerRequest;
            dataBuffer.Item3 = new();
            _requestDict.AddOrUpdate(transactionId, dataBuffer);
            linkClient.Send(transactionId);
        }

        /// <summary>
        /// 接收连接ID
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="connectionId">连接ID</param>
        private void ReceiveConnectionId(int transactionId, long connectionId)
        {
            if (!_requestDict.Remove(transactionId, out var result))
                return;

            var request = result.Item2;
            request.ConnectionId = connectionId;
            transactionId = _random.Next();
            request.TransactionId = transactionId;
            result.Item1.SendAsync(request);
            var cts = result.Item3;
            cts?.Cancel();
            cts?.Dispose();

            cts = new();
            _peerRequestDict.AddOrUpdate(transactionId, cts);


            UdpSendTo(result.Item1, cts, request);
            DataBuffer<LinkClient, TrackerRequest, CancellationTokenSource>.ReleaseDataBuffer(result);
        }

        /// <summary>
        /// 接收对等体请求
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        private void OnReceivePeer(int transactionId)
        {
            if (!_peerRequestDict.Remove(transactionId, out var cts))
                return;

            cts.Cancel();
            cts.Dispose();
        }

        /// <summary>
        /// 根据Uri获取主机IP端点
        /// </summary>
        /// <param name="uri">Uri对象</param>
        /// <returns>主机IP端点，若获取失败则返回null</returns>
        private IPEndPoint? GetHostIPEndPoint(Uri uri)
        {
            try
            {
                var ips = Dns.GetHostAddresses(uri.Host);
                var ip = ips.First(i => i.AddressFamily == AddressFamily.InterNetwork);
                if (ip == null) return null;
                int port = uri.Port;
                return new IPEndPoint(ip, port);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 根据指定的URI获取对应的LinkClient对象。
        /// </summary>
        /// <param name="uri">要获取LinkClient对象的URI。</param>
        /// <returns>返回对应的LinkClient对象。</returns>
        private LinkClient GetLinkClient(Uri uri)
        {
            return (uri.Scheme) switch
            {
                "udp" => _clientFactory.Create<IUdpLinker, UdpTrackerParser>(),
                "http" => _clientFactory.Create<IHttpLinker, HttpTrackerParser>(),
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// 通过UDP发送数据到指定客户端。
        /// </summary>
        /// <typeparam name="T">发送的数据类型</typeparam>
        /// <param name="linkClient">链接客户端</param>
        /// <param name="cts">取消令牌源</param>
        /// <param name="value">要发送的数据</param>
        private void UdpSendTo<T>(LinkClient linkClient, CancellationTokenSource cts, T value)
        {
            Task.Run(async () =>
            {
                await UdpSendToAsync(linkClient, cts, value);
            });
        }

        /// <summary>
        /// 异步通过UDP发送数据到指定客户端。
        /// </summary>
        /// <typeparam name="T">发送的数据类型</typeparam>
        /// <param name="linkClient">链接客户端</param>
        /// <param name="cts">取消令牌源</param>
        /// <param name="value">要发送的数据</param>
        /// <returns>异步任务</returns>
        /// <exception cref="Exception">链接Tracker服务器超时</exception>
        private async Task UdpSendToAsync<T>(LinkClient linkClient, CancellationTokenSource cts, T value)
        {
            int retries = Retries;
            var allToken = _cts.Token;
            var timeToken = cts.Token;
            while (retries > 0 && !allToken.IsCancellationRequested && !timeToken.IsCancellationRequested)
            {
                try
                {
                    linkClient.Send(value);
                    await Task.WhenAny(Task.Delay(_timeout, timeToken), Task.FromCanceled(allToken));
                    retries--;
                }
                catch (TaskCanceledException ex)
                {
                    // 被取消，说明收到响应或外部取消，直接退出方法即可
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    // 其它异常可根据需要处理或记录
                    throw;
                }
            }
            throw new Exception($"链接Tracker服务器超时");
        }

        /// <summary>
        /// 获取所有Tracker
        /// </summary>
        public IEnumerable<Tracker> GetAllTrackers() => _tracker.Values;
    }
}
