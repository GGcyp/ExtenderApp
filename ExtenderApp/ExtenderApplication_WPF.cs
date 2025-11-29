using System.Diagnostics;
using System.IO;
using System.Runtime.Loader;
using System.Windows;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExtenderApp
{
    /// <summary>
    /// WPF 应用程序入口类（使用
    /// Microsoft.Extensions.DependencyInjection +
    /// Autofac）。 负责：
    /// - 解析应用根路径并构建 IConfiguration；
    /// - 动态加载插件/库到独立的 AssemblyLoadContext；
    /// - 构建并填充 DI 容器（Autofac）；
    /// - 获取 ILogger 并记录启动信息；
    /// - 在 OnStartup 时初始化主线程上下文并触发启动执行器；
    /// - 在 OnExit 时释放 DI 容器资源。
    /// </summary>
    internal class ExtenderApplication_WPF : Application
    {
        /// <summary>
        /// 根服务提供者（由 Autofac 封装的
        /// IServiceProvider）。 在应用退出时如果实现了
        /// IDisposable 将被释放。
        /// </summary>
        private IServiceProvider serviceProvider;

        /// <summary>
        /// 自定义的 AssemblyLoadContext，用于在运行时加载主程序集。
        /// </summary>
        private AssemblyLoadContext context;

        /// <summary>
        /// 当前类的日志记录器，通过 DI 获取。
        /// </summary>
        private ILogger<ExtenderApplication_WPF> _logger;

        /// <summary>
        /// 构造函数：执行启动前的初始化、配置构建、程序集加载及 DI 容器构建， 并记录启动耗时信息。
        /// </summary>
        public ExtenderApplication_WPF()
        {
            // 记录启动耗时
            Stopwatch sw = new Stopwatch();
            sw.Start();
            DebugMessage($"开始启动 : {DateTime.Now}");

            DebugMessage($"开始生成服务 : {DateTime.Now}");

            // 使用 Microsoft DI 构建
            // IServiceCollection，随后由 Autofac 填充
            IServiceCollection services = new ServiceCollection();

            // 为插件加载创建独立的 AssemblyLoadContext，设置isCollectible为true以便卸载
            context = new("ExtenderApp_WPF", true);

            // 在 DI 中注册 IConfiguration（基于
            // appsettings.json + 环境变量）
            services.AddSingleton<IConfiguration>(provider =>
            {
                var configBuilder = new ConfigurationBuilder()
                    .SetBasePath(ProgramDirectory.AppRootPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
                return configBuilder.Build();
            });

            // 加载插件目录中的程序集（Pack 文件夹）
            LoadAssembliesForFolder(context, ProgramDirectory.PackPath);

            // 从指定的库目录加载程序集并调用扩展的加载逻辑（扩展方法在其他位置实现）
            context.LoadAssemblyAndStartupFormFolderPath(ProgramDirectory.LibPath, services);

            // 使用 Autofac 构建容器并将
            // IServiceCollection 中的服务填充进去
            var builder = new ContainerBuilder();
            builder.Populate(services);
            serviceProvider = new AutofacServiceProvider(builder.Build());
            DebugMessage($"生成服务成功 : {DateTime.Now}");

            sw.Stop();

            // 从容器中获取 logger 并记录启动完成及耗时
            _logger = serviceProvider.GetRequiredService<ILogger<ExtenderApplication_WPF>>();
            _logger.LogInformation("{Now}启动成功，本次启动耗时{timeSpan}秒", DateTime.Now, TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds).TotalSeconds);
        }

        /// <summary>
        /// WPF 启动时调用。初始化主线程上下文并触发启动执行器。
        /// 注意：IStartupExecuter.ExecuteAsync()
        /// 未在此处等待（fire-and-forget / 自行处理异步行为）。
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 初始化主线程上下文（例如同步上下文或消息循环相关的初始化）
            serviceProvider.GetRequiredService<IMainThreadContext>().InitMainThreadContext();

            // 执行应用启动逻辑（异步）
            serviceProvider.GetRequiredService<IStartupExecuter>().ExecuteAsync();
        }

        /// <summary>
        /// 应用退出时调用。释放 DI 容器资源（如果实现了 IDisposable）。
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// 将指定文件夹（及其子目录）下的所有 .dll 加载到给定的
        /// AssemblyLoadContext。 跳过空或无效的路径参数。
        /// </summary>
        /// <param name="context">目标 AssemblyLoadContext。</param>
        /// <param name="fullPath">目标文件全称</param>
        private void LoadAssembliesForFolder(AssemblyLoadContext context, string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return;

            // 查找所有 DLL 并加载到指定的
            // AssemblyLoadContext 中
            var dllFiles = Directory.GetFiles(fullPath, "*.dll", SearchOption.AllDirectories);
            foreach (var dllFile in dllFiles)
            {
                context.LoadFromAssemblyPath(dllFile);
            }
        }

        /// <summary>
        /// 对外的错误记录入口（方法名保留原拼写 LogEorrer）。 使用注入的
        /// ILogger 记录未处理异常信息（若 _logger 为 null 则静默忽略）。
        /// </summary>
        /// <param name="ex">需记录的异常。</param>
        internal void LogEorrer(Exception ex)
        {
            _logger?.LogError(ex, "应用程序发生未处理的异常");
        }

#if DEBUG

        /// <summary>
        /// 调试用信息输出，使用 System.Diagnostics.Debug
        /// 打印（仅在 DEBUG 编译符下有效）。
        /// </summary>
        private static void DebugMessage(string message)
        {
            Debug.Print(message);
        }

#endif
    }
}