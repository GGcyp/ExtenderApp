


namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 范围描述符类
    /// </summary>
    internal class ScopeDescriptor
    {
        /// <summary>
        /// 服务类型
        /// </summary>
        public Type ServiceType => OriginaService is null ? ScopeService.ServiceType : OriginaService.ServiceType;

        /// <summary>
        /// 被替换的服务
        /// </summary>
        public ServiceDescriptor? OriginaService { get; set; }

        /// <summary>
        /// 作用域内的新服务
        /// </summary>
        public ServiceDescriptor ScopeService { get; }

        public ScopeDescriptor(ServiceDescriptor scopeService)
        {
            ScopeService = scopeService;
            OriginaService = null;
        }
    }
}
