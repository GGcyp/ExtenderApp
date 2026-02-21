using System.Net;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// Tcp 链接客户端接口：表示一个可进行 TCP 连接的客户端抽象。
    /// </summary>
    public interface ITcpLinkClient : ILinkClient, ITcpLink
    {
    }
}