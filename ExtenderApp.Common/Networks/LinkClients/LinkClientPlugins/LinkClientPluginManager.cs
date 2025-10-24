using System;
using System.Collections.Concurrent;
using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class LinkClientPluginManager : ILinkClientPluginManager
    {
        private readonly ConcurrentDictionary<Type, List<ILinkClientPlugin>> _plugins;
        private ILinkClient? client;

        public LinkClientPluginManager()
        {
            _plugins = new();
        }

        public void AddPlugin(ILinkClientPlugin plugin)
        {
            throw new NotImplementedException();
        }

        public void OnAfterReceive(ILinkClient client)
        {
            throw new NotImplementedException();
        }

        public void OnAfterSend(ILinkClient client, SocketOperationResult result)
        {
            throw new NotImplementedException();
        }

        public void OnAttach(ILinkClient client)
        {
            throw new NotImplementedException();
        }

        public void OnBeforeSend(ILinkClient client, ref LinkClientPluginSendData sendData)
        {
            throw new NotImplementedException();
        }

        public void OnConnected(ILinkClient client, EndPoint remoteEndPoint)
        {
            throw new NotImplementedException();
        }

        public void OnConnecting(ILinkClient client, EndPoint remoteEndPoint)
        {
            throw new NotImplementedException();
        }

        public void OnDetach(ILinkClient client)
        {
            throw new NotImplementedException();
        }

        public void OnDisconnected(ILinkClient client, Exception? error)
        {
            throw new NotImplementedException();
        }

        public void OnDisconnecting(ILinkClient client)
        {
            throw new NotImplementedException();
        }
    }
}
