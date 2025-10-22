using System;
using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class ClientPluginManager : IClientPluginManager
    {
        private readonly ConcurrentDictionary<Type, List<IClientPlugin>> _plugins;
        private IClient? client;

        public ClientPluginManager()
        {
            _plugins = new();
        }

        public void AddPlugin(IClientPlugin plugin)
        {
            if (plugin is IClientSendPlugin sendPlugin)
            {
                AddPlugin(sendPlugin);
            }

            if (plugin is IClientReceivePlugin receivePlugin)
            {
                AddPlugin(receivePlugin);
            }

            if (plugin is IPersistentPlugin persistentPlugin)
            {
                AddPlugin(persistentPlugin);
                if (client != null)
                {
                    persistentPlugin.Inject(client);
                }
            }
        }

        // 保留：内部通用调用逻辑
        public void InvokePlugins<T>(Action<T, LinkerClientContext> action, LinkerClientContext context) where T : IClientPlugin
        {
            ArgumentNullException.ThrowIfNull(action);
            ArgumentNullException.ThrowIfNull(context);

            var type = typeof(T);
            if (_plugins.TryGetValue(type, out var pluginList))
            {
                lock (pluginList)
                {
                    foreach (T plugin in pluginList)
                    {
                        action(plugin, context);
                    }
                }
            }
        }

        public void InvokePlugins<T>(IClient client) where T : IPersistentPlugin
        {
            ArgumentNullException.ThrowIfNull(client);

            var type = typeof(T);
            if (_plugins.TryGetValue(type, out var pluginList))
            {
                lock (pluginList)
                {
                    foreach (T plugin in pluginList)
                    {
                        plugin.Inject(client);
                    }
                }
            }
        }

        private void AddPlugin<T>(T plugin) where T : IClientPlugin
        {
            var type = typeof(T);
            var pluginList = _plugins.GetOrAdd(type, _ => new List<IClientPlugin>());
            lock (pluginList)
            {
                pluginList.Add(plugin);
            }
        }
    }
}
