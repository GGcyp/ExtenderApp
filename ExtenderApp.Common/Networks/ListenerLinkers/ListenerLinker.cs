using System.Net;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 一个抽象的监听器连接器类，实现了 <see
    /// cref="IListenerLinker{T}"/> 接口。
    /// </summary>
    /// <typeparam name="T">
    /// 泛型参数，表示连接器类型，必须实现 <see cref="ILinker"/> 接口。
    /// </typeparam>
    public abstract class ListenerLinker<T> : DisposableObject, IListenerLinker<T>
        where T : ILinker
    {
        /// <summary>
        /// 私有只读属性，表示链接器工厂。
        /// </summary>
        protected ILinkerFactory<T> linkerFactory;

        /// <summary>
        /// 获取监听点的端点信息。
        /// </summary>
        public abstract EndPoint? ListenerPoint { get; }

        public event EventHandler<T>? OnAccept;
        public void RaiseOnAccept(T linker)
        {
            OnAccept?.Invoke(this, linker);
        }

        public ListenerLinker(ILinkerFactory<T> linkerFactory)
        {
            this.linkerFactory = linkerFactory;
        }

        #region 子类实现

        public abstract void Bind(EndPoint endPoint);

        public abstract void Listen(int backlog = 10);

        #endregion
    }
}