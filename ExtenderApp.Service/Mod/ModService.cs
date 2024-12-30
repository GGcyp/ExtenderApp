using System.Reflection;
using System.Runtime.Loader;
using AppHost.Builder.Extensions;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Services;

namespace ExtenderApp.Service
{
    internal class ModService : IModService
    {
        private const string MOD_INIT_FILE_NAME = "init.json";

        /// <summary>
        /// 模组厂库
        /// </summary>
        private readonly ModStore _modStore;
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
        public ModService(ModStore mods, IPathService pathProvider, IJsonParser parser, IScopeExecutor executor)
        {
            _modStore = mods;
            _scopeExecutor = executor;
            _pathProvider = pathProvider;
            _jsonParser = parser;

            LoadModInfo();
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

        public void LoadModInfo(string modFolderPath = null)
        {
            if (string.IsNullOrEmpty(modFolderPath)) modFolderPath = _pathProvider.ModsPath;
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

            details.StartupType = _scopeExecutor.LoadScope<ModEntityStartup>(startAssembly).StartType;

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
        public void UnloadMod(ModDetails details)
        {
            _scopeExecutor.UnLoadScope(details.Title);
            details.LoadContext.Unload();
        }
    }
}
