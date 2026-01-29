using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks
{
    internal class LinkClientPluginManager : DisposableObject, ILinkClientPluginManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SortedList<int, ILinkClientPlugin> _plugins;

        // 写时更新的快照，读时无锁遍历
        private readonly object _sync = new();

        public ILinkClientPlugin? this[int index] => _plugins.TryGetValue(index, out var plugin) ? plugin : null;

        public LinkClientPluginManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _plugins = new();
        }

        public void AddPlugin<T>(int priority) where T : class, ILinkClientPlugin
        {
            T plugin = _serviceProvider.GetService<T>() ??
                ActivatorUtilities.CreateInstance<T>(_serviceProvider);
            AddPlugin(priority, plugin);
        }

        public void AddPlugin(int priority, ILinkClientPlugin plugin)
        {
            ArgumentNullException.ThrowIfNull(plugin);

            if (!_plugins.TryGetValue(priority, out var existingPlugin) ||
                existingPlugin == null)
            {
                lock (_sync)
                {
                    _plugins.Add(priority, plugin);
                }
                return;
            }

            if (existingPlugin.GetType() == plugin.GetType())
            {
                throw new InvalidCastException($"不可以重复添加相同的插件");
            }

            // 如果优先级冲突，则寻找下一个可用的优先级
            lock (_sync)
            {
                priority++;
                while (_plugins.ContainsKey(priority))
                {
                    priority++;
                }
                _plugins.Add(priority, plugin);
            }
        }

        public bool RemovePlugin(int priority)
        {
            lock (_sync)
            {
                if (!_plugins.TryGetValue(priority, out var plugin) ||
                    plugin == null)
                {
                    return false;
                }
                _plugins.Remove(priority);

                // 在移除时调用插件的 OnDetach 并释放资源
                try
                {
                    plugin.OnDetach();
                }
                catch
                {
                    // 忽略单个插件的 OnDetach 异常，防止影响其它清理
                }
                plugin.DisposeSafe();
                return true;
            }
        }

        public bool RemovePlugin<T>() where T : class, ILinkClientPlugin
        {
            for (int i = 0; i < _plugins.Count; i++)
            {
                if (_plugins.Values[i] is T)
                {
                    return RemovePlugin(_plugins.Keys[i]);
                }
            }
            return true;
        }

        public bool TryGetPlugin<T>(out T plugin) where T : class, ILinkClientPlugin
        {
            // 使用无锁快照以保持读性能与线程安全
            foreach (var p in _plugins)
            {
                if (p is T cast)
                {
                    plugin = cast;
                    return true;
                }
            }
            plugin = null!;
            return false;
        }

        public IEnumerable<ILinkClientPlugin> GetPlugins()
        {
            // 返回当前快照（只读枚举）
            return _plugins.Values;
        }

        private void ForeachPlugin(Action<ILinkClientPlugin> action)
        {
            lock (_sync)
            {
                foreach (var plugin in GetPlugins())
                {
                    try
                    {
                        action(plugin);
                    }
                    catch
                    {
                        // 忽略单个插件异常，保证其它插件仍能被调用
                    }
                }
            }
        }

        private void ForeachPlugin<T>(Action<ILinkClientPlugin, T> action, T value)
        {
            lock (_sync)
            {
                foreach (var plugin in GetPlugins())
                {
                    try
                    {
                        action(plugin, value);
                    }
                    catch
                    {
                        // 忽略单个插件异常，保证其它插件仍能被调用
                    }
                }
            }
        }

        public void OnAttach(ILinkClientAwareSender client)
        {
            ForeachPlugin(static (plugin, client) =>
            {
                plugin.OnAttach(client);
            }, client);
        }

        public void OnDetach()
        {
            ForeachPlugin(static plugin =>
            {
                plugin.OnDetach();
                plugin.DisposeSafe();
            });
        }

        public void OnConnecting(EndPoint remoteEndPoint)
        {
            ForeachPlugin(static (plugin, remoteEndPoint) =>
            {
                plugin.OnConnecting(remoteEndPoint);
            }, remoteEndPoint);
        }

        public void OnConnected(EndPoint remoteEndPoint, Exception? exception)
        {
            ForeachPlugin(static (plugin, tuple) =>
            {
                plugin.OnConnected(tuple.remoteEndPoint, tuple.exception);
            }, (remoteEndPoint, exception));
        }

        public void OnDisconnecting()
        {
            ForeachPlugin(static plugin =>
            {
                plugin.OnDisconnecting();
            });
        }

        public void OnDisconnected(Exception? error)
        {
            ForeachPlugin(static (plugin, error) =>
            {
                plugin.OnDisconnected(error);
            }, error);
        }

        public void OnSend(ref FrameContext frame)
        {
            ForeachPlugin(static (plugin, frame) =>
            {
                plugin.OnSend(ref frame);
            }, frame);
        }

        public void OnReceive(SocketOperationValue operationValue, ref FrameContext frame)
        {
            lock (_sync)
            {
                foreach (var plugin in GetPlugins())
                {
                    plugin.OnReceive(operationValue, ref frame);
                    if (frame.HasException)
                        return;
                }
            }
        }
    }
}