using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 启动类基类
    /// </summary>
    public abstract class Startup
    {
        /// <summary>
        /// 获取启动类类型
        /// </summary>
        public static Type StartupType { get; } = typeof(Startup);

        /// <summary>
        /// 添加服务
        /// </summary>
        /// <param name="services">服务集合</param>
        public virtual void AddService(IServiceCollection services)
        {
        }
    }
}