using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;


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
        /// 获取过场动画视图类型
        /// </summary>
        /// <returns>返回过场动画视图类型，如果没有则为null</returns>
        public virtual Type? CutsceneViewType { get; }

        public override void ConfigureScopeOptions(ScopeOptionsBuilder builder)
        {
            builder.ScopeName = GetType().Name;
        }

        /// <summary>
        /// 配置二进制格式化程序存储
        /// </summary>
        /// <param name="store">二进制格式化程序存储</param>
        public virtual void ConfigureBinaryFormatterStore(IBinaryFormatterStore store)
        {

        }

        /// <summary>
        /// 配置插件详细信息。
        /// 可在插件启动时对 <see cref="PluginDetails"/> 实例进行自定义设置，
        /// 如插件标题、描述、版本、启动类型、动画视图类型、插件作用域等元数据。
        /// 子类可重写此方法以实现插件元数据的初始化、扩展或动态调整。
        /// </summary>
        /// <param name="details">待配置的插件详细信息对象。</param>
        public virtual void ConfigureDetails(PluginDetails details)
        {
        }
    }
}
