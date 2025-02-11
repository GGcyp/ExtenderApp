using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;


namespace ExtenderApp.Services
{
    /// <summary>
    /// 插件实体启动类，继承自 ScopeStartup 类
    /// </summary>
    public abstract class PluginEntityStartup : ScopeStartup
    {
        /// <summary>
        /// 获取启动类型
        /// </summary>
        /// <returns>启动类型</returns>
        public abstract Type StartType { get; }

        /// <summary>
        /// 配置作用域选项
        /// </summary>
        /// <param name="options">作用域选项</param>
        public override void ConfigureScopeOptions(ScopeOptions options)
        {
            options.ScopeName = GetType().Name;
        }

        /// <summary>
        /// 配置二进制格式化程序存储
        /// </summary>
        /// <param name="store">二进制格式化程序存储</param>
        public virtual void ConfigureBinaryFormatterStore(IBinaryFormatterStore store)
        {

        }
    }
}
