

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 启动执行器接口。实现者用于在应用启动阶段执行一次性的初始化逻辑（例如注册、预热、加载资源等）。
    /// </summary>
    public interface IStartupExecuter : IStartupExecute, IDisposable, IAsyncDisposable
    {

    }
}