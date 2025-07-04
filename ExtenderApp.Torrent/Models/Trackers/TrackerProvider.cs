using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;

namespace ExtenderApp.Torrent
{
    public class TrackerProvider
    {
        private readonly LinkerClientFactory _clientFactory;
        private readonly Dictionary<Uri, Tracker> _tracker;
        private readonly Dictionary<string, Uri> _urls;

        public TrackerProvider(LinkerClientFactory linkerClientFactory)
        {
            _clientFactory = linkerClientFactory;
            _tracker = new();
            _urls = new();
        }

        /// <summary>
        /// 根据指定的URI获取对应的Tracker对象。
        /// </summary>
        /// <param name="uri">要获取Tracker对象的URI。</param>
        /// <returns>返回对应的Tracker对象。</returns>
        public Tracker GetTracker(string uri)
        {
            if (_urls.TryGetValue(uri, out var resultUri))
                return GetTracker(resultUri);

            lock (_urls)
            {
                if (_urls.TryGetValue(uri, out resultUri))
                    return GetTracker(resultUri);

                resultUri = new Uri(uri);
                _urls.Add(uri, resultUri);
            }
            return GetTracker(resultUri);
        }

        /// <summary>
        /// 根据指定的URL获取对应的Tracker对象。
        /// </summary>
        /// <param name="url">要获取Tracker对象的URL。</param>
        /// <returns>返回对应的Tracker对象。</returns>
        public Tracker GetTracker(Uri url)
        {
            if (_tracker.TryGetValue(url, out var resultTracker))
                return resultTracker;

            lock (_tracker)
            {
                if (_tracker.TryGetValue(url, out resultTracker))
                    return resultTracker;

                LinkClient linkClient = GetLinkClient(url);

                resultTracker = new Tracker(linkClient, url);
                _tracker.Add(url, resultTracker);
            }
            return resultTracker;
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
                "http" => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }



        /// <summary>
        /// 获取所有Tracker
        /// </summary>
        public IEnumerable<Tracker> GetAllTrackers() => _tracker.Values;
    }
}
