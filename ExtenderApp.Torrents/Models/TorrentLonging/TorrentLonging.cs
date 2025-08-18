using ExtenderApp.Abstract;
using ExtenderApp.Services;
using MonoTorrent.Logging;

namespace ExtenderApp.Torrents.Models
{

    /// <summary>
    /// TorrentLonging 类实现了 ILogger 接口，用于记录与 Torrent 相关的日志信息。
    /// </summary>
    public class TorrentLonging : ILogger
    {
        /// <summary>
        /// 类名
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// 日志输出服务
        /// </summary>
        private readonly ILogingService _logingService;

        /// <summary>
        /// TorrentLonging 类的构造函数。
        /// </summary>
        /// <param name="name">Torrent 的名称。</param>
        /// <param name="logingService">用于记录日志的服务实例。</param>
        public TorrentLonging(string name, ILogingService logingService)
        {
            _name = name;
            _logingService = logingService;
        }

        /// <summary>
        /// 记录调试信息。
        /// </summary>
        /// <param name="message">要记录的调试信息。</param>
        public void Debug(string message)
        {
            _logingService.Debug(message, _name);
        }

        /// <summary>
        /// 记录错误信息。
        /// </summary>
        /// <param name="message">要记录的错误信息。</param>
        public void Error(string message)
        {
            _logingService.Error(message, _name, null);
        }

        /// <summary>
        /// 记录信息日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        public void Info(string message)
        {
            _logingService.Info(message, _name);
        }
    }
}
