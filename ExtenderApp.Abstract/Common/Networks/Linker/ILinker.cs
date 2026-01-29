namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 网络链接器接口。
    /// </summary>
    public interface ILinker : IDisposable, ILinkInfo, ILinkReceiver, ILinkSender, ILinkConnect
    {
        /// <summary>
        /// 克隆当前链接器实例,仅复置数据不进行操作。
        /// </summary>
        /// <returns>返回克隆后的实例</returns>
        ILinker Clone();
    }
}