using System.Reflection;

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 服务构建详情类
    /// </summary>
    internal class ServiceConstructorDetail
    {
        /// <summary>
        /// 服务类构造信息
        /// </summary>
        public ConstructorInfo ServiceConstructorInfo { get; }

        /// <summary>
        /// 构造所需要的传入参数
        /// </summary>
        public ServiceConstructorDetail[]? ConstructorDetails { get; private set; }

        /// <summary>
        /// 服务描述
        /// </summary>
        public ServiceDescriptor ServiceDescriptor { get; }

        /// <summary>
        /// 服务实例对象
        /// </summary>
        private object? _serviceInstance;

        public ServiceConstructorDetail(ServiceDescriptor serviceDescriptor) : this(null, null, serviceDescriptor)
        {

        }

        public ServiceConstructorDetail(ConstructorInfo serviceConstructorInfo) : this(serviceConstructorInfo, null, null)
        {

        }

        public ServiceConstructorDetail(ConstructorInfo serviceConstructorInfo, ServiceConstructorDetail[] constructorDetails, ServiceDescriptor serviceDescriptor)
        {
            ServiceConstructorInfo = serviceConstructorInfo;
            ConstructorDetails = constructorDetails;
            ServiceDescriptor = serviceDescriptor;
        }

        /// <summary>
        /// 获得服务
        /// </summary>
        /// <returns></returns>
        public object? GetService(IServiceProvider provider)
        {
            if (ServiceDescriptor == null)
            {
                if (ServiceConstructorInfo == null)
                {
                    return null;
                }

                if (ConstructorDetails == null)
                {
                    return GetDefaultService();
                }
                else
                {
                    return CreateService(provider);
                }
            }

            if (ServiceDescriptor.HasFactory)
            {
                return GetServiceForFactory(provider);
            }

            return GetRegisterService(provider);
        }

        /// <summary>
        /// 创建一个服务实例。
        /// </summary>
        /// <param name="provider">服务提供者。</param>
        /// <returns>返回创建的服务实例。</returns>
        private object CreateService(IServiceProvider provider)
        {
            //如果这个类的构建函数需要其他服务作为参数，则去查找并放入
            object?[]? parameters = null;
            if (ConstructorDetails.Length > 0)
            {
                parameters = new object[ConstructorDetails.Length];
                for (int i = 0; i < ConstructorDetails.Length; i++)
                {
                    parameters[i] = ConstructorDetails[i]?.GetService(provider);
                }
            }

            return ServiceConstructorInfo.Invoke(parameters);
        }

        /// <summary>
        /// 获取已经被注册的服务
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private object GetRegisterService(IServiceProvider provider)
        {
            if (ServiceDescriptor!.ImplementationInstance != null)
            {
                //如果是单例类，则直接返回单例
                return ServiceDescriptor.ImplementationInstance;
            }
            else if (_serviceInstance != null)
            {
                //如果是单例类，则直接返回单例
                return _serviceInstance;
            }


            object result = ConstructorDetails == null ? GetDefaultService() : CreateService(provider);

            if (ServiceDescriptor.Lifetime == ServiceLifetime.Singleton || ServiceDescriptor.Lifetime == ServiceLifetime.Scoped)
            {
                //如果是单例则保存
                //ServiceDescriptor.ImplementationInstance = result;
                _serviceInstance = result;
                ConstructorDetails = null; //清除构造参数信息，减少内存占用
            }

            return result;
        }

        /// <summary>
        /// 获取没有注册，但无构造函数的服务类
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private object GetDefaultService()
        {
            return ServiceConstructorInfo.Invoke(null);
        }

        /// <summary>
        /// 通过工厂创建的服务
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        private object GetServiceForFactory(IServiceProvider provider)
        {
            if (ServiceDescriptor.IsKeyedService)
            {
                return ServiceDescriptor.KeyedImplementationFactory!.Invoke(provider, ServiceDescriptor.ServiceKey);
            }
            return ServiceDescriptor!.ImplementationFactory!.Invoke(provider);
        }
    }
}
