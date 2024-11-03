using System.Reflection;

namespace AppHost.Extensions.Hosting
{
    public static class HostEnvironmentBuilder
    {
        public static IHostEnvironment CreateEnvironment()
        {
            return new HostEnvironment()
            {
                ApplicationName = Assembly.GetEntryAssembly()!.FullName!,
                ContentRootPath = Directory.GetCurrentDirectory(),
            };
        }
    }
}
