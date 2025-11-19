

using System.Net;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 支持绑定本地终结点的链路接口。
    /// </summary>
    public interface ILinkBind
    {
        /// <summary>
        /// 绑定本地终结点（地址+端口）。
        /// </summary>
        /// <param name="endPoint">要绑定的本地地址与端口（如 127.0.0.1:12345）。</param>
        /// <remarks>
        /// <para>- 未连接模式下用于接收任意来源数据报；也可用于在已连接模式下指定固定的本地端口。</para>
        /// <para>- 一般应在首次收发前调用一次；重复绑定将抛出异常。</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">endPoint 为空。</exception>
        /// <exception cref="System.Net.Sockets.SocketException">底层套接字绑定失败（端口占用、权限不足、地址族不匹配等）。</exception>
        void Bind(EndPoint endPoint);
    }
}
