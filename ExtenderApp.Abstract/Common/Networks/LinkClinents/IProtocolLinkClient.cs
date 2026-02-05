namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 标准协议客户端接口。
    /// <para>专注于自定义插件逻辑和协议头部的正常处理，仅支持基础字节流发送。</para>
    /// </summary>
    public interface IProtocolLinkClient : ILinkClient<IProtocolLinkClient>, ILinker
    {
    }

    public interface IProtocolLinkClient<TLinker> : IProtocolLinkClient
        where TLinker : ILinker
    {
    }
}