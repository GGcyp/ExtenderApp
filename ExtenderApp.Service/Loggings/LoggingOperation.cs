using System.Diagnostics;
using System.Text;
using ExtenderApp.Common;
using ExtenderApp.Data;

namespace ExtenderApp.Services.Logging
{
    internal class LoggingOperation : ConcurrentOperation<LoggingData>
    {
        private LogInfo info;

        public void SetLogInfo(LogInfo logInfo)
        {
            info = logInfo;
        }

        public override void Execute(LoggingData item)
        {
            var pathProvider = item.PathProvider;
            var stream = item.StreamWriter;
            var fileTime = item.CurrentFiletime;

            if (stream == null || 
                info.Time.Year != fileTime.Year || 
                info.Time.Month != fileTime.Month || 
                info.Time.Day != fileTime.Day)
            {
                var tempFiletime = info.Time.ToString("yyyyMMdd");
                var fileName = string.Concat(tempFiletime, FileExtensions.LogFileExtensions);
                var filepath = Path.Combine(pathProvider.LoggingPath, fileName);
                fileTime = DateTime.UtcNow;

                stream?.Close();
                stream?.Dispose();
                stream = null;

                try
                {
                    stream = File.Exists(filepath) ? File.AppendText(filepath) : File.CreateText(filepath);
                    stream.AutoFlush = true;
                }
                catch (Exception ex)
                {
                    //this.Error($"无法创建或打开日志文件: {ex.Message}", typeof(ILogingService), ex);
#if DEBUG
                    Debug.Print(ex.Message);
#endif
                }
            }


            item.StreamWriter = stream;
            item.CurrentFiletime = fileTime;
            var logInfoMessage = LogInfoToStringBuilder(item.LogText, info);
            stream!.WriteLine(logInfoMessage);
#if DEBUG
            Debug.Print(logInfoMessage);
#endif
        }

        /// <summary>
        /// 将日志信息转换为字符串构建器中的字符串
        /// </summary>
        /// <param name="info">日志信息对象</param>
        /// <returns>包含日志信息的字符串</returns>
        private string LogInfoToStringBuilder(StringBuilder stringBuilder, LogInfo info)
        {
            stringBuilder.Append($"时间: {info.Time.ToString("yyyy-MM-dd HH:mm:ss")}");
            stringBuilder.Append($"，线程ID: {info.ThreadId}");
            stringBuilder.Append($"，日志级别: {info.LogLevel}");
            stringBuilder.Append($"，消息源: {info.Source}");
            stringBuilder.Append($"，消息: {info.Message}");

            if (info.Exception != null)
            {
                stringBuilder.Append("，异常详情: ");
                stringBuilder.AppendLine();
                stringBuilder.Append(info.Exception.Message);
                stringBuilder.Append($"，异常堆栈: {info.Exception.StackTrace}");
            }

            var result = stringBuilder.ToString();
            stringBuilder.Clear(); // 清空StringBuilder以便下次使用

            return result;
        }

        public override bool TryReset()
        {
            info = new LogInfo();
            return true;
        }
    }
}
