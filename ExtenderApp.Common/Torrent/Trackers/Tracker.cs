using ExtenderApp.Common;
using ExtenderApp.Common.Networks;
using ExtenderApp.Common.Torrent.Models.Trackers;
using System.Net;
using System.Net.Sockets;

namespace ExtenderApp.Common.Torrent
{
    /// <summary>
    /// 表示一个Tracker对象，用于跟踪和管理与BitTorrent协议的通信。
    /// </summary>
    public class Tracker : DisposableObject
    {
        //public bool CanScrape => throw new NotImplementedException();

        //public LinkState Status => throw new NotImplementedException();

        /// <summary>
        /// 移除回调委托
        /// </summary>
        private readonly Action<Uri> _removeCallback;

        /// <summary>
        /// 响应缓存的延迟初始化对象
        /// </summary>
        private readonly Lazy<TrackerResponseCache> _responseCacheLazy;

        /// <summary>
        /// 获取或设置客户端对象。
        /// </summary>
        public LinkClient Client { get; }

        /// <summary>
        /// 获取或设置跟踪器的URI。
        /// </summary>
        public Uri TrackerUri { get; }

        public bool IsUdpLink { get; }

        public Tracker(LinkClient client, EndPoint endPoint, Uri trackerUri, Action<Uri> removeCallback, bool isUdpLink)
        {
            _responseCacheLazy = new(() =>
            {
                TrackerResponseCache cache = new();
                cache.ChangeInterval(TimeSpan.FromMinutes(1));
                return cache;
            });

            Client = client;
            TrackerUri = trackerUri;
            _removeCallback = removeCallback;
            Client.OnErrored += OnErrored;
            Client.ConnectAsync(endPoint);
            IsUdpLink = isUdpLink;
        }

        private void OnErrored(Exception obj)
        {
            switch (obj)
            {
                case SocketException socketException:
                    _removeCallback?.Invoke(TrackerUri);
                    Dispose();
                    //throw socketException;
                    return;
            }
        }

        public bool CanResend(InfoHash infoHash)
        {
            if (!_responseCacheLazy.Value.TryGet(infoHash, out var response))
                return true;

            if (DateTime.UtcNow - response.LastAnnounceTime > TimeSpan.FromMinutes(response.Interval))
            {
                response = _responseCacheLazy.Value.Remove(infoHash);
                response.Release();
                return true;
            }

            return false;
        }

        public void AddTrackerResponse(InfoHash infoHash, TrackerResponse response)
        {
            _responseCacheLazy.Value.AddOrUpdate(infoHash, response);
        }

        protected override void Dispose(bool disposing)
        {
            Client?.Dispose();
        }
    }
}