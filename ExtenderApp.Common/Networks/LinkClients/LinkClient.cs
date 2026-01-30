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

        public LinkClient(TLinker linker)
        {
            Linker = linker ?? throw new ArgumentNullException(nameof(linker));
        }

        #region ILinker 直通方法

        public abstract Result<SocketOperationValue> Receive(Memory<byte> memory);

        public abstract ValueTask<Result<SocketOperationValue>> ReceiveAsync(Memory<byte> memory, CancellationToken token = default);

        public abstract Result<SocketOperationValue> Send(Memory<byte> memory);

        public abstract ValueTask<Result<SocketOperationValue>> SendAsync(Memory<byte> memory, CancellationToken token = default);

        public abstract void Connect(EndPoint remoteEndPoint);

        public abstract ValueTask ConnectAsync(EndPoint remoteEndPoint, CancellationToken token = default);

        public abstract void Disconnect();

        public abstract ValueTask DisconnectAsync(CancellationToken token = default);

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
    }
}