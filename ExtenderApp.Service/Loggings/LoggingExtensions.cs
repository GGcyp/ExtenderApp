using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 日志服务扩展方法类
    /// </summary>
    public static class LogServiceExtensions
    {
        /// <summary>
        /// 向日志服务记录调试信息
        /// </summary>
        /// <param name="service">日志服务接口</param>
        /// <param name="message">调试信息内容</param>
        /// <param name="sourceType">日志来源类型</param>
        public static void Debug(this ILogingService service, string message, Type sourceType)
        {
            service.Debug(message, sourceType.Name);
        }

        /// <summary>
        /// 向日志服务记录调试信息
        /// </summary>
        /// <param name="service">日志服务接口</param>
        /// <param name="message">调试信息内容</param>
        /// <param name="source">日志来源</param>
        public static void Debug(this ILogingService service, string message, string source)
        {
            LogInfo info = new LogInfo()
            {
                Time = DateTime.UtcNow,
                Message = message,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Source = source,
                LogLevel = LogLevel.DEBUG,
            };
            service.Print(info);
        }

        /// <summary>
        /// 向日志服务记录普通信息
        /// </summary>
        /// <param name="service">日志服务接口</param>
        /// <param name="message">信息内容</param>
        /// <param name="sourceType">日志来源类型</param>
        public static void Info(this ILogingService service, string message, Type sourceType)
        {
            service.Info(message, sourceType.Name);
        }

        /// <summary>
        /// 向日志服务记录普通信息
        /// </summary>
        /// <param name="service">日志服务接口</param>
        /// <param name="message">信息内容</param>
        /// <param name="source">日志来源</param>
        public static void Info(this ILogingService service, string message, string source)
        {
            LogInfo info = new LogInfo()
            {
                Time = DateTime.UtcNow,
                Message = message,
                Source = source,
                LogLevel = LogLevel.INFO,
            };
            service.Print(info);
        }

        /// <summary>
        /// 向日志服务记录警告信息
        /// </summary>
        /// <param name="service">日志服务接口</param>
        /// <param name="message">警告信息内容</param>
        /// <param name="sourceType">日志来源类型</param>
        public static void Warning(this ILogingService service, string message, Type sourceType)
        {
            service.Warning(message, sourceType.Name);
        }

        /// <summary>
        /// 向日志服务记录警告信息
        /// </summary>
        /// <param name="service">日志服务接口</param>
        /// <param name="message">警告信息内容</param>
        /// <param name="source">日志来源</param>
        public static void Warning(this ILogingService service, string message, string source)
        {
            LogInfo info = new LogInfo()
            {
                Time = DateTime.UtcNow,
                Message = message,
                Source = source,
                LogLevel = LogLevel.WARNING,
            };
            service.Print(info);
        }

        /// <summary>
        /// 向日志服务记录错误信息
        /// </summary>
        /// <param name="service">日志服务接口</param>
        /// <param name="message">错误信息内容</param>
        /// <param name="sourceType">日志来源类型</param>
        /// <param name="exception">异常对象</param>
        public static void Error(this ILogingService service, string message, Type sourceType, Exception exception)
        {
            service.Error(message, sourceType.Name, exception);
        }

        /// <summary>
        /// 向日志服务记录错误信息
        /// </summary>
        /// <param name="service">日志服务接口</param>
        /// <param name="message">错误信息内容</param>
        /// <param name="source">日志来源</param>
        /// <param name="exception">异常对象</param>
        public static void Error(this ILogingService service, string message, string source, Exception exception)
        {
            LogInfo info = new LogInfo()
            {
                Time = DateTime.UtcNow,
                Message = message,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Source = source,
                LogLevel = LogLevel.ERROR,
                Exception = exception,
            };
            service.Print(info);
        }

        /// <summary>
        /// 向日志服务记录严重错误信息
        /// </summary>
        /// <param name="service">日志服务接口</param>
        /// <param name="message">严重错误信息内容</param>
        /// <param name="sourceType">日志来源类型</param>
        /// <param name="exception">异常对象</param>
        public static void Fatal(this ILogingService service, string message, Type sourceType, Exception exception)
        {
            service.Fatal(message, sourceType.Name, exception);
        }

        /// <summary>
        /// 向日志服务记录严重错误信息
        /// </summary>
        /// <param name="service">日志服务接口</param>
        /// <param name="message">严重错误信息内容</param>
        /// <param name="source">日志来源</param>
        /// <param name="exception">异常对象</param>
        public static void Fatal(this ILogingService service, string message, string source, Exception exception)
        {
            LogInfo info = new LogInfo()
            {
                Time = DateTime.UtcNow,
                Message = message,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Source = source,
                LogLevel = LogLevel.FATAL,
                Exception = exception,
            };
            service.Print(info);
        }
    }
}
