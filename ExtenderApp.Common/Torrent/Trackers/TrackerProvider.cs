using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Caches;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Networks;

namespace ExtenderApp.Common.Torrent
{
    /// <summary>
    /// TrackerProvider 类，用于提供 Tracker 服务。
    /// </summary>
    public class TrackerProvider
    {
        /// <summary>
        /// LinkerClientFactory 对象，用于创建 LinkClient 对象。
        /// </summary>
        private readonly LinkClientFactory _clientFactory;

        /// <summary>
        /// Tracker 对象的并发字典，以 Uri 为键。
        /// </summary>
        private readonly ConcurrentDictionary<Uri, Tracker> _tracker;

        /// <summary>
        /// URL 字符串与 Uri 的并发字典。
        /// </summary>
        private readonly ConcurrentDictionary<string, Uri> _urls;

        /// <summary>
        /// 无法连接的字符串的 HashSet。
        /// </summary>
        private readonly HashSet<string> _unconnectableHashSet;


        /// <summary>
        /// 当移除 Uri 时调用的回调方法。
        /// </summary>
        private readonly Action<Uri> _removeCallback;

        public TrackerProvider(LinkClientFactory linkerClientFactory)
        {
            _clientFactory = linkerClientFactory;

            _tracker = new();
            _urls = new();

            _unconnectableHashSet = new();
            _removeCallback = u => Remove(u);
        }

        /// <summary>
        /// 注册多个追踪器，通过字符串URI列表进行注册
        /// </summary>
        /// <param name="uris">字符串URI列表</param>
        public void RegisterTracker(IEnumerable<string> uris)
        {
            if (uris == null) return;

            foreach (var uri in uris)
            {
                RegisterTracker(uri);
            }
        }

        /// <summary>
        /// 注册多个追踪器，通过Uri对象列表进行注册
        /// </summary>
        /// <param name="uris">Uri对象列表</param>
        public void RegisterTracker(IEnumerable<Uri> uris)
        {
            if (uris == null) return;

            foreach (Uri uri in uris)
            {
                RegisterTracker(uri);
            }
        }

        /// <summary>
        /// 注册跟踪器。
        /// </summary>
        /// <param name="uri">跟踪器的URI。</param>
        /// <remarks>
        /// 如果传入的URI已存在于不可连接的集合中，则直接返回。
        /// 如果传入的URI在URL字典中已经存在，并且对应的跟踪器已存在，则直接返回。
        /// 如果跟踪器注册成功，则将URI和对应的Uri对象添加到URL字典中；如果注册失败，则将URI添加到不可连接的集合中。
        /// </remarks>
        public void RegisterTracker(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return;

            if (_unconnectableHashSet.Contains(uri))
                return;

            if (_urls.TryGetValue(uri, out var resultUri))
            {
                if (_tracker.ContainsKey(resultUri))
                    return;
            }

            lock (_urls)
            {
                if (_urls.TryGetValue(uri, out resultUri))
                {
                    if (_tracker.ContainsKey(resultUri))
                        return;
                }

                resultUri = new Uri(uri);
                if (RegisterTracker(resultUri))
                {
                    _urls.TryAdd(uri, resultUri);
                    return;
                }

                _unconnectableHashSet.Add(uri);
            }
        }

        /// <summary>
        /// 注册跟踪器。
        /// </summary>
        /// <param name="uri">跟踪器的Uri对象。</param>
        /// <returns>如果注册成功，则返回true；否则返回false。</returns>
        /// <remarks>
        /// 如果传入的Uri对象的Scheme不是"udp"或者Uri对象为null，则直接返回false。
        /// 如果传入的Uri对象已经存在于跟踪器字典中，则直接返回true。
        /// 如果跟踪器注册成功，则返回true；否则返回false。
        /// </remarks>
        public bool RegisterTracker(Uri uri)
        {
            if (uri == null)
                return false;

            if (uri.Scheme != "udp" || uri == null)
                return false;

            if (_tracker.ContainsKey(uri))
                return true;

            lock (_tracker)
            {
                if (_tracker.ContainsKey(uri))
                    return true;

                var ipEndPoint = GetHostIPEndPoint(uri);
                if (ipEndPoint == null)
                    return false;

                var item = GetLinkClient(uri);

                var tracker = new Tracker(item.Item1, ipEndPoint, uri, _removeCallback, item.Item2);
                _tracker.TryAdd(uri, tracker);
            }
            return true;
        }

        /// <summary>
        /// 根据指定的URI获取对应的Tracker对象。
        /// </summary>
        /// <param name="uri">要获取Tracker对象的URI。</param>
        /// <returns>返回对应的Tracker对象。</returns>
        public Tracker? GetTracker(string uri)
        {
            if (string.IsNullOrEmpty(uri)) return null;

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
            if (uri == null) return null;

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


                var item = GetLinkClient(uri);

                var tracker = new Tracker(item.Item1, ipEndPoint, uri, _removeCallback, item.Item2);
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
            if (string.IsNullOrEmpty(uri)) return null;

            if (!_urls.Remove(uri, out var targetUri))
                return null;
            _unconnectableHashSet.Add(uri);
            return Remove(targetUri);
        }

        /// <summary>
        /// 根据Uri移除对应的Tracker对象
        /// </summary>
        /// <param name="uri">Uri对象</param>
        /// <returns>移除的Tracker对象</returns>
        public Tracker? Remove(Uri uri)
        {
            if (uri == null) return null;

            if (_tracker.Remove(uri, out var tracker))
                return tracker;

            _unconnectableHashSet.Add(uri.ToString());
            return tracker;
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
        private (LinkClient, bool) GetLinkClient(Uri uri)
        {
            return (uri.Scheme) switch
            {
                "udp" => (_clientFactory.Create<IUdpLinker, UdpTrackerParser>(), true),
                "http" => (_clientFactory.Create<IHttpLinker, HttpTrackerParser>(), false),
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// 获取所有Tracker
        /// </summary>
        public IEnumerable<Tracker> GetAllTrackers() => _tracker.Values;
    }
}
