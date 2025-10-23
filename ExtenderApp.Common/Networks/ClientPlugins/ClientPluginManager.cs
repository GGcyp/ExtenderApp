using System;
using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class ClientPluginManager
    {
        private readonly ConcurrentDictionary<Type, List<IClientPlugin>> _plugins;
        private IClient? client;

        public ClientPluginManager()
        {
            _plugins = new();
        }
    }
}
