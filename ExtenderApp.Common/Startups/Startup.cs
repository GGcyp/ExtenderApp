using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 库启动基类
    /// </summary>
    public abstract class Startup : IStartup
    {
        /// <summary>
        /// 添加服务
        /// </summary>
        /// <param name="services">服务集合</param>
        public virtual void AddService(IServiceCollection services)
        {
        }
    }
}