using System.Collections.Concurrent;
using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using System.Text;

namespace ExtenderApp.Service
{
    /// <summary>
    /// 日志服务类，实现了ILogService接口
    /// </summary>
    internal class LogService : ILogService
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
        private readonly string _logFolderPath;

        /// <summary>
        /// 日志文本内容
        /// </summary>
        private readonly StringBuilder _logText;

        /// <summary>
        /// 构造函数，初始化日志服务
        /// </summary>
        /// <param name="service">刷新服务</param>
        /// <param name="environment">主机环境</param>
        public LogService(IRefreshService service, IHostEnvironment environment)
        {
            _logQueue = new ConcurrentQueue<LogInfo>();
            service.AddFixUpdate(FixUpdate);

            //检查是否存在存放日志文件夹，如不存在则创建
            _logFolderPath = Path.Combine(environment.ContentRootPath, AppSetting.AppLogFolderName);
            if (!Directory.Exists(_logFolderPath))
            {
                Directory.CreateDirectory(_logFolderPath);
            }

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
                var filePath = Path.Combine(_logFolderPath, fileName);

                using (StreamWriter stream = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath))
                {
                    stream.WriteLine(info.ToString(_logText));
                }
            }
        }
    }
}
