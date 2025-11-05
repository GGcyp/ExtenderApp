using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供针对 IStartup 类型的查找与程序集加载扩展方法。
    /// - 支持从路径加载托管程序集并调用其 IStartup.AddService 方法。
    /// - 对于非托管（native）库使用 <see cref="NativeLibrary.Load(string)"/> 加载。
    /// </summary>
    public static class IStartupExtensions
    {
        /// <summary>
        /// 标识启动接口类型，用于在程序集内查找实现。
        /// </summary>
        public static Type StartupType = typeof(IStartup);

        /// <summary>
        /// 尝试从给定的 <see cref="Assembly"/> 中找到并实例化符合条件的启动类（泛型）。
        /// 实例化失败或类型不匹配将返回 false。
        /// </summary>
        /// <typeparam name="TStartup">期望的启动类型，需实现 <see cref="IStartup"/>。</typeparam>
        /// <param name="assembly">要搜索的程序集实例。</param>
        /// <param name="startup">找到并成功创建的启动实例（输出）。</param>
        /// <returns>找到并成功实例化则为 true，否则为 false。</returns>
        public static bool TryGetStartup<TStartup>(this Assembly assembly, out TStartup startup)
            where TStartup : class, IStartup
        {
            startup = default!;
            if (!TryGetStartup(assembly, out Type startupType))
            {
                return false;
            }

            try
            {
                var inst = Activator.CreateInstance(startupType);
                if (inst is not TStartup typedInst)
                {
                    return false;
                }
                startup = typedInst;
                return true;
            }
            catch
            {
                // 实例化失败视为未找到合规的启动项
                startup = default!;
                return false;
            }
        }

        /// <summary>
        /// 尝试从给定的 <see cref="Assembly"/> 中找到并实例化符合条件的启动类（非泛型）。
        /// 实例化失败或类型不匹配将返回 false。
        /// </summary>
        /// <param name="assembly">要搜索的程序集实例。</param>
        /// <param name="startup">找到并成功创建的 <see cref="IStartup"/> 实例（输出）。</param>
        /// <returns>找到并成功实例化则为 true，否则为 false。</returns>
        public static bool TryGetStartup(this Assembly assembly, out IStartup startup)
        {
            startup = default!;
            if (!TryGetStartup(assembly, out Type startupType))
            {
                return false;
            }

            try
            {
                var inst = Activator.CreateInstance(startupType);
                if (inst is not IStartup typedInst)
                {
                    return false;
                }
                startup = typedInst;
                return true;
            }
            catch
            {
                // 实例化失败视为未找到合规的启动项
                startup = default!;
                return false;
            }
        }

        /// <summary>
        /// 在程序集内查找第一个非抽象且可以赋值给 <see cref="StartupType"/> 的类型。
        /// </summary>
        /// <param name="assembly">要搜索的程序集。</param>
        /// <param name="startupType">找到的类型（输出）。</param>
        /// <returns>找到则为 true，否则为 false。</returns>
        public static bool TryGetStartup(this Assembly assembly, out Type startupType)
        {
            startupType = default!;
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly), "寻找启动接口的库不能为空");

            try
            {
                startupType = assembly.GetTypes().FirstOrDefault(t => !t.IsAbstract && StartupType.IsAssignableFrom(t));
            }
            catch
            {
                return false;
            }
            return startupType != null;
        }

        /// <summary>
        /// 从指定路径加载一个程序集文件并尝试调用其中的 IStartup.AddService。
        /// - 若文件为非托管（native）库，则使用 <see cref="NativeLibrary.Load(string)"/> 直接加载（不会查找 IStartup）。
        /// - 若为托管程序集，则通过给定的 <see cref="AssemblyLoadContext"/> 加载并查找 IStartup 实现。
        /// </summary>
        /// <param name="context">用于加载托管程序集的 AssemblyLoadContext。</param>
        /// <param name="path">要加载的程序集或本机库文件路径。</param>
        /// <param name="services">用于传递到 IStartup.AddService 的服务集合。</param>
        public static void LoadAssemblyAndStartupFormPath(this AssemblyLoadContext context, string path, IServiceCollection services)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "程序集加载上下文不能为空");
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path), "程序集路径不能为空");
            if (services == null)
                throw new ArgumentNullException(nameof(services), "服务集合不能为空");
            if (!File.Exists(path))
                throw new FileNotFoundException("程序集文件未找到", path);

            if (!IsManagedAssembly(path))
            {
                // 非托管库：使用 NativeLibrary 载入（不会执行 .NET 反射查找）
                NativeLibrary.Load(path);
                return;
            }

            try
            {
                // 托管程序集：装载并查找 IStartup
                Assembly assembly = context.LoadFromAssemblyPath(path);
                if (!assembly.TryGetStartup(out IStartup startup))
                    return;

                startup.AddService(services);
            }
            catch
            {
                // 装载或实例化失败时不抛出（调用者已在外部负责记录/处理）
            }
        }

        /// <summary>
        /// 遍历指定文件夹，加载其中的 *.dll 文件：
        /// - 对非托管库使用 <see cref="NativeLibrary.Load(string)"/>；
        /// - 对托管程序集通过 <see cref="AssemblyLoadContext"/> 装载并调用 IStartup.AddService（若存在）。
        /// </summary>
        /// <param name="context">用于加载托管程序集的 AssemblyLoadContext。</param>
        /// <param name="FolderPath">包含 dll 的文件夹路径。</param>
        /// <param name="services">用于传递到 IStartup.AddService 的服务集合。</param>
        public static void LoadAssemblyAndStartupFormFolderPath(this AssemblyLoadContext context, string FolderPath, IServiceCollection services)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "程序集加载上下文不能为空");
            if (string.IsNullOrEmpty(FolderPath))
                throw new ArgumentNullException(nameof(FolderPath), "要加载的程序集文件夹路径不能为空");
            if (services == null)
                throw new ArgumentNullException(nameof(services), "服务集合不能为空");
            if (!Directory.Exists(FolderPath))
                throw new FileNotFoundException("要加载的程序集文件夹路径未找到", FolderPath);

            string[] dllPaths = Directory.GetFiles(FolderPath, "*.dll", SearchOption.AllDirectories);
            foreach (var path in dllPaths)
            {
                if (!IsManagedAssembly(path))
                {
                    // 非托管库：尝试加载并继续下一文件
                    NativeLibrary.Load(path);
                    continue;
                }

                Assembly assembly = context.LoadFromAssemblyPath(path);
            }

            // 托管程序集：装载并调用 IStartup（每个文件独立处理）
            foreach (var assembly in context.Assemblies)
            {
                if (!assembly.TryGetStartup(out IStartup startup))
                    continue;

                startup.AddService(services);
            }
        }

        /// <summary>
        /// 判断指定文件是否为托管 (.NET) 程序集。
        /// 实现策略：
        /// 1) 优先使用 <see cref="PEReader"/> 检查 PE headers 中是否包含 CLR header（CorHeader）并且具有元数据；
        /// 2) 当无法使用 PEReader 时回退到 <see cref="AssemblyName.GetAssemblyName(string)"/>（该方法在非托管文件上会抛出 <see cref="BadImageFormatException"/>）。
        /// 返回 true 表示看起来是托管程序集（包含 CLR header / 可被识别为 .NET 程序集）。
        /// </summary>
        /// <param name="path">要检查的文件路径。</param>
        /// <returns>是托管程序集返回 true，否则返回 false。</returns>
        private static bool IsManagedAssembly(string path)
        {
            // 尝试使用 PEReader（更可靠）
            try
            {
                using var stream = File.OpenRead(path);
                using var peReader = new PEReader(stream, PEStreamOptions.PrefetchEntireImage);
                var headers = peReader.PEHeaders;
                // 有 CLR header 通常表示为托管程序集（包括混合模式 / C++/CLI）
                if (headers?.CorHeader != null && peReader.HasMetadata)
                    return true;

                return false;
            }
            catch
            {
                // 若 PEReader 不可用或解析失败，回退到 AssemblyName.GetAssemblyName
            }

            try
            {
                // AssemblyName.GetAssemblyName 会在非托管文件上抛出 BadImageFormatException
                _ = AssemblyName.GetAssemblyName(path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}