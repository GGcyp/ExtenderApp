using ExtenderApp.Common;
using ExtenderApp.Common.Networks;
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

        private readonly LinkClient _client;

        private readonly Action<Uri> _removeCallback;

        private readonly Action<LinkClient, TrackerRequest> _sendCallback;

        public Uri TrackerUri { get; }

        private CancellationTokenSource? cts;

        public Tracker(LinkClient client, EndPoint endPoint, Uri trackerUri, Action<Uri> removeCallback, Action<LinkClient, TrackerRequest> sendCallback)
        {
            _client = client;
            TrackerUri = trackerUri;
            _removeCallback = removeCallback;
            _client.OnErrored += OnErrored;
            _client.ConnectAsync(endPoint);
            _sendCallback = sendCallback;
        }

        private void OnErrored(Exception obj)
        {
            switch (obj)
            {
                case SocketException socketException:
                    _removeCallback?.Invoke(TrackerUri);
                    Dispose();
                    break;
            }
        }

        /// <summary>
        /// 向 Tracker 发送 Announce 请求
        /// </summary>
        public void AnnounceAsync(TrackerRequest trackerRequest)
        {
            ThrowIfDisposed();

            _sendCallback(_client, trackerRequest);
        }

        protected override void Dispose(bool disposing)
        {
            _client?.Dispose();
        }
    }
}