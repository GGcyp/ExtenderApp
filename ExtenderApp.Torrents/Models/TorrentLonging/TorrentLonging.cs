using ExtenderApp.Abstract;
using ExtenderApp.Services;
using Microsoft.Extensions.Logging;
using MonoTorrent.Logging;

namespace ExtenderApp.Torrents.Models
{

    /// <summary>
    /// TorrentLonging 类实现了 ILogger 接口，用于记录与 Torrent 相关的日志信息。
    /// </summary>
    public class TorrentLonging : MonoTorrent.Logging.ILogger
    {
        /// <summary>
        /// 类名
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// 日志输出服务
        /// </summary>
        private readonly ILogger<TorrentLonging> _logger;

        /// <summary>
        /// TorrentLonging 类的构造函数。
        /// </summary>
        /// <param name="name">Torrent 的名称。</param>
        /// <param name="logger">用于记录日志的服务实例。</param>
        public TorrentLonging(string name, ILogger<TorrentLonging> logger)
        {
            _name = name;
            _logger = logger;
        }

        /// <summary>
        /// 记录调试信息。
        /// </summary>
        /// <param name="message">要记录的调试信息。</param>
        public void Debug(string message)
        {
            _logger.LogDebug(message, _name);
        }

        /// <summary>
        /// 记录错误信息。
        /// </summary>
        /// <param name="message">要记录的错误信息。</param>
        public void Error(string message)
        {
            _logger.LogError(null, message);
        }

        /// <summary>
        /// 记录信息日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        public void Info(string message)
        {
            _logger.LogInformation(message);
        }
    }
}
