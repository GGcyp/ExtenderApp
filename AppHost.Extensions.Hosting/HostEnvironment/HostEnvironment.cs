using System.Reflection;

namespace AppHost.Extensions.Hosting
{
    internal class HostEnvironment : IHostEnvironment
    {
        public string ApplicationName { get; set; }

        public string ContentRootPath { get; set; }

        public string EnvironmentName { get; set; }

        public HostEnvironment(string applicationName, string contentRootPath, string environmentName)
        {
            ApplicationName = applicationName;
            ContentRootPath = contentRootPath;
            EnvironmentName = environmentName;
        }
    }
}
