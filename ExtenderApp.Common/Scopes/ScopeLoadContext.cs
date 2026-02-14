using System.Reflection;
using System.Runtime.Loader;

namespace ExtenderApp.Common.Scopes
{
    /// <summary>
    /// 插件加载上下文：使用 AssemblyDependencyResolver 定位依赖， 并优先返回 AppDomain 已加载的程序集（保证共享契约类型唯一）。
    /// </summary>
    public sealed class ScopeLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        /// <summary>
        /// </summary>
        /// <param name="pluginPath">插件主 DLL 的完整路径（用于定位依赖）。</param>
        public ScopeLoadContext(string pluginPath) : base($"PluginLoadContext:{pluginPath}", isCollectible: true)
        {
            _resolver = new(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // 1) 优先返回已经由宿主加载的同名程序集（保证类型一致性）
            var already = AppDomain.CurrentDomain
                                   .GetAssemblies()
                                   .FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase));
            if (already != null)
                return already;

            // 2) 使用 AssemblyDependencyResolver 定位并在当前 ALC 中加载
            var path = _resolver.ResolveAssemblyToPath(assemblyName);
            if (path != null)
                return LoadFromAssemblyPath(path);

            // 3) 无法解析则返回 null（允许默认/上层继续尝试）
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
                return LoadUnmanagedDllFromPath(libraryPath);

            return IntPtr.Zero;
        }
    }
}