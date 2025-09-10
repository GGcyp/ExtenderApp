using System.Reflection;
using System.Runtime.InteropServices;

namespace Communal.Handles
{
    /// <summary>
    /// 通用处理类，处理程序集
    /// </summary>
    public static class AssemblyHandle
    {
        /// <summary>
        /// 加载指定的动态链接库
        /// </summary>
        /// <param name="filePath">动态链接库的地址</param>
        /// <returns>加载地址</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string filePath);

        /// <summary>
        /// 加载指定目录下的所有程序集
        /// </summary>
        /// <param name="directoryPath">加载文件夹路径</param>
        /// <param name="predicate">指定程序集名字加载</param>
        /// <returns>加载的程序集</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static Assembly[] LoadAssembliesForDirectory(string directoryPath, Func<string, bool> predicate = null)
        {
            // 检查目录是否存在
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
            }

            // 加载目录下的程序集
            var files = Directory.GetFiles(directoryPath, "*.dll");
            if (predicate != null) files = files.Where(predicate).ToArray();

            return LoadAssembliesForFilePathArray(files);
        }

        /// <summary>
        /// 加载指定目录下的所有程序集
        /// </summary>
        /// <param name="files">加载文件夹路径</param>
        /// <returns>加载的程序集</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static Assembly[] LoadAssembliesForFilePathArray(string[] files)
        {
            // 检查目录是否存在
            if (files == null)
            {
                throw new DirectoryNotFoundException($"Load assembly path cannt be null");
            }

            // 加载目录下的程序集
            Assembly[] assemblies = new Assembly[files.Length];
            //var loadContext = new CustomAssemblyLoadContext();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var file = files[i];
                var assembly = Assembly.LoadFrom(file);
                //var assembly = loadContext.LoadFromAssemblyPath(file);
                assemblies[i] = assembly;
            }
            //loadContext.Unload();
            return assemblies;
        }

        /// <summary>
        /// 从一个程序集内查找所有继承自指定类型的类
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="type">指定类型</param>
        /// <returns></returns>
        public static Type[] FindAllInheritedTheTypeForAssembly(Assembly assembly, Type type)
        {
            if (assembly == null || type == null) throw new ArgumentNullException($"The Assembly and type cannt be null");

            Type[] entityTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && type.IsAssignableFrom(t))
                .ToArray();
            return entityTypes;
        }

        /// <summary>
        /// 从一个程序集内查找第一个继承自指定类型的类型
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="type">指定类型</param>
        /// <returns></returns>
        public static Type? FindFirstInheritedTheTypeForAssembly(Assembly assembly, Type type)
        {
            if (assembly == null || type == null) throw new ArgumentNullException($"The Assembly and type cannt be null");

            var entityType = assembly.GetTypes()
                .FirstOrDefault(t => !t.IsAbstract && type.IsAssignableFrom(t));

            return entityType;
        }
    }
}
