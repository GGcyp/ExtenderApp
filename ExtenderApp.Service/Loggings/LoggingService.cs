using ExtenderApp.Abstract;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;
using ExtenderApp.Services.Logging;
using Microsoft.Extensions.Logging;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 日志服务类，实现了ILogService接口
    /// </summary>
    internal class LoggingService : ILogingService
    {
        private readonly ILogger _logger;

        /// <summary>
        /// 构造函数，初始化日志服务
        /// </summary>
        /// <param name="logger">日志记录器实例</param>
        public LoggingService(ILogger logger)
        {
            _logger = logger;
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope(state);
        }
    }
}
