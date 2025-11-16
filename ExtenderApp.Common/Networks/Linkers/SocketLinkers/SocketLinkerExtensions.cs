using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 为 <see cref="ILinker"/> 提供针对 <see cref="SocketLinker"/> 的便捷扩展方法。
    /// </summary>
    public static class SocketLinkerExtensions
    {
        /// <summary>
        /// 若给定的 <see cref="ILinker"/> 实例为 <see cref="SocketLinker"/>，则返回其内部 <see cref="Socket"/> 实例；否则返回 <c>null</c>。
        /// </summary>
        /// <param name="linker">要检查的链路器实例。</param>
        /// <returns>当 <paramref name="linker"/> 是 <see cref="SocketLinker"/> 时返回其 <see cref="Socket"/>，否则返回 <c>null</c>。</returns>
        public static Socket? GetSocket(this ILinker linker)
        {
            if (linker is not SocketLinker socketLinker)
            {
                return null;
            }
            return socketLinker.Socket;
        }
    }
}