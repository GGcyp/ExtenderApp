

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 网络链接器接口。
    /// </summary>
    public interface ILinker : IDisposable, ILinkInfo, ILinkReceiver, ILinkSender, ILinkConnect
    {

    }
}