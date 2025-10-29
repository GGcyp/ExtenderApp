
namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示一个可进行“连接 / 断开 / 发送 / 接收”的链路抽象。
    /// </summary>
    public interface ILinker : IDisposable, ILinkInfo, ILinkReceiver, ILinkSender, ILinkConnect
    {

    }
}