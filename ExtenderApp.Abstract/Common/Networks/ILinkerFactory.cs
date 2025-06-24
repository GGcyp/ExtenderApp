

using System.Net.Sockets;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 链接器工厂接口
    /// </summary>
    public interface ILinkerFactory
    {
        /// <summary>
        /// 创建一个类型为 T 的 Linker 对象。
        /// </summary>
        /// <typeparam name="T">必须实现 ILinker 接口的类型。</typeparam>
        /// <returns>返回类型为 T 的 Linker 对象。</returns>
        T CreateLinker<T>() where T : ILinker;

        /// <summary>
        /// 创建一个指定类型的连接器实例。
        /// </summary>
        /// <typeparam name="T">连接器的类型，必须实现ILinker接口。</typeparam>
        /// <param name="socket">与连接器关联的Socket对象，可以为null。</param>
        /// <returns>返回指定类型的连接器实例。</returns>
        T CreateLinker<T>(Socket? socket) where T : ILinker;
    }
}
