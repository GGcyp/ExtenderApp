
namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 依赖注入重启器
    /// </summary>
    public class ResetDependencyOrgan
    {
        private readonly Action<IServiceCollection> _action;
        public IServiceCollection Services { get; }

        public ResetDependencyOrgan(IServiceCollection services, Action<IServiceCollection> action)
        {
            Services = services;
            _action = action;
        }

        /// <summary>
        /// 重置依赖注入服务
        /// </summary>
        public void Reset()
        {
            _action?.Invoke(Services);
        }
    }
}
