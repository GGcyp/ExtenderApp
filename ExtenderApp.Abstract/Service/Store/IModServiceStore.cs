using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示一个范围服务存储的接口，继承自<see cref="IServiceStore"/>接口。
    /// </summary>
    public interface IModServiceStore : IServiceStore
    {
        /// <summary>
        /// 获取当前范围服务的模块详细信息。
        /// </summary>
        /// <value>返回当前范围服务的模块详细信息。</value>
        public ModDetails ModDetails { get; }
    }
}
