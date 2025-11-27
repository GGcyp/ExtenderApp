using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LinkClients
{
    public abstract class LinkClient<TLinker> : DisposableObject, ILinkClient, ILinker
        where TLinker : ILinker
    {
        protected TLinker Linker;

        #region ILinker 直通属性

        public virtual bool Connected => Linker.Connected;

        public virtual EndPoint? LocalEndPoint => Linker.LocalEndPoint;

        public virtual EndPoint? RemoteEndPoint => Linker.RemoteEndPoint;

        public virtual CapacityLimiter CapacityLimiter => Linker.CapacityLimiter;

        public virtual ValueCounter SendCounter => Linker.SendCounter;

        public virtual ValueCounter ReceiveCounter => Linker.ReceiveCounter;

        public ProtocolType ProtocolType => Linker.ProtocolType;

        public SocketType SocketType => Linker.SocketType;

        public AddressFamily AddressFamily => Linker.AddressFamily;

        #endregion ILinker 直通属性

        public LinkClient(TLinker linker)
        {
            Linker = linker ?? throw new ArgumentNullException(nameof(linker));
        }

        public Result<SocketOperationValue> Receive(Memory<byte> memory)
        {
            return Linker.Receive(memory);
        }

        public ValueTask<Result<SocketOperationValue>> ReceiveAsync(Memory<byte> memory, CancellationToken token = default)
        {
            return Linker.ReceiveAsync(memory, token);
        }

        public Result<SocketOperationValue> Send(Memory<byte> memory)
        {
            return Linker.Send(memory);
        }

        public ValueTask<Result<SocketOperationValue>> SendAsync(Memory<byte> memory, CancellationToken token = default)
        {
            return Linker.SendAsync(memory, token);
        }

        public void Connect(EndPoint remoteEndPoint)
        {
            Linker.Connect(remoteEndPoint);
        }

        public ValueTask ConnectAsync(EndPoint remoteEndPoint, CancellationToken token = default)
        {
            return Linker.ConnectAsync(remoteEndPoint, token);
        }

        public void Disconnect()
        {
            Linker.Disconnect();
        }

        public ValueTask DisconnectAsync(CancellationToken token = default)
        {
            return Linker.DisconnectAsync(token);
        }

        protected override ValueTask DisposeAsyncManagedResources()
        {
            if (!Linker.Connected)
                return ValueTask.CompletedTask;

            return Linker.DisconnectAsync();
        }

        public ILinker Clone()
        {
            return Linker.Clone();
        }
    }
}