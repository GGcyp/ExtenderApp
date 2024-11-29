using System.Reflection;
using System.Runtime.Loader;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using AppHost.Builder.Extensions;


namespace ExtenderApp.Mod
{
    /// <summary>
    /// Mod加载器类
    /// </summary>
    internal class ModLoader : IModLoader
    {
        /// <summary>
        /// 作用域执行器
        /// </summary>
        private IScopeExecutor _scopeExecutor;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="executor">作用域执行器</param>
        public ModLoader(IScopeExecutor executor)
        {
            _scopeExecutor = executor;
        }

        /// <summary>
        /// 加载Mod
        /// </summary>
        /// <param name="details">Mod详情</param>
        public void Load(ModDetails details)
        {
            ArgumentNullException.ThrowIfNull(details, nameof(details));

            if (string.IsNullOrEmpty(details.Path) || string.IsNullOrEmpty(details.StartupDll))
                throw new ArgumentNullException("Mod详情中的路径或启动DLL不能为空");

            var loadContext = new AssemblyLoadContext(details.Title, true);
            details.LoadContext = loadContext;
            string dllPath = Path.Combine(details.Path, details.StartupDll);
            var startAssembly = LoadAssembly(loadContext, dllPath);

            details.StartupType = _scopeExecutor.LoadScope<ModEntityStartup>(startAssembly).StartType;

            string packPath = Path.Combine(details.Path, AppSetting.AppPackFolderName);
            if (!Directory.Exists(packPath)) return;

            foreach (var dir in Directory.GetFiles(packPath))
            {
                LoadAssembly(loadContext, dir);
            }
        }

        /// <summary>
        /// 加载程序集
        /// </summary>
        /// <param name="context">程序集加载上下文</param>
        /// <param name="path">程序集路径</param>
        /// <returns>加载的程序集</returns>
        private Assembly LoadAssembly(AssemblyLoadContext context, string path)
        {
            Assembly reslut;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                reslut = context.LoadFromStream(stream);
            }
            return reslut;
        }

        /// <summary>
        /// 卸载Mod
        /// </summary>
        /// <param name="details">Mod详情</param>
        public void Unload(ModDetails details)
        {
            _scopeExecutor.UnLoadScope(details.Title);
            details.LoadContext.Unload();
        }
    }
}
