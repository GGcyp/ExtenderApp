using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

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

        public int ReceiveBufferSize { get => Linker.ReceiveBufferSize; set => Linker.ReceiveBufferSize = value; }
        public int SendBufferSize { get => Linker.SendBufferSize; set => Linker.SendBufferSize = value; }
        public int ReceiveTimeout { get => Linker.ReceiveTimeout; set => Linker.ReceiveTimeout = value; }
        public int SendTimeout { get => Linker.SendTimeout; set => Linker.SendTimeout = value; }

        public ILinkClientPipeline Pipeline => throw new NotImplementedException();

        #endregion ILinker 直通属性

        public LinkClient(TLinker linker)
        {
            Linker = linker ?? throw new ArgumentNullException(nameof(linker));
        }

        public void SetOption(LinkOptionLevel optionLevel, LinkOptionName optionName, DataBuffer optionValue)
        {
            ThrowIfDisposed();
            Linker.SetOption(optionLevel, optionName, optionValue);
        }
    }

    public abstract class LinkClient<TLinker, TLinkClient> : LinkClient<TLinker>, ILinkClient<TLinkClient>
        where TLinker : ILinker
        where TLinkClient : ILinkClient
    {
        public LinkClient(TLinker linker) : base(linker)
        {
        }

        protected override ValueTask DisposeAsyncManagedResources()
        {
            return Linker.DisposeSafeAsync();
        }
    }
}