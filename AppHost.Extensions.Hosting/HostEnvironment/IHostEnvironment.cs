namespace AppHost.Extensions.Hosting
{
    /// <summary>
    /// 主机环境接口
    /// </summary>
    public interface IHostEnvironment
    {
        /// <summary>
        /// 获取或设置应用程序的名称。
        /// </summary>
        string ApplicationName { get; }
        /// <summary>
        /// 获取或设置应用程序的内容根路径。
        /// </summary>
        string ContentRootPath { get; }
        /// <summary>
        /// 获取或设置应用程序的环境名称。
        /// </summary>
        string EnvironmentName { get; }
    }
}
