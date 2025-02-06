using System.Net;
using System.Net.NetworkInformation;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 表示一个局域网主机信息类，实现了IResettable和IDisposable接口。
    /// </summary>
    public class LANHostInfo : IResettable, IDisposable
    {
        /// <summary>
        /// 用于Ping操作的Ping对象。
        /// </summary>
        private readonly Ping _ping;

        /// <summary>
        /// 获取或设置主机的IP地址。
        /// </summary>
        public IPAddress? IP { get; private set; }

        /// <summary>
        /// 获取发现主机的时间。
        /// </summary>
        public DateTime FoundTime { get; private set; }

        /// <summary>
        /// 获取最后一次与主机连接的时间。
        /// </summary>
        public DateTime LastLinkTime { get; private set; }

        /// <summary>
        /// 获取上次查询主机是否在线
        /// </summary>
        public bool IsOnline { get; private set; }

        public LANHostInfo()
        {
            _ping = new();
        }

        /// <summary>
        /// 同步Ping指定IP地址的主机，并返回Ping结果。
        /// </summary>
        /// <param name="ip">要Ping的IP地址。</param>
        /// <param name="timeout">Ping超时时间，单位为毫秒，默认值为100毫秒。</param>
        /// <returns>PingReply对象，包含Ping结果。</returns>
        /// <exception cref="InvalidOperationException">如果当前局域网内主机还在线，则抛出此异常。</exception>
        public PingReply PingHost(IPAddress ip, int timeout = 100)
        {
            if (IP != null && StillOnline(timeout))
            {
                throw new InvalidOperationException("当前局域网内主机还在线，无法更换主机  当前主机:" + IP.ToString());
            }

            var reply = _ping.Send(ip, 100);
            if (reply.Status != IPStatus.Success) return reply;

            IP = ip;
            FoundTime = DateTime.Now;
            LastLinkTime = FoundTime;
            IsOnline = true;

            return reply;
        }

        /// <summary>
        /// 异步Ping指定IP地址的主机，并返回Ping结果。
        /// </summary>
        /// <param name="ip">要Ping的IP地址。</param>
        /// <param name="timeout">Ping超时时间，单位为毫秒，默认值为100毫秒。</param>
        /// <returns>异步任务，任务结果为PingReply对象，包含Ping结果。</returns>
        /// <exception cref="InvalidOperationException">如果当前局域网内主机还在线，则抛出此异常。</exception>
        public async Task<PingReply> PingHostAsync(IPAddress ip, int timeout = 100)
        {
            if (IP != null && await StillOnlineAsync(timeout))
            {
                throw new InvalidOperationException("当前局域网内主机还在线，无法更换主机  当前主机:" + IP.ToString());
            }

            var reply = await _ping.SendPingAsync(ip, 100);
            if (reply.Status != IPStatus.Success) return reply;

            IP = ip;
            FoundTime = DateTime.Now;
            LastLinkTime = FoundTime;
            IsOnline = true;

            return reply;
        }

        /// <summary>
        /// 检查当前局域网内主机是否在线。
        /// </summary>
        /// <param name="timeout">Ping超时时间，单位为毫秒，默认值为100毫秒。</param>
        /// <returns>如果主机在线，则返回true；否则返回false。</returns>
        public bool StillOnline(int timeout = 100)
        {
            if (IP is null)
            {
                IsOnline = false;
                return IsOnline;
            }

            IsOnline = _ping.Send(IP, timeout).Status == IPStatus.Success;
            return IsOnline;
        }

        /// <summary>
        /// 异步检查当前局域网内主机是否在线。
        /// </summary>
        /// <param name="timeout">Ping超时时间，单位为毫秒，默认值为100毫秒。</param>
        /// <returns>异步任务，任务结果为bool类型，表示主机是否在线。</returns>
        public async Task<bool> StillOnlineAsync(int timeout = 100)
        {
            if (IP is null)
            {
                IsOnline = false;
                return IsOnline;
            }

            var reply = await _ping.SendPingAsync(IP, timeout);
            IsOnline = reply.Status == IPStatus.Success;
            return IsOnline;
        }

        public void Dispose()
        {
            _ping.Dispose();
            IP = null;
            IsOnline = false;
        }

        public bool TryReset()
        {
            IP = null;
            IsOnline = false;
            FoundTime = DateTime.MinValue;
            LastLinkTime = DateTime.MinValue;
            return true;
        }
    }
}
