using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 库启动接口
    /// </summary>
    public interface IStartup
    {
        /// <summary>
        /// 添加服务
        /// </summary>
        /// <param name="services">服务集合</param>
        public void AddService(IServiceCollection services);
    }
}