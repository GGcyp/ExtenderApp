using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端链路的基础接口。
    /// <para>定义了客户端最核心的资源释放行为以及帧器（Framer）的管理能力。</para>
    /// </summary>
    public interface ILinkClient : IDisposable, ILinkInfo, ILinkConnect, ILinkOption, ILinkClientPipeline
    {
        Result<LinkOperationValue> SendAsync<T>(T value, CancellationToken token = default);
    }
}