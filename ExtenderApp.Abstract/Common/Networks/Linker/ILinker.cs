using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 网络链接器接口，表示可发起并管理网络连接的低层抽象。
    /// <para>组合了链接信息、接收、发送与连接控制等能力，并提供套接字选项配置方法。</para>
    /// </summary>
    public interface ILinker : IDisposable, ILinkInfo, ILinkReceiver, ILinkSender, ILinkConnect, ILinkBind, ILinkOption
    {
        /// <summary>
        /// 克隆当前链接器实例（浅拷贝/仅复位数据，不执行网络操作）。
        /// <para>返回的实例应与原实例相互独立，且不应自动开启连接或共享运行时状态。</para>
        /// </summary>
        /// <returns>克隆后的 <see cref="ILinker"/> 实例。</returns>
        ILinker Clone();
    }
}