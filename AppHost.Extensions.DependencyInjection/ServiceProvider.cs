using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Reflection;

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 服务提供者类，用于实现IServiceProvider接口，提供依赖注入服务。
    /// </summary>
    public class ServiceProvider : IServiceProvider
    {
        /// <summary>
        /// 所有依赖注入关系字典
        /// </summary>
        private readonly Dictionary<Type, ServiceDescriptor> _serviceDescriptorDict;

        /// <summary>
        /// 并发字典，存储服务构造详情
        /// </summary>
        private readonly ConcurrentDictionary<Type, ServiceConstructorDetail> _serviceConstructorDetailsDict;

        public ServiceProvider(IServiceCollection services)
        {
            services.AddSingleton<IServiceProvider>(this);
            services.AddSingleton<IScopeExecutor>(new ScopeExecutor(AddService, RemoveService));

            //因为目前只有获取，所以先不进行多线程的保护
            _serviceDescriptorDict = services.ToDictionary(sd => sd.ServiceType, sd => sd);
            _serviceConstructorDetailsDict = new();
        }

        public object? GetService(Type serviceType)
        {
            object? result = null;
            try
            {
                result = CreateOrGetServiceConstructorDetail(serviceType)?.GetService(this);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
            return result;
        }

        #region 基础服务

        /// <summary>
        /// 获取服务构建详情
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns>返回ServiceConstructorDetail类型的服务构建详情</returns>
        private ServiceConstructorDetail CreateOrGetServiceConstructorDetail(Type serviceType)
        {
            //查询是否已经装进字典的注册服务构造详情
            if (_serviceConstructorDetailsDict.TryGetValue(serviceType, out var detail))
            {
                return detail;
            }

            //检查是否是泛型
            if (serviceType.IsGenericType)
            {
                detail = CreateGenericServiceConstructorDetail(serviceType);
            }
            else
            {
                detail = CreateServiceConstructorDetail(serviceType);
            }

            return detail;
        }

        /// <summary>
        /// 创建泛型服务
        /// </summary>
        /// <param name="serviceGenericType">泛型服务类型</param>
        /// <returns>返回ServiceConstructorDetail类型的泛型服务构建详情</returns>
        private ServiceConstructorDetail CreateGenericServiceConstructorDetail(Type serviceGenericType)
        {
            //整个泛型类是否已经被注册
            //已经被注册则就以整个类型进行创建构造类
            ServiceConstructorDetail detail = CreateServiceConstructorDetail(serviceGenericType);
            if (detail != null) return detail;

            //获取泛型基础定义
            Type serviceGenericTypeDefinition = serviceGenericType.GetGenericTypeDefinition();

            //服务是否被注册
            if (_serviceDescriptorDict.TryGetValue(serviceGenericTypeDefinition, out var descriptor))
            {
                // 获取泛型类型参数
                Type[] serviceTypeArguments = serviceGenericType.GetGenericArguments();

                //如果是IEnumerable<>类型的
                if (descriptor.ImplementationType == null && descriptor.HasFactory)
                {
                    descriptor.ServiceKey = serviceTypeArguments;
                    return new ServiceConstructorDetail(descriptor);
                }

                serviceGenericType = descriptor.ImplementationType!.MakeGenericType(serviceTypeArguments);

            }
            else
            {
                //如果是接口或是抽象类者返回空值
                if (serviceGenericType.IsAbstract) throw new InvalidOperationException($"The serviceType unregistered {serviceGenericType}");
            }

            ConstructorInfo constructorInfo = serviceGenericType.GetConstructors().FirstOrDefault();
            //获取已经注册的服务类构造信息，或者是无参数构造参数
            ServiceConstructorDetail[] details = GetServiceConstructorParameterInfoDetails(constructorInfo.GetParameters());

            detail = new ServiceConstructorDetail(constructorInfo, details, descriptor);
            _serviceConstructorDetailsDict.TryAdd(serviceGenericType, detail);
            return detail;
        }

        /// <summary>
        /// 创建服务构造详情类
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns>返回ServiceConstructorDetail类型的服务构造详情</returns>
        private ServiceConstructorDetail CreateServiceConstructorDetail(Type serviceType)
        {
            ServiceConstructorDetail detail;
            ConstructorInfo constructorInfo;
            //查看是否是注册服务类
            if (_serviceDescriptorDict.TryGetValue(serviceType, out var descriptor))
            {
                if (descriptor.HasFactory)
                {
                    detail = new ServiceConstructorDetail(descriptor);
                    _serviceConstructorDetailsDict.TryAdd(serviceType, detail);
                    return detail;
                }
                else
                {
                    constructorInfo = descriptor.ImplementationType.GetConstructors().FirstOrDefault();
                }
            }
            else
            {
                //不是注册服务类,是实例类，同时无参数构造参数
                if (serviceType.IsAbstract)
                {
                    if (!serviceType.IsGenericType) throw new InvalidOperationException($"The serviceType unregistered {serviceType}");
                    return null;
                }
                constructorInfo = serviceType.GetConstructors().FirstOrDefault();
            }

            //获取已经注册的服务类构造信息，或者是无参数构造参数
            ServiceConstructorDetail[] details = GetServiceConstructorParameterInfoDetails(constructorInfo?.GetParameters());

            detail = new ServiceConstructorDetail(constructorInfo, details, descriptor);
            _serviceConstructorDetailsDict.TryAdd(serviceType, detail);
            return detail;
        }

        /// <summary>
        /// 获取服务构造函数参数列表
        /// </summary>
        /// <param name="parameterInfos">构造函数参数信息数组</param>
        /// <returns>返回ServiceConstructorDetail类型的数组，包含构造函数参数的服务构建详情</returns>
        private ServiceConstructorDetail[] GetServiceConstructorParameterInfoDetails(ParameterInfo[]? parameterInfos)
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

                    //没有在已注册的可创建类中发现
                    if (detail == null)
                    {
                        //无默认值的,无构造参数
                        if (!parameterInfo.HasDefaultValue) throw new ArgumentNullException(parameterInfo.Name);

                        detail = new ServiceConstructorDetail(parameterType.GetConstructors().FirstOrDefault());
                        //m_ServiceConstructorDetailsDict.TryAdd(parameterType, detail);
                    }
                    details[i] = detail;
                }
            }
            return details;
        }

        #endregion

        #region Scope

        private ServiceDescriptor? AddService(ServiceDescriptor item)
        {
            if (!_serviceDescriptorDict.TryGetValue(item.ServiceType, out ServiceDescriptor? originaService))
            {
                _serviceDescriptorDict.Add(item.ServiceType, item);
                return null;
            }

            _serviceDescriptorDict[item.ServiceType] = item;
            _serviceConstructorDetailsDict.Remove(item.ServiceType, out var temp2);
            return originaService;
        }

        private void RemoveService(ServiceDescriptor scope, ServiceDescriptor originaService)
        {
            if (!_serviceDescriptorDict.TryGetValue(scope.ServiceType, out ServiceDescriptor? scopeService))
            {
                _serviceConstructorDetailsDict.Remove(scope.ServiceType, out var temp);
                return;
            }

            _serviceDescriptorDict[scope.ServiceType] = originaService;
            _serviceConstructorDetailsDict.Remove(scope.ServiceType, out var temp2);
        }

        #endregion
    }
}
