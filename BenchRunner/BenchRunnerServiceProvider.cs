using System.Runtime.Loader;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace BenchRunner
{
    internal class BenchRunnerServiceProvider : IDisposable
    {
        // keep context for future plugin loading but do not manually load core assemblies to avoid ALC conflicts
        public AssemblyLoadContext? context;

        public IServiceProvider ServiceProvider { get; }

        public BenchRunnerServiceProvider()
        {
            // 使用 Microsoft DI 构建 IServiceCollection，随后由 Autofac 填充
            IServiceCollection services = new ServiceCollection();

            // 注册序列化等核心服务来自 ExtenderApp.Common
            services.AddSerializations();

            // 为插件加载创建独立的 AssemblyLoadContext（保留但不加载核心程序集）
            try
            {
                context = new("BenchRunner", true);

                // Load plugin/lib folders if present
                LoadAssembliesForFolder(context, ProgramDirectory.PackPath);
                // Attempt to call extension loader if exists in external libs
                try { context.LoadAssemblyAndStartupFormFolderPath(ProgramDirectory.LibPath, services); } catch { }
            }
            catch
            {
                context = null;
            }

            // 使用 Autofac 构建容器并将 IServiceCollection 中的服务填充进去
            var builder = new ContainerBuilder();
            builder.Populate(services);
            ServiceProvider = new AutofacServiceProvider(builder.Build());

            // 尝试解析关键服务并在失败时打印完整异常堆栈，便于定位在构造/初始化阶段发生的问题
            try
            {
                // 不使用 GetRequiredService 以避免抛出简单的 InvalidOperationException
                var svc = ServiceProvider.GetService<IBinarySerialization>();
                if (svc == null)
                {
                    Console.WriteLine("Warning: IBinarySerialization not registered (null) in ServiceProvider.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while resolving services:");
                Console.WriteLine(FormatException(ex));
            }
        }

        private static string FormatException(Exception ex)
        {
            var sb = new StringBuilder();
            var current = ex;
            int level = 0;
            while (current != null)
            {
                sb.AppendLine($"[{level}] {current.GetType().FullName}: {current.Message}");
                sb.AppendLine(current.StackTrace ?? string.Empty);
                current = current.InnerException;
                level++;
            }
            return sb.ToString();
        }

        private static void LoadAssembliesForFolder(AssemblyLoadContext context, string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath) || !Directory.Exists(fullPath))
                return;

            var dllFiles = Directory.GetFiles(fullPath, "*.dll", SearchOption.AllDirectories);
            foreach (var dllFile in dllFiles)
            {
                try { context.LoadFromAssemblyPath(dllFile); } catch { }
            }
        }

        public void Dispose()
        {
            try { context?.Unload(); } catch { }
        }
    }
}