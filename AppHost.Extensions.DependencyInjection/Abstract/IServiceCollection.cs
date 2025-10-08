using System.Collections;

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 服务收集接口
    /// </summary>
    public interface IServiceCollection : IList<ServiceDescriptor>, ICollection<ServiceDescriptor>, IEnumerable<ServiceDescriptor>, IEnumerable
    {

    }
}
