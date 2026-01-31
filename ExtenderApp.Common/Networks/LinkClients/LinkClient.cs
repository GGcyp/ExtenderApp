using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LinkClients
{
    public abstract class LinkClient<TLinker> : DisposableObject, ILinkClient
        where TLinker : ILinker
    {
        protected TLinker Linker;

        public ILinkClientFramer? Framer { get; protected set; }
        public ILinkClientPluginManager? PluginManager { get; protected set; }

        #region ILinker 直通属性

        public virtual bool Connected => Linker.Connected;

        public virtual EndPoint? LocalEndPoint => Linker.LocalEndPoint;

        public virtual EndPoint? RemoteEndPoint => Linker.RemoteEndPoint;

        public CapacityLimiter CapacityLimiter => Linker.CapacityLimiter;

        public ValueCounter SendCounter => Linker.SendCounter;

        public ValueCounter ReceiveCounter => Linker.ReceiveCounter;

        public ProtocolType ProtocolType => Linker.ProtocolType;

        public SocketType SocketType => Linker.SocketType;

        public AddressFamily AddressFamily => Linker.AddressFamily;

        #endregion ILinker 直通属性

        public LinkClient(TLinker linker)
        {
            Linker = linker ?? throw new ArgumentNullException(nameof(linker));
        }

        public void SetClientFramer(ILinkClientFramer framer)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(framer, nameof(framer));
            Framer = framer;
        }

        public void SetClientPluginManager(ILinkClientPluginManager pluginManager)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(pluginManager, nameof(pluginManager));
            PluginManager = pluginManager;
        }

        #region ILinker 直通方法

        public Result<SocketOperationValue> Receive(Memory<byte> memory)
        {
            var result = Linker.Receive(memory);
            return ReceivePrivate(result, memory);
        }

        public async ValueTask<Result<SocketOperationValue>> ReceiveAsync(Memory<byte> memory, CancellationToken token = default)
        {
            ThrowIfDisposed();

            var result = await Linker.ReceiveAsync(memory, token);
            return ReceivePrivate(result, memory);
        }

        /// <summary>
        /// 处理接收的数据
        /// </summary>
        /// <param name="result">接收结果</param>
        /// <param name="memory">接收缓冲区</param>
        /// <returns>处理后的结果</returns>
        private Result<SocketOperationValue> ReceivePrivate(Result<SocketOperationValue> result, Memory<byte> memory)
        {
            if (!result.IsSuccess)
                return result;

            FrameContext context = new(memory);
            if (Framer != null)
            {
                Framer.Encode(ref context);
            }

            if (PluginManager != null)
            {
                PluginManager.OnReceive(result, ref context);
            }

            context.UnreadMemory.CopyTo(memory);
            context.Dispose();
            return result;
        }

        public Result<SocketOperationValue> Send(Memory<byte> memory)
        {
            ThrowIfDisposed();
            SendPrivate(memory);
            return Linker.Send(memory);
        }

        public ValueTask<Result<SocketOperationValue>> SendAsync(Memory<byte> memory, CancellationToken token = default)
        {
            ThrowIfDisposed();
            SendPrivate(memory);
            return Linker.SendAsync(memory, token);
        }

        /// <summary>
        /// 发送数据前的处理
        /// </summary>
        /// <param name="memory">发送缓冲区</param>
        private void SendPrivate(Memory<byte> memory)
        {
            FrameContext context = new(memory);
            if (PluginManager != null)
            {
                PluginManager.OnSend(ref context);
            }

            if (Framer != null)
            {
                Framer.Encode(ref context);
            }
            context.UnreadMemory.CopyTo(memory);
            context.Dispose();
        }

        public void Connect(EndPoint remoteEndPoint)
        {
            ThrowIfDisposed();
            if (PluginManager != null)
            {
                PluginManager.OnConnecting(remoteEndPoint);
            }
            Linker.Connect(remoteEndPoint);
        }

        public virtual ValueTask ConnectAsync(EndPoint remoteEndPoint, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (PluginManager != null)
            {
                PluginManager.OnConnecting(remoteEndPoint);
            }
            return Linker.ConnectAsync(remoteEndPoint, token);
        }

        public virtual void Disconnect()
        {
            ThrowIfDisposed();
            if (PluginManager != null)
            {
                try
                {
                    PluginManager.OnDisconnecting();
                    Linker.Disconnect();
                }
                catch (Exception ex)
                {
                    PluginManager.OnDisconnected(ex);
                }
            }
            else
            {
                Linker.Disconnect();
            }
        }

        public virtual async ValueTask DisconnectAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (PluginManager != null)
            {
                try
                {
                    PluginManager.OnDisconnecting();
                    await Linker.DisconnectAsync(token);
                }
                catch (Exception ex)
                {
                    PluginManager.OnDisconnected(ex);
                }
            }
            else
            {
                await Linker.DisconnectAsync(token);
            }
        }

        #endregion ILinker 直通方法

        protected override void DisposeManagedResources()
        {
            Linker.DisposeSafe();
        }

        protected override ValueTask DisposeAsyncManagedResources()
        {
            return Linker.DisposeSafeAsync();
        }

        public ILinker Clone()
        {
            return Linker.Clone();
        }

        public void SetOption(LinkOptionLevel optionLevel, LinkOptionName optionName, DataBuffer optionValue)
        {
            Linker.SetOption(optionLevel, optionName, optionValue);
        }
    }
}