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

        public ILinkClientFramer? Framer { get; protected set; }

        public ReadOnlyMemory<byte> Magic => Framer?.Magic ?? ReadOnlyMemory<byte>.Empty;

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

        public void SetOption(LinkOptionLevel optionLevel, LinkOptionName optionName, DataBuffer optionValue)
        {
            ThrowIfDisposed();
            Linker.SetOption(optionLevel, optionName, optionValue);
        }

        public void SetMagic(ReadOnlySpan<byte> magic)
        {
            if (Framer == null)
                throw new InvalidOperationException("未设置帧器，无法配置 Magic");
            Framer.SetMagic(magic);
        }
    }

    public abstract class LinkClient<TLinker, TLinkClient> : LinkClient<TLinker>, ILinkClient<TLinkClient>
        where TLinker : ILinker
        where TLinkClient : ILinkClient
    {
        public ILinkClientPluginManager<TLinkClient>? PluginManager { get; protected set; }
        
        public LinkClient(TLinker linker) : base(linker)
        {
        }

        public void SetClientPluginManager(ILinkClientPluginManager<TLinkClient> pluginManager)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(pluginManager, nameof(pluginManager));
            PluginManager = pluginManager;
        }

        protected override void DisposeManagedResources()
        {
            PluginManager.DisposeSafe();
            Framer.DisposeSafe();
            Linker.DisposeSafe();
        }

        protected override ValueTask DisposeAsyncManagedResources()
        {
            return Linker.DisposeSafeAsync();
        }
    }
}