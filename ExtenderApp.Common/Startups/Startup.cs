using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 库启动基类
    /// </summary>
    public abstract class Startup : IStartup
    {
        ///<inheritdoc/>
        public abstract void AddService(IServiceCollection services);
    }
}