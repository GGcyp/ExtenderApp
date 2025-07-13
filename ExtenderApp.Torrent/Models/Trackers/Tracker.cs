using ExtenderApp.Common;
using ExtenderApp.Common.Networks;
using ExtenderApp.Torrent.Models.Trackers;
using System.Net;
using System.Net.Sockets;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 表示一个Tracker对象，用于跟踪和管理与BitTorrent协议的通信。
    /// </summary>
    public class Tracker : DisposableObject
    {
        //public bool CanScrape => throw new NotImplementedException();

        //public LinkState Status => throw new NotImplementedException();

        private readonly Action<Uri> _removeCallback;
        private readonly Lazy<TrackerResponseCache> _responseCacheLazy;

        public LinkClient Client { get; }
        public Uri TrackerUri { get; }

        public Tracker(LinkClient client, EndPoint endPoint, Uri trackerUri, Action<Uri> removeCallback)
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
        }

        private void OnErrored(Exception obj)
        {
            switch (obj)
            {
                case SocketException socketException:
                    _removeCallback?.Invoke(TrackerUri);
                    Dispose();
                    //throw socketException;
                    break;
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