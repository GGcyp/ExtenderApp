using Microsoft.Extensions.Logging;

namespace ExtenderApp.Torrents.Models
{
    /// <summary>
    /// TorrentLonging 工厂类，用于创建 TorrentLonging 实例。
    /// </summary>
    public class TorrentLongingFactory
    {
        /// <summary>
        /// 依赖注入的日志服务，用于记录日志。
        /// </summary>
        private readonly ILogger<TorrentLonging> _logger;

        /// <summary>
        /// TorrentLongingFactory 构造函数。
        /// </summary>
        /// <param name="logger">日志服务实例，用于后续创建 TorrentLonging 实例时记录日志。</param>
        public TorrentLongingFactory(ILogger<TorrentLonging> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 创建一个 TorrentLonging 实例。
        /// </summary>
        /// <param name="name">TorrentLonging 实例的名称。</param>
        /// <returns>返回创建的 TorrentLonging 实例。</returns>
        public TorrentLonging CreateTorrentLonging(string name)
        {
            return new TorrentLonging(name, _logger);
        }
    }
}
