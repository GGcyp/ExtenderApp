using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtenderApp.Data
{
    public struct LogInfo
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 线程id
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// 异常源
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 异常对象
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 将LogInfo结构体转换为字符串形式
        /// </summary>
        /// <returns>格式化后的字符串表示</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"时间: {Time.ToString("yyyy-MM-dd HH:mm:ss")}");
            sb.Append($"，线程ID: {ThreadId}");
            sb.Append($"，日志级别: {LogLevel}");
            sb.Append($"，异常源: {Source}");
            sb.Append($"，异常信息: {Message}");

            if (Exception != null)
            {
                sb.Append($"，异常详情: {Exception.Message}");
                sb.Append($"，异常堆栈: {Exception.StackTrace}");
            }

            return sb.ToString();
        }
    }
}
