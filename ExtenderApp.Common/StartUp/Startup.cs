using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 启动类基类
    /// </summary>
    public abstract class Startup
    {
        /// <summary>
        /// 启动类类型
        /// </summary>
        private static Type m_StartupType;

        /// <summary>
        /// 获取启动类类型
        /// </summary>
        public static Type Type
        {
            get
            {
                if (m_StartupType == null)
                {
                    m_StartupType = typeof(Startup);
                }
                return m_StartupType;
            }
        }

        /// <summary>
        /// 添加服务
        /// </summary>
        /// <param name="services">服务集合</param>
        public virtual void AddService(IServiceCollection services)
        {
        }
    }
}