using ExtenderApp.Abstract;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;
using ExtenderApp.Services.Logging;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 日志服务类，实现了ILogService接口
    /// </summary>
    internal class LoggingService : ConcurrentOperate<LoggingData>, ILogingService
    {
        private static ObjectPool<LoggingOperation> _pool =
            ObjectPool.Create(new SelfResetPooledObjectPolicy<LoggingOperation>());

        public static LoggingOperation Get() => _pool.Get();
        public static void Release(LoggingOperation operation) => _pool.Release(operation);

        /// <summary>
        /// 构造函数，初始化日志服务
        /// </summary>
        /// <param name="service">刷新服务</param>
        /// <param name="environment">主机环境</param>
        public LoggingService(IPathService provider)
        {
            //检查是否存在存放日志文件夹，如不存在则创建
            string logFolderPath = provider.LoggingPath;
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }

            //将数据传递给基类
            LoggingData data = new LoggingData(provider);
            Start(data);
        }

        /// <summary>
        /// 打印日志信息到日志队列
        /// </summary>
        /// <param name="info">日志信息</param>
        public void Print(LogInfo info)
        {
            var operation = Get();
            operation.SetLogInfo(info);
            ExecuteAsync(operation);
        }
    }
}
