

using Microsoft.Extensions.DependencyInjection;

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 服务生成器
    /// </summary>
    public class ServiceBuilder
    {
        /// <summary>
        /// 创建服务集合
        /// </summary>
        /// <returns>创建好的服务集合</returns>
        public static IServiceCollection CreateServiceCollection()
        {
            return new ServiceCollection();
        }
    }
}
