using System.Collections.Concurrent;
using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using System.Text;
using System.Diagnostics;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 日志服务类，实现了ILogService接口
    /// </summary>
    internal class LoggingService : ILogingService
    {
        /// <summary>
        /// 日志文件扩展名
        /// </summary>
        private const string EXTENSION = ".log";

        /// <summary>
        /// 日志队列
        /// </summary>
        private readonly ConcurrentQueue<LogInfo> _logQueue;

        /// <summary>
        /// 日志文件夹路径
        /// </summary>
        private readonly IPathService _pathProvider;

        /// <summary>
        /// 日志文本内容
        /// </summary>
        private readonly StringBuilder _logText;

        /// <summary>
        /// 构造函数，初始化日志服务
        /// </summary>
        /// <param name="service">刷新服务</param>
        /// <param name="environment">主机环境</param>
        public LoggingService(IRefreshService service, IPathService provider)
        {
            _logQueue = new ConcurrentQueue<LogInfo>();
            service.AddFixUpdate(FixUpdate);

            //检查是否存在存放日志文件夹，如不存在则创建
            string logFolderPath = provider.LoggingPath;
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }

            _pathProvider = provider;
            _logText = new StringBuilder();
        }

        /// <summary>
        /// 固定更新方法，用于将日志队列中的日志写入文件
        /// </summary>
        public void FixUpdate()
        {
            if (_logQueue.Count <= 0) return;

            WriteAndSave();
        }

        /// <summary>
        /// 打印日志信息到日志队列
        /// </summary>
        /// <param name="info">日志信息</param>
        public void Print(LogInfo info)
        {
            _logQueue.Enqueue(info);
        }

        /// <summary>
        /// 将日志信息写入并保存到文件中
        /// </summary>
        /// <remarks>
        /// 因为认为文件不会很大，所以一天的所有日志信息将放入一个文件中。
        /// </remarks>
        private void WriteAndSave()
        {
            //因为我认为文件不会很大，所以一天的就放一个文件里。
            while (_logQueue.Count > 0)
            {
                _logText.Clear();
                LogInfo info;
                if (!_logQueue.TryDequeue(out info)) break;

                var fileTime = info.Time.ToString("yyyyMMdd");
                var fileName = string.Concat(fileTime, EXTENSION);
                var filePath = Path.Combine(_pathProvider.LoggingPath, fileName);

                using (StreamWriter stream = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath))
                {
                    string logInfoMessage = LogInfoToStringBuilder(info);
                    stream.WriteLine(logInfoMessage);
#if DEBUG
                    Debug.Print(logInfoMessage);
#endif
                }
            }
        }

        /// <summary>
        /// 将日志信息转换为字符串构建器中的字符串
        /// </summary>
        /// <param name="info">日志信息对象</param>
        /// <returns>包含日志信息的字符串</returns>
        public string LogInfoToStringBuilder(LogInfo info)
        {
            _logText.Append($"时间: {info.Time.ToString("yyyy-MM-dd HH:mm:ss")}");
            _logText.Append($"，线程ID: {info.ThreadId}");
            _logText.Append($"，日志级别: {info.LogLevel}");
            _logText.Append($"，消息源: {info.Source}");
            _logText.Append($"，消息: {info.Message}");

            if (info.Exception != null)
            {
                _logText.Append($"，异常详情: {info.Exception.Message}");
                _logText.Append($"，异常堆栈: {info.Exception.StackTrace}");
            }

            return _logText.ToString();
        }
    }
}
