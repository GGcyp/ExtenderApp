using ExtenderApp.Common;
using ExtenderApp.Common.Networks;
using ExtenderApp.Abstract;
using System.Diagnostics;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 表示一个Tracker对象，用于跟踪和管理与BitTorrent协议的通信。
    /// </summary>
    public class Tracker : DisposableObject
    {
        private const int Retries = 3;

        //public bool CanScrape => throw new NotImplementedException();

        //public LinkState Status => throw new NotImplementedException();

        private readonly LinkClient _client;

        //// 事件：收到 Tracker 响应
        //public event EventHandler<TrackerResponseEventArgs> ResponseReceived;
        //// 事件：Tracker 通信失败
        //public event EventHandler<TrackerErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// 获取Tracker的URL。
        /// </summary>
        public Uri TrackerUrl { get; private set; }

        private CancellationTokenSource? cts;

        /// <summary>
        /// 超时设置
        /// </summary>
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(15);

        private bool isUdpTracker;
        private bool isConnection;

        public event Action<Tracker>? OnConnection;

        public Tracker(LinkClient client, Uri trackerUrl)
        {
            TrackerUrl = trackerUrl;

            _client = client;
            _client.OnSendedTraffic += _client_OnSendedTraffic;
            _client.OnReceiveingTraffic += _client_OnReceiveingTraffic;

            if (_client is LinkClient<IUdpLinker, UdpTrackerParser> udpClient)
            {
                udpClient.Parser.OnReceiveConnectionId += OnReceiveConnectionId;
                isUdpTracker = true;
            }
        }

        private void _client_OnReceiveingTraffic(int obj)
        {
            Debug.Print($"接收到{obj}");
        }

        private void _client_OnSendedTraffic(int obj)
        {
            Debug.Print($"发送{obj}");
        }

        /// <summary>
        /// 建立连接的方法。
        /// </summary>
        public void Connection(int transactionId)
        {
            _client.Connect(TrackerUrl);
            if (isUdpTracker)
            {
                Task.Run(async () =>
                {
                    await UdpSendTo(transactionId);
                });
                return;
            }
        }

        /// <summary>
        /// 向 Tracker 发送 Announce 请求
        /// </summary>
        public void AnnounceAsync(TrackerRequest trackerRequest)
        {
            ThrowIfDisposed();

            //_trackerRequest.Hash = infoHash;
            //_trackerRequest.Uploaded = uploaded;
            //_trackerRequest.Downloaded = downloaded;
            //_trackerRequest.Left = left;
            //_trackerRequest.Event = (byte)eventType;
            //_trackerRequest.TransactionId = _random.Next();

            if (isUdpTracker && !isConnection)
            {
                throw new InvalidOperationException("请先连接服务器再发送消息");
            }

            if (isUdpTracker)
            {
                Task.Run(async () =>
                {
                    await UdpSendTo(trackerRequest);
                });
                return;
            }

            _client.SendAsync(trackerRequest);
        }

        private async Task UdpSendTo<T>(T value)
        {
            int retries = Retries;
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            cts = new();
            var token = cts.Token;
            while (retries > 0 && !token.IsCancellationRequested)
            {
                try
                {
                    _client.Send(value);
                    await Task.Delay(_timeout, token);
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
            throw new Exception($"链接Tracker服务器超时 服务器名{TrackerUrl}");
        }

        private void OnReceiveConnectionId(long obj)
        {
            cts?.Cancel();
            isConnection = true;
            OnConnection?.Invoke(this);
        }     
    }

    ///// <summary>
    ///// Tracker 响应数据
    ///// </summary>
    //public class TrackerResponse
    //{
    //    public bool Success { get; set; }
    //    public string ErrorMessage { get; set; }
    //    public int Interval { get; set; } // 下次请求间隔（秒）
    //    public List<PeerInfo> Peers { get; set; }
    //}
}