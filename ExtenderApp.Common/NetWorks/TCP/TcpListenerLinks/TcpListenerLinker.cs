﻿using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    public class TcpListenerLinker : ListenerLinker
    {
        private readonly ILinkerFactory _linkerFactory;

        public TcpListenerLinker(ILinkerFactory factory) : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            _linkerFactory = factory;
        }

        protected override ILinker CreateLinker(Socket clientSocket)
        {
            var result = _linkerFactory.CreateLinker<ITcpLinker>(clientSocket);
            return result;
        }
    }
}
