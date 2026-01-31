using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// UDP 链接器相关的扩展方法集合。
    /// </summary>
    public static class UdpLinkerExtensions
    {
        /// <summary>
        /// 将 IUdpLinker 服务及其工厂注册到 <see cref="IServiceCollection"/> 中。
        /// </summary>
        /// <param name="services">要注册服务的 <see cref="IServiceCollection"/> 实例。</param>
        /// <returns>返回传入的 <see cref="IServiceCollection"/>，以便链式调用。</returns>
        public static IServiceCollection AddUdpLinker(this IServiceCollection services)
        {
            services.AddLinker<IUdpLinker, UdpLinkerFactory>();
            return services;
        }

        /// <summary>
        /// 将当前 UDP 链接器加入指定的组播地址（加入组播组）。
        /// </summary>
        /// <param name="udpLinker">目标 <see cref="IUdpLinker"/> 实例。</param>
        /// <param name="multicastAddr">要加入的组播 <see cref="IPAddress"/>。</param>
        /// <returns>返回原始的 <see cref="IUdpLinker"/> 实例以支持方法链式调用。</returns>
        /// <remarks>
        /// 根据链接器的地址族（IPv4/IPv6）选择相应的组播选项类型（ <see cref="MulticastOption"/> 或 <see cref="IPv6MulticastOption"/>）， 并通过 <see
        /// cref="IUdpLinker.SetOption"/> 将选项设置到底层套接字。
        /// </remarks>
        public static IUdpLinker AddMulticastGroup(this IUdpLinker udpLinker, IPAddress multicastAddr)
        {
            // 根据不同的地址族设置组播成员资格
            LinkOptionLevel optionLevel = LinkOptionLevel.IP;
            object optionValue;
            if (udpLinker.AddressFamily == AddressFamily.InterNetwork)
            {
                // 对于IPv4地址，创建并应用组播选项
                optionValue = new MulticastOption(multicastAddr);
            }
            else
            {
                // 对于IPv6地址，创建并应用组播选项
                optionValue = new IPv6MulticastOption(multicastAddr);
            }
            udpLinker.SetOption(optionLevel, LinkOptionName.AddMembership, DataBuffer<object>.Get(optionValue));
            return udpLinker;
        }

        /// <summary>
        /// 将当前 UDP 链接器从指定的组播地址中移除（退出组播组）。
        /// </summary>
        /// <param name="udpLinker">目标 <see cref="IUdpLinker"/> 实例。</param>
        /// <param name="multicastAddr">要移除的组播 <see cref="IPAddress"/>。</param>
        /// <returns>返回原始的 <see cref="IUdpLinker"/> 实例以支持方法链式调用。</returns>
        /// <remarks>
        /// 根据链接器的地址族（IPv4/IPv6）选择相应的组播选项类型（ <see cref="MulticastOption"/> 或 <see cref="IPv6MulticastOption"/>）， 并通过 <see
        /// cref="IUdpLinker.SetOption"/> 将移除组播的选项设置到底层套接字。
        /// </remarks>
        public static IUdpLinker DropMulticastGroup(this IUdpLinker udpLinker, IPAddress multicastAddr)
        {
            // 根据不同的地址族设置组播成员资格
            LinkOptionLevel optionLevel = LinkOptionLevel.IP;
            object optionValue;
            if (udpLinker.AddressFamily == AddressFamily.InterNetwork)
            {
                // 对于IPv4地址，创建并应用组播选项
                optionValue = new MulticastOption(multicastAddr);
            }
            else
            {
                // 对于IPv6地址，创建并应用组播选项
                optionValue = new IPv6MulticastOption(multicastAddr);
            }
            udpLinker.SetOption(optionLevel, LinkOptionName.DropMembership, DataBuffer<object>.Get(optionValue));
            return udpLinker;
        }
    }
}