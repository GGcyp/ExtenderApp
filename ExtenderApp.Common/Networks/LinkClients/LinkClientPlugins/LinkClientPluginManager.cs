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

        private Result ForeachPlugin(Func<ILinkClientPlugin, Result> action)
        {
            lock (_sync)
            {
                foreach (var plugin in GetPlugins())
                {
                    Result result = action(plugin);
                    if (!result.IsSuccess)
                    {
                        return Result.FromException(new InvalidOperationException($"插件执行失败: {result.Message}", result.Exception));
                    }
                }
            }
            return Result.Success();
        }

        private Result ForeachPlugin<T>(Func<ILinkClientPlugin, T, Result> action, T value)
        {
            lock (_sync)
            {
                foreach (var plugin in GetPlugins())
                {
                    Result result = action(plugin, value);
                    if (!result.IsSuccess)
                    {
                        return Result.FromException(new InvalidOperationException($"插件执行失败: {result.Message}", result.Exception));
                    }
                }
            }
            return Result.Success();
        }

        public Result OnAttach(ILinkClientAwareSender client)
        {
            return ForeachPlugin(static (plugin, client) =>
            {
                return plugin.OnAttach(client);
            }, client);
        }

        public void OnDetach()
        {
            ForeachPlugin(static plugin =>
            {
                plugin.OnDetach();
                plugin.DisposeSafe();
                return Result.Success();
            });
        }

        public Result OnConnecting(EndPoint remoteEndPoint)
        {
            return ForeachPlugin(static (plugin, remoteEndPoint) =>
            {
                return plugin.OnConnecting(remoteEndPoint);
            }, remoteEndPoint);
        }

        public Result OnConnected(EndPoint remoteEndPoint, Exception? exception)
        {
            return ForeachPlugin(static (plugin, tuple) =>
            {
                return plugin.OnConnected(tuple.remoteEndPoint, tuple.exception);
            }, (remoteEndPoint, exception));
        }

        public Result OnDisconnecting()
        {
            return ForeachPlugin(static plugin =>
            {
                return plugin.OnDisconnecting();
            });
        }

        public Result OnDisconnected(Exception? error)
        {
            return ForeachPlugin(static (plugin, error) =>
            {
                return plugin.OnDisconnected(error);
            }, error);
        }

        public Result OnSend(ref FrameContext frame)
        {
            lock (_sync)
            {
                foreach (var plugin in GetPlugins())
                {
                    var result = plugin.OnSend(ref frame);
                    if (!result)
                        return result;
                }
            }
            return Result.Success();
        }

        public Result OnReceive(SocketOperationValue operationValue, ref FrameContext frame)
        {
            lock (_sync)
            {
                foreach (var plugin in GetPlugins())
                {
                    var result = plugin.OnReceive(operationValue, ref frame);
                    if (!result)
                        return result;
                }
            }
            return Result.Success();
        }
    }
}