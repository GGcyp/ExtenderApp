using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExtenderApp
{
    internal class ExtenderApplication_WPF : Application
    {
        private const string APP_FOLDER_PACK = "pack";
        private const string APP_FOLDER_LIB = "lib";
        private IServiceProvider serviceProvider;
        private AssemblyLoadContext context;
        private ILogger<ExtenderApplication_WPF> _logger;


        public ExtenderApplication_WPF()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            DebugMessage($"开始启动 : {DateTime.Now}");

            DebugMessage($"开始生成服务 : {DateTime.Now}");
            string currAppPath = ResolveAppRootPath();
            IServiceCollection services = new ServiceCollection();

            context = new("ExtenderApp_WPF", true);

            LoadAssembliesForFolder(context, currAppPath, APP_FOLDER_PACK);
            context.LoadAssemblyAndStartupFormFolderPath(Path.Combine(currAppPath, APP_FOLDER_LIB), services);
            var builder = new ContainerBuilder();
            builder.Populate(services);
            serviceProvider = new AutofacServiceProvider(builder.Build());
            DebugMessage($"生成服务成功 : {DateTime.Now}");

            sw.Stop();
            _logger = serviceProvider.GetRequiredService<ILogger<ExtenderApplication_WPF>>();
            _logger.LogInformation("{Now}启动成功，本次启动耗时{timeSpan}秒", DateTime.Now, TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds).TotalSeconds);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            serviceProvider.GetRequiredService<IMainThreadContext>().InitMainThreadContext();
            serviceProvider.GetRequiredService<IStartupExecuter>().ExecuteAsync();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

#if DEBUG

        private static void DebugMessage(string message)
        {
            Debug.Print(message);
        }

#endif

        private void LoadAssembliesForFolder(AssemblyLoadContext context, string appPath, string folderName)
        {
            if (string.IsNullOrEmpty(folderName) || string.IsNullOrEmpty(appPath))
                return;
            string fullPath = Path.Combine(appPath, folderName);

            var dllFiles = Directory.GetFiles(fullPath, "*.dll", SearchOption.AllDirectories);
            foreach (var dllFile in dllFiles)
            {
                context.LoadFromAssemblyPath(dllFile);
            }
        }

        private static string ResolveAppRootPath()
        {
            // 1) 优先使用 AppContext.BaseDirectory（大多数托管/控制台/桌面应用正确）
            var baseDir = AppContext.BaseDirectory;
            if (!string.IsNullOrEmpty(baseDir))
            {
                return baseDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            // 2) 使用 EntryAssembly 的位置（可用于 exe 主程序集）
            var entryLocation = Assembly.GetEntryAssembly()?.Location;
            if (!string.IsNullOrEmpty(entryLocation))
            {
                var dir = Path.GetDirectoryName(entryLocation);
                if (!string.IsNullOrEmpty(dir))
                    return dir;
            }

            // 3) 使用当前执行程序集的位置（当被加载为库时有帮助）
            var execLocation = Assembly.GetExecutingAssembly()?.Location;
            if (!string.IsNullOrEmpty(execLocation))
            {
                var dir = Path.GetDirectoryName(execLocation);
                if (!string.IsNullOrEmpty(dir))
                    return dir;
            }

            // 4) 最后回退到当前工作目录
            return Directory.GetCurrentDirectory();
        }

        internal void LogEorrer(Exception ex)
        {
            _logger?.LogError(ex, "应用程序发生未处理的异常");
        }
    }
}