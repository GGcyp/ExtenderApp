using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 启动执行基类
    /// </summary>
    public abstract class StartupExecute : DisposableObject, IStartupExecute
    {
        public abstract ValueTask ExecuteAsync();
    }
}
