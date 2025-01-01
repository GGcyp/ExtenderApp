

namespace AppHost.Extensions.DependencyInjection
{
    public class ServiceBuilder
    {
        public static IServiceCollection CreateServiceCollection()
        {
            return new ServiceCollection();
        }
    }
}
