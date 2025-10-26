using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 在构建链路客户端（LinkClient）时传递的轻量载体，携带依赖注入提供器与格式化器管理器。
    /// </summary>
    /// <remarks>
    /// 该类型为值类型（struct），用于在构建流程中以低开销方式同时传递：
    /// - <see cref="IServiceProvider"/>：用于解析运行时依赖（例如事件处理或额外服务）；  
    /// - <see cref="ILinkClientFormatterManager"/>：管理客户端格式化器的注册与查找，用于消息反序列化前的路由。  
    /// 注意：该结构不会强制成员非空，使用 <see cref="IsEmpty"/> 可快速判断是否已正确初始化。
    /// </remarks>
    public struct FormatterManagerBuilder
    {
        /// <summary>
        /// 运行时服务提供者，供格式化器或管道组件解析依赖项。
        /// </summary>
        public IServiceProvider Provider { get; }

        /// <summary>
        /// 客户端格式化器管理器，用于查询/注册 <see cref="ILinkClientFormatter"/> / <see cref="IClientFormatter{T}"/>.
        /// </summary>
        public ILinkClientFormatterManager Manager { get; }

        /// <summary>
        /// 指示当前构建器是否为空或未初始化（当 <see cref="Provider"/> 或 <see cref="Manager"/> 为 null 时为 true）。
        /// </summary>
        public bool IsEmpty => Manager is null || Provider is null;

        /// <summary>
        /// 初始化一个新的 <see cref="FormatterManagerBuilder"/> 实例。
        /// </summary>
        /// <param name="provider">用于解析依赖的 <see cref="IServiceProvider"/> 实例（可为 null）。</param>
        /// <param name="manager">管理客户端格式化器的 <see cref="ILinkClientFormatterManager"/> 实例（可为 null）。</param>
        public FormatterManagerBuilder(IServiceProvider provider, ILinkClientFormatterManager manager)
        {
            Provider = provider;
            Manager = manager;
        }
    }
}
