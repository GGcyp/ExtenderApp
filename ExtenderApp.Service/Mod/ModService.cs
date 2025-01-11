using System.Reflection;
using System.Runtime.Loader;
using AppHost.Builder.Extensions;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Services;
using ExtenderApp.Common.Error;

namespace ExtenderApp.Service
{
    /// <summary>
    /// ModService类，实现了IModService接口
    /// </summary>
    internal class ModService : IModService
    {
        private const string MOD_INIT_FILE_NAME = "init.json";

        /// <summary>
        /// 模组厂库
        /// </summary>
        private readonly ModStore _modStore;
        private readonly LoadModTransform _modTransform;


        /// <summary>
        /// 作用域执行器
        /// </summary>
        private IScopeExecutor _scopeExecutor;
        /// <summary>
        /// 路径提供者接口实例
        /// </summary>
        private IPathService _pathProvider;
        /// <summary>
        /// Json文件接口
        /// </summary>
        private IJsonParser _jsonParser;
        private IBinaryFormatterStore _store;

        public ModService(ModStore mods, IPathService pathProvider, IJsonParser parser, IScopeExecutor executor, IBinaryFormatterStore store)
        {
            _modStore = mods;
            _scopeExecutor = executor;
            _pathProvider = pathProvider;
            _jsonParser = parser;
            _modTransform = new();
            _store = store;

            LoadModInfo(_pathProvider.ModsPath);
        }

        private bool Contains(ModeInfo info)
        {
            return GetModDetails(info.ModStartupDll) is not null;
        }

        public ModDetails? GetModDetails(string modStartDLLName)
        {
            if (string.IsNullOrEmpty(modStartDLLName)) return null;

            for (int i = 0; i < _modStore.Count; i++)
            {
                if (_modStore[i].StartupDll == modStartDLLName) return _modStore[i];
            }
            return null;
        }

        public void LoadModInfo(string modFolderPath)
        {
            string.IsNullOrEmpty(modFolderPath).ArgumentFalse(nameof(IModService), "加载模组地址不能为空");
            var modPaths = Directory.GetDirectories(modFolderPath);

            foreach (var dir in modPaths)
            {
                var fileInfo = new LocalFileInfo(Path.Combine(dir, MOD_INIT_FILE_NAME));
                if (!fileInfo.Exists) continue;

                //解析模组的信息
                ModeInfo info = _jsonParser.Deserialize<ModeInfo>(new FileOperate(fileInfo));

                if (string.IsNullOrEmpty(info.ModStartupDll)
                    || Contains(info)) continue;

                //加载模组主程序集
                ModDetails details = new ModDetails(info);
                details.Path = dir;

                _modStore.Add(details);
            }
        }

        public void LoadMod(ModDetails details)
        {
            ArgumentNullException.ThrowIfNull(details, nameof(details));

            if (details.LoadContext is not null)
                return;

            if (string.IsNullOrEmpty(details.Path) || string.IsNullOrEmpty(details.StartupDll))
                throw new ArgumentNullException("Mod详情中的路径或启动DLL不能为空");

            var loadContext = new AssemblyLoadContext(details.Title, true);
            details.LoadContext = loadContext;
            string dllPath = Path.Combine(details.Path, details.StartupDll);
            var startAssembly = LoadAssembly(loadContext, dllPath);

            _modTransform.Details = details;
            var modStartup = _scopeExecutor.LoadScope<ModEntityStartup>(startAssembly, _modTransform.AddServiceToModeScope);

            if (modStartup == null)
                throw new InvalidOperationException(string.Format("未找到这个模组的启动项：{0}", details.Title));
            details.StartupType = modStartup.StartType;
            modStartup.ConfigureBinaryFormatterStore(_store);


            //添加模组依赖库
            string packName = string.IsNullOrEmpty(details.PackPath) ? _pathProvider.PackFolderName : details.PackPath;
            string packPath = Path.Combine(details.Path, packName);
            if (!Directory.Exists(packPath)) return;

            foreach (var dir in Directory.GetFiles(packPath))
            {
                LoadAssembly(loadContext, dir);
            }
        }

        /// <summary>
        /// 卸载Mod
        /// </summary>
        /// <param name="details">Mod详情</param>
        public void UnloadMod(ModDetails details)
        {
            _scopeExecutor.UnLoadScope(details.Title);
            details.LoadContext.Unload();
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
        /// 用于加载模组临时类。
        /// </summary>
        private class LoadModTransform
        {
            /// <summary>
            /// 模组的详细信息。
            /// </summary>
            public ModDetails Details { get; set; }

            /// <summary>
            /// 向服务集合中添加模组作用域。
            /// </summary>
            /// <param name="options">作用域选项</param>
            /// <param name="services">服务集合</param>
            /// <remarks>
            /// 此方法会将<see cref="IServiceStore"/>服务注册为单例，并将其实现类设置为<see cref="ScopeServiceStore"/>。
            /// 同时，还会将<see cref="Details"/>属性作为单例服务添加到服务集合中。
            /// </remarks>
            public void AddServiceToModeScope(ScopeOptions options, IServiceCollection services)
            {
                Details.ModScope = options.ScopeName;

                services.AddSingleton<IServiceStore, ScopeServiceStore>();
                services.AddSingleton(Details);
            }
        }
    }
}
