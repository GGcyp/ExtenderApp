using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// LinkClientFactory 类用于创建 LinkClient 实例。
    /// </summary>
    public abstract class LinkClientFactory<TLinkClient, TLinker> : ILinkClientFactory<TLinkClient>
        where TLinkClient : ILinkClient
        where TLinker : ILinker
    {
        private readonly ILinkerFactory<TLinker> _linkerFactory;

        public ProtocolType ProtocolType => _linkerFactory.ProtocolType;

        public SocketType SocketType => _linkerFactory.SocketType;

        protected LinkClientFactory(ILinkerFactory<TLinker> linkerFactory)
        {
            _linkerFactory = linkerFactory;
        }

        public TLinkClient CreateLinkClient()
        {
            return CreateLinkClient(AddressFamily.InterNetwork);
        }

        public TLinkClient CreateLinkClient(AddressFamily addressFamily)
        {
            return CreateLinkClient(new Socket(addressFamily, SocketType, ProtocolType));
        }

        public TLinkClient CreateLinkClient(Socket socket)
        {
            ArgumentNullException.ThrowIfNull(socket, nameof(socket));
            var linker = _linkerFactory.CreateLinker(socket);
            return CreateLinkClient(linker);
        }

        public TLinkClient CreateLinkClient(ILinker linker)
        {
            ArgumentNullException.ThrowIfNull(linker, nameof(linker));
            if (linker.SocketType != SocketType || linker.ProtocolType != ProtocolType)
                throw new ArgumentException(ProtocolType + "/" + SocketType + " 类型的链接器是必须的。", nameof(linker));

            return CreateLinkClient((TLinker)linker);
        }

        protected abstract TLinkClient CreateLinkClient(TLinker linker);
    }
}