

namespace AppHost.Extensions.DependencyInjection
{
    public class SericeBuilder
    {
        public static IServiceCollection CreateServiceCollection()
        {
            return new ServiceCollection();
        }
    }
}
