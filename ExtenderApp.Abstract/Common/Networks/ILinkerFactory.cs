
using System.Net.Sockets;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 链接器工厂接口
    /// </summary>
    public interface ILinkerFactory
    {
        /// <summary>
        /// 获取指定类型的连接器实例。
        /// </summary>
        /// <typeparam name="T">连接器类型，必须实现ILinker接口。</typeparam>
        /// <returns>返回类型为T的连接器实例。</returns>
        T GetLinker<T>() where T : ILinker;

        /// <summary>
        /// 根据协议类型创建链接器
        /// </summary>
        /// <param name="type">协议类型</param>
        /// <returns>返回创建的链接器</returns>
        ILinker CreateLinker(ProtocolType type);


        /// <summary>
        /// 释放连接器实例。
        /// </summary>
        /// <param name="linker">需要释放的连接器实例。</param>
        void ReleaseLinker(ILinker linker);
    }
}
