using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    public abstract class LinkClient<TLinker> : DisposableObject, ILinkClient
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

        #endregion ILinker 直通属性

        public LinkClient(TLinker linker)
        {
            Linker = linker ?? throw new ArgumentNullException(nameof(linker));
        }

        protected override void Dispose(bool disposing)
        {
            Linker.Dispose();
        }
    }
}