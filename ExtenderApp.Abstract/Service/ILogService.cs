using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 日志服务接口
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// 打印日志信息
        /// </summary>
        /// <param name="info">日志信息对象</param>
        void Print(LogInfo info);
    }
}
