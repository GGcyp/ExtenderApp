using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class LinkClientPluginManager<TLinker> : ILinkClientPluginManager<TLinker> where TLinker : ILinker
    {
        private readonly ConcurrentDictionary<Type, ILinkClientPlugin<TLinker>> _plugins;
        // 写时更新的快照，读时无锁遍历
        private volatile ILinkClientPlugin<TLinker>[] _snapshot;

        public LinkClientPluginManager()
        {
            _plugins = new();
            _snapshot = Array.Empty<ILinkClientPlugin<TLinker>>();
        }

        public void AddPlugin<T>(T plugin) where T : ILinkClientPlugin<TLinker>
        {
            ArgumentNullException.ThrowIfNull(plugin, nameof(plugin));

            Type type = typeof(T);
            if (_plugins.ContainsKey(type))
            {
                throw new InvalidOperationException($"插件管理器中已存在类型为 {type.FullName} 的插件，不能重复添加");
            }
            if (!_plugins.TryAdd(type, plugin))
            {
                throw new InvalidOperationException($"向插件管理器添加类型为 {type.FullName} 的插件失败");
            }

            // 写时重建快照
            _snapshot = _plugins.Values.ToArray();
        }

        public bool RemovePlugin<T>() where T : ILinkClientPlugin<TLinker>
        {
            Type type = typeof(T);
            if (_plugins.TryRemove(type, out _))
            {
                _snapshot = _plugins.Values.ToArray();
                return true;
            }
            return false;
        }

        public bool TryGetPlugin<T>(out T? plugin) where T : class, ILinkClientPlugin<TLinker>
        {
            Type type = typeof(T);
            if (_plugins.TryGetValue(type, out var found))
            {
                plugin = found as T;
                return plugin is not null;
            }
            plugin = null;
            return false;
        }

        public void ReplacePlugin<T>(T plugin) where T : ILinkClientPlugin<TLinker>
        {
            ArgumentNullException.ThrowIfNull(plugin, nameof(plugin));
            Type type = typeof(T);

            while (true)
            {
                if (!_plugins.TryGetValue(type, out var existing))
                {
                    throw new InvalidOperationException($"要替换的插件类型 {type.FullName} 不存在");
                }

                if (_plugins.TryUpdate(type, plugin, existing))
                {
                    _snapshot = _plugins.Values.ToArray();
                    return;
                }

                // 竞争重试
            }
        }

        public IReadOnlyList<ILinkClientPlugin<TLinker>> GetPlugins()
        {
            // 返回当前快照的只读包装（避免外部修改）
            return Array.AsReadOnly(_snapshot.ToArray());
        }

        public void OnAttach(ILinkClient<TLinker> client)
        {
            foreach (var plugin in _snapshot)
            {
                plugin.OnAttach(client);
            }
        }

        public void OnSend(ILinkClient<TLinker> client, ref LinkClientPluginSendMessage sendData)
        {
            foreach (var plugin in _snapshot)
            {
                plugin.OnSend(client, ref sendData);
            }
        }

        public void OnConnecting(ILinkClient<TLinker> client, EndPoint remoteEndPoint)
        {
            foreach (var plugin in _snapshot)
            {
                plugin.OnConnecting(client, remoteEndPoint);
            }
        }

        public void OnDetach(ILinkClient<TLinker> client)
        {
            foreach (var plugin in _snapshot)
            {
                plugin.OnDetach(client);
            }
        }

        public void OnDisconnected(ILinkClient<TLinker> client, Exception? error)
        {
            foreach (var plugin in _snapshot)
            {
                plugin.OnDisconnected(client, error);
            }
        }

        public void OnDisconnecting(ILinkClient<TLinker> client)
        {
            foreach (var plugin in _snapshot)
            {
                plugin.OnDisconnecting(client);
            }
        }

        public void OnConnected(ILinkClient<TLinker> client, EndPoint remoteEndPoint, Exception exception)
        {
            foreach (var plugin in _snapshot)
            {
                plugin.OnConnected(client, remoteEndPoint, exception);
            }
        }

        public void OnReceive(ILinkClient<TLinker> client, ref LinkClientPluginReceiveMessage message)
        {
            foreach (var plugin in _snapshot)
            {
                plugin.OnReceive(client, ref message);
            }
        }
    }
}
