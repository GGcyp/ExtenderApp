using AppHost.Extensions.DependencyInjection;

namespace MainApp.Common.File
{
    /// <summary>
    /// 提供对<see cref="IFileAccessProvider"/>接口的扩展方法。
    /// </summary>
    public static class FileAccessProviderExtensions
    {
        /// <summary>
        /// 将文件访问提供程序添加到主机应用程序构建器中。
        /// </summary>
        /// <param name="services">主机应用程序构建器实例。</param>
        /// <returns>更新后的主机应用程序构建器实例。</returns>
        public static IServiceCollection AddAccessProvider(this IServiceCollection services)
        {
            services.AddSingleton<IFileAccessProvider, FileAccessProvider>();
            return services;
        }

        /// <summary>
        /// 获取必需的文件解析器。
        /// </summary>
        /// <param name="fileAccessProvider">文件访问提供程序。</param>
        /// <param name="extensionType">文件扩展名类型。</param>
        /// <returns>返回与指定文件扩展名类型匹配的文件解析器。</returns>
        /// <exception cref="ArgumentNullException">如果 <paramref name="fileAccessProvider"/> 为 null。</exception>
        /// <exception cref="ArgumentException">如果找不到与指定 <paramref name="extensionType"/> 匹配的文件解析器。</exception>
        public static IFileParser GetRequiredFileParser(this IFileAccessProvider fileAccessProvider, FileExtensionType extensionType)
        {
            IFileParser fileParser = fileAccessProvider?.GetParser(extensionType);

            if (fileParser == null) throw new ArgumentNullException(nameof(fileParser));

            return fileParser;
        }

        /// <summary>
        /// 对文件进行操作的扩展方法
        /// </summary>
        /// <param name="fileAccessProvider">文件访问提供者</param>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="processAction">处理文件的委托，参数为对象数组</param>
        public static void FileOperation(this IFileAccessProvider fileAccessProvider, FileInfoData infoData, Action<object> processAction)
        {
            fileAccessProvider.GetRequiredFileParser(infoData.Extension).ProcessFile(infoData, processAction);
        }
    }
}
