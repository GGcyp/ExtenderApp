using ExtenderApp.Common.Caches;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Networks;

namespace ExtenderApp.Torrent
{
    public class TorrentSender
    {
        /// <summary>
        /// 重试次数常量，值为3。
        /// </summary>
        private const int Retries = 3;

        /// <summary>
        /// CancellationTokenSource 对象，用于取消操作。
        /// </summary>
        private readonly CancellationTokenSource _cts;

        /// <summary>
        /// 请求字典，使用 EvictionCache 存储 LinkClient、TrackerRequest 和 CancellationTokenSource 的 DataBuffer 对象。
        /// </summary>
        private readonly EvictionCache<int, DataBuffer<LinkClient, TrackerRequest, CancellationTokenSource>> _requestDict;

        /// <summary>
        /// 对等请求字典，使用 EvictionCache 存储 CancellationTokenSource 对象。
        /// </summary>
        private readonly EvictionCache<int, CancellationTokenSource> _peerRequestDict;

        /// <summary>
        /// 私有只读属性，用于存储键为整数类型，值为DataBuffer<InfoHashPeerStore, Tracker>类型的EvictionCache
        /// </summary>
        private readonly EvictionCache<int, DataBuffer<InfoHashPeerStore, Tracker>> _storeAndTrackerDict;

        /// <summary>
        /// 超时时间跨度。
        /// </summary>
        private readonly TimeSpan _timeout;

        /// <summary>
        /// 用于生成随机数的 Random 对象。
        /// </summary>
        private readonly Random _random;

        /// <summary>
        /// 只读字段，用于解析UDP Tracker数据
        /// </summary>
        private readonly UdpTrackerParser _udpTrackerParser;

        private readonly TrackerProvider _trackerProvider;

        private readonly TorrentPeerProvider _torrentPeerProvider;

        public TorrentSender(UdpTrackerParser udpTrackerParser, TrackerProvider trackerProvider, TorrentPeerProvider torrentPeerProvider)
        {
            _trackerProvider = trackerProvider;
            _torrentPeerProvider = torrentPeerProvider;

            _requestDict = new();
            _peerRequestDict = new();

            _random = new();
            _cts = new();

            var interval = TimeSpan.FromSeconds(50);
            _peerRequestDict = new();
            _peerRequestDict.ChangeInterval(interval);
            _requestDict = new();
            _requestDict.ChangeInterval(interval);
            _storeAndTrackerDict = new();
            _storeAndTrackerDict.ChangeInterval(interval);

            _timeout = TimeSpan.FromSeconds(15);

            _udpTrackerParser = udpTrackerParser;
            _udpTrackerParser.OnReceiveConnectionId += ReceiveConnectionId;
            _udpTrackerParser.OnReceivePeerAddress += OnReceivePeerAddress;
        }

        /// <summary>
        /// 向所有追踪器发送请求
        /// </summary>
        /// <param name="trackerRequest">追踪请求</param>
        public void SendTrackerRequest(TrackerRequest trackerRequest)
        {
            foreach (var item in _trackerProvider.GetAllTrackers())
            {
                SendTrackerRequest(item, trackerRequest);
            }
        }

        /// <summary>
        /// 发送Tracker请求
        /// </summary>
        /// <param name="linkClient">LinkClient对象</param>
        /// <param name="trackerRequest">Tracker请求对象</param>
        public void SendTrackerRequest(string trackerUri, TrackerRequest trackerRequest)
        {
            var tracker = _trackerProvider.GetTracker(trackerUri);
            SendTrackerRequest(tracker, trackerRequest);
        }

        /// <summary>
        /// 发送跟踪器请求
        /// </summary>
        /// <param name="tracker">跟踪器实例</param>
        /// <param name="trackerRequest">跟踪器请求</param>
        private void SendTrackerRequest(Tracker? tracker, TrackerRequest trackerRequest)
        {
            InfoHash hash = trackerRequest.Hash;
            if (tracker == null || !tracker.CanResend(hash))
                return;
            var linkClient = tracker.Client;

            int transactionId = _random.Next();
            CancellationTokenSource cts = new();
            UdpSendTo(linkClient, cts, transactionId);

            transactionId = _random.Next();
            trackerRequest.TransactionId = transactionId;

            var requestDataBuffer = DataBuffer<LinkClient, TrackerRequest, CancellationTokenSource>.GetDataBuffer();
            requestDataBuffer.Item1 = linkClient;
            requestDataBuffer.Item2 = trackerRequest;
            requestDataBuffer.Item3 = cts;

            var storeDatabuffer = DataBuffer<InfoHashPeerStore, Tracker>.GetDataBuffer();
            storeDatabuffer.Item1 = _torrentPeerProvider.GetInfoHashPeerStore(hash);
            storeDatabuffer.Item2 = tracker;
            _storeAndTrackerDict.AddOrUpdate(transactionId, storeDatabuffer);
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
            result.Item1.SendAsync(request);
            var cts = result.Item3;
            cts?.Cancel();
            cts?.Dispose();

            cts = new();
            UdpSendTo(result.Item1, cts, request);
            _peerRequestDict.AddOrUpdate(transactionId, cts);


            DataBuffer<LinkClient, TrackerRequest, CancellationTokenSource>.ReleaseDataBuffer(result);
        }

        /// <summary>
        /// 接收对等体请求
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        private DataBuffer<InfoHashPeerStore, Tracker> OnReceivePeerAddress(int transactionId)
        {
            if (_peerRequestDict.Remove(transactionId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }

            _storeAndTrackerDict.Remove(transactionId, out var dataBuffer);
            return dataBuffer;
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
            while (retries > 0 && !allToken.IsCancellationRequested && !timeToken.IsCancellationRequested && !linkClient.IsDisposed)
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
    }
}
