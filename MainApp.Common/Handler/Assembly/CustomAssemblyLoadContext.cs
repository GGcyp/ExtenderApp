using System.Reflection;
using System.Runtime.Loader;

namespace Communal.Handles
{
    /// <summary>
    /// 通用层，加载一个程序集和他所依赖的动态库
    /// </summary>
    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        protected override Assembly Load(AssemblyName assemblyPath)
        {
            // 检查已加载的程序集
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyPath);
            if (assembly != null)
            {
                return assembly;
            }

            // 加载指定名称的程序集
            return LoadFromAssemblyPath($"{assemblyPath.Name}.dll");
        }
    }
}
