using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions.Configuration;

namespace AppHost.Extensions.Configuration
{
    public class ConfigurationManager : IConfigurationManager
    {
        private readonly Dictionary<Type, object> _configurations = new Dictionary<Type, object>();

        public ConfigurationManager()
        {
            _configurations = new();
        }

        public void AddConfiguration<T>(T configuration) where T : class
        {
            throw new NotImplementedException();
        }

        public object? GetConfiguration(Type keyType)
        {
            throw new NotImplementedException();
        }
    }
}
