using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// TCP 监听器链接器的抽象基类，实现 <see cref="ITcpListenerLinker"/>。
    /// 封装 <see cref="ILinkerFactory{TLinker}"/> 注入与 <see cref="OnAccept"/> 事件分发。
    /// </summary>
    public abstract class TcpListenerLinker : DisposableObject, ITcpListenerLinker
    {
        /// <summary>
        /// 用于将已接入的底层 <see cref="System.Net.Sockets.Socket"/> 包装为 <see cref="ITcpLinker"/> 的工厂。
        /// </summary>
        protected ILinkerFactory<ITcpLinker> linkerFactory;

        /// <summary>
        /// 获取监听器的本地终结点（本地地址与端口）。
        /// 在成功绑定并开始监听后有效；未绑定或未监听时可能为 null。
        /// </summary>
        public abstract EndPoint? ListenerPoint { get; }

        /// <summary>
        /// 新连接接入事件。事件参数为包装完成的 <see cref="ITcpLinker"/>。
        /// </summary>
        public event EventHandler<ITcpLinker>? OnAccept;

        /// <summary>
        /// 触发 <see cref="OnAccept"/> 事件的受保护辅助方法。
        /// </summary>
        /// <param name="linker">已包装的连接器实例。</param>
        public void RaiseOnAccept(ITcpLinker linker)
        {
            OnAccept?.Invoke(this, linker);
        }

        /// <summary>
        /// 使用指定链接器工厂构造监听器。
        /// </summary>
        /// <param name="linkerFactory">用于构造 <see cref="ITcpLinker"/> 的工厂。</param>
        public TcpListenerLinker(ILinkerFactory<ITcpLinker> linkerFactory)
        {
            this.linkerFactory = linkerFactory;
        }

        #region 子类实现

        /// <summary>
        /// 绑定本地终结点。
        /// </summary>
        /// <param name="endPoint">要绑定的本地地址与端口。</param>
        public abstract void Bind(EndPoint endPoint);

        /// <summary>
        /// 开始监听传入连接。
        /// </summary>
        /// <param name="backlog">挂起连接队列的最大长度。</param>
        public abstract void Listen(int backlog = 10);

        #endregion
    }
}