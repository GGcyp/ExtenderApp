using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 服务提供者类，用于实现IServiceProvider接口，提供依赖注入服务。
    /// </summary>
    internal class ServiceProvider : IServiceProvider, IDisposable, IAsyncDisposable, IServiceProviderIsService
    {
        /// <summary>
        /// 所有依赖注入关系字典
        /// </summary>
        private readonly FrozenDictionary<Type, ExtenderServiceDescriptor> _serviceDescriptorDict;

        /// <summary>
        /// 并发字典，存储服务构造详情
        /// </summary>
        protected readonly ConcurrentDictionary<Type, ServiceConstructorDetail> _serviceConstructorDetailsDict;

        public ServiceProvider(IServiceCollection services)
        {
            services.AddSingleton<IServiceProvider>(this);
            services.AddSingleton(this.CreateCloser());
            _serviceDescriptorDict = services.ToDictionary(sd => sd.ServiceType, sd => sd).ToFrozenDictionary();
            _serviceConstructorDetailsDict = new();
        }

        public virtual object? GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType), "服务类型不能为空。");
            }

            object? result = null;
            try
            {
                result = CreateOrGetServiceConstructorDetail(serviceType)?.GetService(this);
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        #region 基础服务

        /// <summary>
        /// 获取服务构建详情
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns>返回ServiceConstructorDetail类型的服务构建详情</returns>
        internal ServiceConstructorDetail? CreateOrGetServiceConstructorDetail(Type serviceType)
        {
            ServiceConstructorDetail? result = null;
            //查询是否已经装进字典的注册服务构造详情
            if (_serviceConstructorDetailsDict.TryGetValue(serviceType, out result))
            {
                return result;
            }

            //检查是否是泛型
            if (serviceType.IsGenericType)
            {
                result = CreateGenericServiceConstructorDetail(serviceType);
            }
            else
            {
                result = CreateServiceConstructorDetail(serviceType);
            }


            return result;
        }

        /// <summary>
        /// 创建泛型服务
        /// </summary>
        /// <param name="serviceGenericType">泛型服务类型</param>
        /// <returns>返回ServiceConstructorDetail类型的泛型服务构建详情</returns>
        protected ServiceConstructorDetail? CreateGenericServiceConstructorDetail(Type serviceGenericType)
        {
            //整个泛型类是否已经被注册
            //已经被注册则就以整个类型进行创建构造类
            ServiceConstructorDetail? detail = CreateServiceConstructorDetail(serviceGenericType);
            if (detail != null) return detail;

            //获取泛型基础定义
            Type serviceGenericTypeDefinition = serviceGenericType.GetGenericTypeDefinition();

            //服务是否被注册
            if (_serviceDescriptorDict.TryGetValue(serviceGenericTypeDefinition, out var descriptor))
            {
                // 获取泛型类型参数
                Type[] serviceTypeArguments = serviceGenericType.GetGenericArguments();

                //如果是IEnumerable<>类型的,或是其他默认将他的子类型传入
                if (descriptor.ImplementationType == null && descriptor.IsKeyedService)
                {
                    return new ServiceConstructorDetail(descriptor, serviceTypeArguments);
                }

                serviceGenericType = descriptor.ImplementationType!.MakeGenericType(serviceTypeArguments);
            }
            else
            {
                ////如果是接口或是抽象类者返回空值,因为他们没有注册
                //if (serviceGenericType.IsAbstract)
                //    ThrowInvalidOperation(serviceGenericType.Name);

                return null;
            }

            ConstructorInfo constructorInfo = serviceGenericType.GetConstructors().FirstOrDefault()!;
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
        protected ServiceConstructorDetail? CreateServiceConstructorDetail(Type serviceType)
        {
            ServiceConstructorDetail detail;
            ConstructorInfo? constructorInfo;
            //查看是否是注册服务类
            if (_serviceDescriptorDict.TryGetValue(serviceType, out var descriptor))
            {
                if (descriptor.ImplementationFactory != null)
                {
                    detail = new ServiceConstructorDetail(descriptor);
                    _serviceConstructorDetailsDict.TryAdd(serviceType, detail);
                    return detail;
                }
                else if (descriptor.ImplementationInstance != null)
                {
                    return new ServiceConstructorDetail(descriptor);
                }
                else
                {
                    if (descriptor.ImplementationType is null)
                        throw new ArgumentNullException(string.Format("该服务类型实例为空:{0}", serviceType.Name));

                    constructorInfo = descriptor.ImplementationType.GetConstructors().FirstOrDefault()!;
                }
            }
            else
            {
                //不是注册服务类,不是实例类，同时无参数构造参数
                if (serviceType.IsAbstract)
                {
                    //if (!serviceType.IsGenericType)
                    //    ThrowInvalidOperation(serviceType.Name);

                    return null;
                }
                constructorInfo = serviceType.GetConstructor(Type.EmptyTypes);
            }

            if (constructorInfo == null)
                return null;

            //获取已经注册的服务类构造信息，或者是无参数构造参数
            var parameterInfos = constructorInfo.GetParameters();
            ServiceConstructorDetail[] details = GetServiceConstructorParameterInfoDetails(parameterInfos);

            detail = new ServiceConstructorDetail(constructorInfo!, details, descriptor!);
            _serviceConstructorDetailsDict.TryAdd(serviceType, detail);
            return detail;
        }

        /// <summary>
        /// 获取服务构造函数参数列表
        /// </summary>
        /// <param name="parameterInfos">构造函数参数信息数组</param>
        /// <returns>返回ServiceConstructorDetail类型的数组，包含构造函数参数的服务构建详情</returns>
        protected virtual ServiceConstructorDetail[] GetServiceConstructorParameterInfoDetails(ParameterInfo[]? parameterInfos)
        {
            ServiceConstructorDetail[] details = null;
            if (parameterInfos is null) return details;

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
                    if (!parameterInfo.HasDefaultValue)
                        ThrowInvalidOperation(parameterType.Name);

                    detail = new ServiceConstructorDetail(parameterType.GetConstructors().FirstOrDefault());

                    //不用注册，可能这个无参服务只有他用
                    //m_ServiceConstructorDetailsDict.TryAdd(parameterType, detail);
                }
                details[i] = detail;
            }

            return details;
        }

        protected void ThrowInvalidOperation(string serviceName)
        {
            throw new InvalidOperationException(string.Format("该服务类型未注册:{0}", serviceName));
        }

        #endregion

        /// <summary>
        /// 尝试从服务描述符字典中获取指定类型的服务描述符。
        /// </summary>
        /// <param name="serviceType">要查找的服务类型。</param>
        /// <param name="serviceDescriptor">输出参数，如果找到匹配的服务描述符，则将其赋值给此参数。</param>
        /// <returns>如果找到匹配的服务描述符，则返回 true；否则返回 false。</returns>
        internal bool TryGetServiceDescriptor(Type serviceType, out ExtenderServiceDescriptor? serviceDescriptor)
        {
            return _serviceDescriptorDict.TryGetValue(serviceType, out serviceDescriptor);
        }

        public virtual void Dispose()
        {
            foreach (var item in _serviceConstructorDetailsDict.Values)
            {
                var instance = item.ServiceInstance;
                switch (instance)
                {
                    case null:
                        continue;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                    case IAsyncDisposable asyncDisposable:
                        // 同步上下文下优先保证资源释放到位
                        asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        break;
                }
            }
            _serviceConstructorDetailsDict.Clear();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            List<Task>? asyncTasks = null;

            foreach (var item in _serviceConstructorDetailsDict.Values)
            {
                var instance = item.ServiceInstance;
                if (instance is null) continue;

                if (instance is IAsyncDisposable asyncDisposable)
                {
                    var vt = asyncDisposable.DisposeAsync();
                    if (!vt.IsCompletedSuccessfully)
                    {
                        (asyncTasks ??= new List<Task>()).Add(vt.AsTask());
                    }
                }
                else if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            if (asyncTasks is not null)
            {
                await Task.WhenAll(asyncTasks);
            }

            _serviceConstructorDetailsDict.Clear();
            GC.SuppressFinalize(this);
        }

        public bool IsService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType), "服务类型不能为空。");
            }
            return _serviceDescriptorDict.ContainsKey(serviceType);
        }
    }
}
