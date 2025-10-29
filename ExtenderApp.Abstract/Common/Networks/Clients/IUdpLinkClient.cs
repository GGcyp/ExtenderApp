

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// UDP 客户端侧的链路抽象，扩展自 <see cref="ILinkClientAwareSender{TUdpLinker}"/>，
    /// </summary>
    public interface IUdpLinkClient : ILinkClientAwareSender<IUdpLinkClient>
    {

    }
}
