

using System.Reflection;

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 作用域服务提供者类，实现了 <see cref="IServiceProvider"/> 和 <see cref="IScopeServiceProvider"/> 接口。
    /// </summary>
    internal class ScopeServiceProvider : ServiceProvider, IScopeServiceProvider
    {
        /// <summary>
        /// 主作用域的提供者
        /// </summary>
        private readonly ServiceProvider _serviceProvider;
        /// <summary>
        /// 作用域执行器
        /// </summary>
        private readonly IScopeExecutor _scopeExecutor;
        /// <summary>
        /// 作用域选项
        /// </summary>
        public ScopeOptions ScopeOptions { get; private set; }

        /// <summary>
        /// 初始化一个新的 <see cref="ScopeServiceProvider"/> 实例。
        /// </summary>
        /// <param name="executor">作用域执行器</param>
        /// <param name="options">作用域选项</param>
        /// <param name="provider">主服务提供者</param>
        /// <param name="services">服务集合</param>
        public ScopeServiceProvider(IScopeExecutor executor, ScopeOptions options, IServiceProvider provider, IServiceCollection services) : base(services)
        {
            _scopeExecutor = executor;
            ScopeOptions = options;
            _serviceProvider = provider as ServiceProvider;
            if(_serviceProvider == null) throw new InvalidOperationException(nameof(provider));
        }

        public override object? GetService(Type serviceType)
        {
            //如果本作用域里有就直接返回
            var result = base.GetService(serviceType);
            if (result != null) return result;



            return _serviceProvider.GetService(serviceType);
        }

        protected override ServiceConstructorDetail[] GetServiceConstructorParameterInfoDetails(ParameterInfo[]? parameterInfos)
        {
            ServiceConstructorDetail[] details = null;
            if (parameterInfos != null && parameterInfos.Length > 0)
            {
                details = new ServiceConstructorDetail[parameterInfos.Length];
                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    var parameterInfo = parameterInfos[i];
                    Type parameterType = parameterInfo.ParameterType;
                    ServiceConstructorDetail detail = CreateOrGetServiceConstructorDetail(parameterType);
                    if (!detail.IsEmpty)
                    {
                        details[i] = detail;
                        continue;
                    }

                    detail = GetReloScope(parameterType);
                    if (!detail.IsEmpty)
                    {
                        details[i] = detail;
                        continue;
                    }

                    detail = GetMainService(parameterType);
                    if (!detail.IsEmpty)
                    {
                        details[i] = detail;
                        continue;
                    }

                    //没有在已注册的可创建类中发现
                    if (detail == null)
                    {
                        //无默认值的,无构造参数
                        if (!parameterInfo.HasDefaultValue)
                            ThrowInvalidOperation(parameterType.Name);

                        detail = new ServiceConstructorDetail(parameterType.GetConstructors().FirstOrDefault());

                        //不用注册，可能这个无参服务只有他用
                        //m_ServiceConstructorDetailsDict.TryAdd(parameterType, detail);
                    }
                    details[i] = detail;
                }
            }
            return details;
        }

        private ServiceConstructorDetail GetMainService(Type serviceType)
        {
            ServiceConstructorDetail result = _serviceProvider.CreateOrGetServiceConstructorDetail(serviceType);
            return result;
        }

        private ServiceConstructorDetail GetReloScope(Type serviceType)
        {
            ServiceConstructorDetail result = ServiceConstructorDetail.Empty;
            var scopeList = ScopeOptions.ReloScopes;
            for (int i = 0; i < scopeList.Count; i++)
            {
                var provider = _scopeExecutor.GetServiceProvider(scopeList[i]) as ServiceProvider;
                if (provider is null) continue;
                result = provider.CreateOrGetServiceConstructorDetail(serviceType);
                if (result != null) return result;
            }
            return result;
        }

        public override void Dispose()
        {
            base.Dispose();
            ScopeOptions.Dispose();
        }
    }
}
