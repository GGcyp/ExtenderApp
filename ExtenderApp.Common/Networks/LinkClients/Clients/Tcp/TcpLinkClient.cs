using System.Net;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// Tcp 链路客户端实现。
    /// </summary>
    internal class TcpLinkClient : LinkClientAwareSender<ITcpLinkClient, ITcpLinker>, ITcpLinkClient
    {
        public bool NoDelay
        {
            get => Linker.NoDelay;
            set => Linker.NoDelay = value;
        }

        public TcpLinkClient(ITcpLinker linker) : base(linker)
        {
        }

        public void Connect(IPAddress[] addresses, int port)
        {
            ThrowIfDisposed();
            try
            {
                Linker.Connect(addresses, port);

                var endPoint = Linker.RemoteEndPoint;
                if (endPoint == null)
                    throw new InvalidOperationException("无法获取远程端点信息");

                // 连接成功后启动接收循环
                StartReceive();
                PluginManager?.OnConnected(_thisClient, endPoint, null);
            }
            catch (Exception ex)
            {
                PluginManager?.OnConnected(_thisClient, null, ex);
                throw;
            }
        }

        public async ValueTask ConnectAsync(IPAddress[] addresses, int port, CancellationToken token = default)
        {
            ThrowIfDisposed();

            try
            {
                await Linker.ConnectAsync(addresses, port, token);

                var endPoint = Linker.RemoteEndPoint;
                if (endPoint == null)
                    throw new InvalidOperationException("无法获取远程端点信息");

                // 连接成功后启动接收循环
                StartReceive();
                PluginManager?.OnConnected(_thisClient, endPoint, null);
            }
            catch (Exception ex)
            {
                PluginManager?.OnConnected(_thisClient, null, ex);
                throw;
            }
        }
    }
}