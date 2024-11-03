using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppHost.Extensions.Hosting
{
    public class HostEnvironment : IHostEnvironment
    {
        private string _applicationName;
        public string ApplicationName
        {
            get => _applicationName;
            set => _applicationName = value;
        }

        private string _contentRootPath;
        public string ContentRootPath
        {
            get => _contentRootPath;
            set => _contentRootPath = value;
        }

        private string _environmentName;
        public string EnvironmentName
        {
            get => _environmentName;
            set => _environmentName = value;
        }
    }
}
