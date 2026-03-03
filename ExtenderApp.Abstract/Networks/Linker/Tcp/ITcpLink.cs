using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示 TCP 链路的抽象接口。
    /// <para>定义了 TCP 协议特有的配置属性（如 NoDelay）以及针对 IP 地址列表发起连接的操作。</para>
    /// </summary>
    public interface ITcpLink
    {
        /// <summary>
        /// 获取或设置一个值，该值指示连接是否应禁用 Nagle 算法（即启用 TCP_NODELAY）。
        /// </summary>
        /// <value>若为 <c>true</c> 则禁用 Nagle 算法（即减少延迟）；否则为 <c>false</c>。</value>
        bool NoDelay { get; set; }

        /// <summary>
        /// 同步连接到指定 IP 地址列表中的第一个有效终结点。
        /// </summary>
        /// <param name="addresses">目标远端的备选 IP 地址数组。</param>
        /// <param name="port">远端监听的端口号。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="addresses"/> 为 <c>null</c> 时抛出。</exception>
        void Connect(IPAddress[] addresses, int port);

        /// <summary>
        /// 异步连接到指定 IP 地址列表中的第一个有效终结点。
        /// </summary>
        /// <param name="addresses">目标远端的备选 IP 地址数组。</param>
        /// <param name="port">远端监听的端口号。</param>
        /// <param name="token">用于取消连接操作的取消令牌。</param>
        /// <returns>表示异步连接操作的 <see cref="ValueTask"/>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="addresses"/> 为 <c>null</c> 时抛出。</exception>
        ValueTask ConnectAsync(IPAddress[] addresses, int port, CancellationToken token = default);
    }
}