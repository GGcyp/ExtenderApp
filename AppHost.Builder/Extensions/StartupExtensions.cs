using System.Reflection;
using AppHost.Common;


namespace AppHost.Builder
{
    /// <summary>
    /// startup扩展程序
    /// </summary>
    public static class StartupExtensions
    {
        /// <summary>
        /// 为指定的文件夹加载程序集
        /// </summary>
        /// <param name="builder">主机应用程序构建器</param>
        /// <param name="folderName">文件夹名称</param>
        /// <returns>返回主机应用程序构建器</returns>
        /// <exception cref="ArgumentException">如果路径没有根目录，则抛出异常</exception>
        public static IHostApplicationBuilder LoadAssembliesForFolder(this IHostApplicationBuilder builder, string folderName)
        {
            string folderPath = Path.Combine(builder.HostEnvironment.ContentRootPath, folderName);
            if (!Path.IsPathRooted(folderPath)) throw new ArgumentException("路径没有根目录");

            var assmblies = AppHostAssemblyHandle.LoadAssemblyForFolder(folderPath);
            return builder;
        }

        /// <summary>
        /// 加载指定文件夹下所有程序集，并启动其中的starup
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        /// <remarks>注意不要重复加载</remarks>
        /// <returns></returns>
        public static IHostApplicationBuilder FindStarupForFolder(this IHostApplicationBuilder builder, string folderName)
        {
            string folderPath = Path.Combine(builder.HostEnvironment.ContentRootPath, folderName);
            if (!Path.IsPathRooted(folderPath)) throw new ArgumentException("路径没有根目录");

            var assmblies = AppHostAssemblyHandle.LoadAssemblyForFolder(folderPath);

            foreach (var assmbly in assmblies)
            {
                builder.FindStarupForAssembly(assmbly);
            }

            return builder;
        }

        /// <summary>
        /// 加载指定文件夹下所有程序集，并启动其中的starup
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        /// <remarks>注意不要重复加载</remarks>
        /// <returns></returns>
        public static IHostApplicationBuilder FindStarupForFolder<TStartup>(this IHostApplicationBuilder builder, string folderName, out List<TStartup> startups) where TStartup : Startup
        {
            string folderPath = Path.Combine(builder.HostEnvironment.ContentRootPath, folderName);
            if (!Path.IsPathRooted(folderPath)) throw new ArgumentException("path is not rooted");

            startups = new List<TStartup>();

            var assmblies = AppHostAssemblyHandle.LoadAssemblyForFolder(folderPath);

            foreach (var assmbly in assmblies)
            {
                var startup = builder.FindStarupForAssembly<TStartup>(assmbly);
                if (startup is null) continue;
                startups.Add(startup);
            }

            return builder;
        }

        /// <summary>
        /// 加载程序集，并启动其中的starup
        /// </summary>
        /// <param name="builder">主机程序</param>
        /// <param name="path">程序集地址</param>
        /// <param name="startup">启动类</param>
        /// <remarks>注意不要重复加载</remarks>
        /// <returns>主机程序</returns>
        public static IHostApplicationBuilder FindStarupForLoadAssemblyFile<TStartup>(this IHostApplicationBuilder builder, string path, out TStartup startup) where TStartup : Startup
        {
            if (!Path.IsPathRooted(path)) throw new ArgumentException("path is not rooted");
            startup = builder.FindStarupForAssembly<TStartup>(Assembly.LoadFrom(path));
            return builder;
        }

        /// <summary>
        /// 加载程序集，并启动其中的starup
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        /// <remarks>注意不要重复加载</remarks>
        /// <returns></returns>
        public static IHostApplicationBuilder FindStarupForLoadAssemblyFile(this IHostApplicationBuilder builder, string path)
        {
            if (!Path.IsPathRooted(path)) throw new ArgumentException("path is not rooted");
            return builder.FindStarupForAssembly(Assembly.LoadFrom(path));
        }

        /// <summary>
        /// 启动程序集中的starup
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IHostApplicationBuilder FindStarupForAssembly(this IHostApplicationBuilder builder, Assembly assembly)
        {
            FindStarupForAssembly<Startup>(builder, assembly);
            return builder;
        }

        /// <summary>
        /// 启动程序集中的starup
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static TStartup? FindStarupForAssembly<TStartup>(this IHostApplicationBuilder builder, Assembly assembly) where TStartup : Startup
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            var startType = assembly.GetTypes().FirstOrDefault(t => !t.IsAbstract && Startup.Type.IsAssignableFrom(t));
            if (startType is null) return null;

            TStartup startup = (Activator.CreateInstance(startType) as TStartup)!;
            startup.Start(builder);
            startup.AddService(builder.Services);

            return startup;
        }
    }
}
