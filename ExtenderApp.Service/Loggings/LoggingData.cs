using System.Text;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ConcurrentOperates;

namespace ExtenderApp.Services.Logging
{
    /// <summary>
    /// 日志数据类，继承自ConcurrentOperateData类
    /// </summary>
    internal class LoggingData : ConcurrentOperateData
    {
        /// <summary>
        /// 日志文件夹路径服务接口
        /// </summary>
        /// <remarks>用于获取日志文件夹的路径</remarks>
        public IPathService PathProvider { get; }

        /// <summary>
        /// 日志文本内容
        /// </summary>
        /// <remarks>用于存储日志的文本内容</remarks>
        public StringBuilder LogText { get; }

        /// <summary>
        /// 获取或设置StreamWriter对象
        /// </summary>
        /// <remarks>用于写入日志到文件中</remarks>
        public StreamWriter StreamWriter { get; set; }

        /// <summary>
        /// 获取或设置当前文件的日期和时间。
        /// </summary>
        public DateTime CurrentFiletime { get; set; }

        /// <summary>
        /// LoggingData类的构造函数
        /// </summary>
        /// <param name="pathProvider">日志文件夹路径服务接口</param>
        /// <param name="logText">日志文本内容</param>
        public LoggingData(IPathService pathProvider)
        {
            PathProvider = pathProvider;
            LogText = new();
        }
    }
}
