using System.Reflection;

namespace AppHost.Extensions.DependencyInjection
{
    internal class TentativeProvider : ITentativeProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public TentativeProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType.IsAbstract || serviceType.IsEnum) return null;

            ConstructorInfo? constructorInfo = serviceType.GetConstructors().FirstOrDefault();

            return GetService(constructorInfo);
        }

        private object? GetService(ConstructorInfo? constructorInfo)
        {
            if (constructorInfo is null) return null;

            ParameterInfo[]? parameterInfos = constructorInfo.GetParameters();
            if (parameterInfos is null) return null;

            object?[]? parameters = new object[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var parameterInfo = parameterInfos[i];
                Type parameterType = parameterInfo.ParameterType;

                var parameter = _serviceProvider.GetService(parameterType);

                //没有在已注册的可创建类中发现
                if (parameter is null)
                {
                    //无默认值的,无构造参数
                    if (!parameterInfo.HasDefaultValue) 
                        return null;

                    parameter = parameterInfo.DefaultValue;
                }
                parameters[i] = parameter;
            }

            return constructorInfo.Invoke(parameters);
        }
    }
}
