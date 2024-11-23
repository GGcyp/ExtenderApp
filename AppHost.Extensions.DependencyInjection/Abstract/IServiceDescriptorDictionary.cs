using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppHost.Extensions.DependencyInjection
{
    internal interface IServiceDescriptorDictionary : IDictionary<Type, ServiceDescriptor>
    {
    }
}
