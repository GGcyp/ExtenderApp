
namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 定义一个接口 ITentativeProvider，继承自 IServiceProvider 接口。
    /// </summary>
    /// <remarks>
    /// 从已经注册的服务中获取服务进行创建，不保存,只能创建实例
    /// </remarks>
    public interface ITentativeProvider : IServiceProvider
    {
    }
}
