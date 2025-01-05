using System.Reflection;

namespace AppHost.Extensions.Hosting
{
    /// <summary>
    /// 提供用于创建主机环境的静态方法。
    /// </summary>
    public static class HostEnvironmentBuilder
    {
        /// <summary>
        /// 创建一个新的主机环境实例。
        /// </summary>
        /// <returns>返回一个新的 <see cref="IHostEnvironment"/> 实例。</returns>
        public static IHostEnvironment CreateEnvironment()
        {
            return new HostEnvironment()
            {
                /// <summary>
                /// 获取或设置应用程序的名称。
                /// </summary>
                ApplicationName = Assembly.GetEntryAssembly()!.FullName!,
                /// <summary>
                /// 获取或设置内容根目录的路径。
                /// </summary>
                ContentRootPath = Directory.GetCurrentDirectory(),
            };
        }
    }
}
