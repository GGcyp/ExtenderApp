using System.Reflection;
using System.Runtime.Loader;
using AppHost.Builder.Extensions;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Services;
using ExtenderApp.Common.Error;
using ExtenderApp.Common;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 插件服务类
    /// </summary>
    internal class PluginService : IPluginService
    {
        /// <summary>
        /// 初始化文件名
        /// </summary>
        private const string MOD_INIT_FILE_NAME = "init.json";

        /// <summary>
        /// 插件存储
        /// </summary>
        private readonly PluginStore _pluginStore;

        /// <summary>
        /// 插件转换
        /// </summary>
        private readonly LoadPluginTransform _pluginTransform;


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

        /// <summary>
        /// 用于存储二进制格式化的私有变量
        /// </summary>
        private IBinaryFormatterStore _binaryFormatterStore;

        public PluginService(PluginStore pluginStore, IPathService pathProvider, IJsonParser parser, IScopeExecutor executor, IBinaryFormatterStore binaryFormatterStore)
        {
            _pluginStore = pluginStore;
            _scopeExecutor = executor;
            _pathProvider = pathProvider;
            _jsonParser = parser;
            _pluginTransform = new();
            _binaryFormatterStore = binaryFormatterStore;

            LoadPluginInfo(_pathProvider.ModsPath);
        }

        /// <summary>
        /// 判断给定的 ModeInfo 是否存在于 ModStore 中。
        /// </summary>
        /// <param name="info">要判断的 ModeInfo 对象。</param>
        /// <returns>如果 ModeInfo 存在于 ModStore 中，则返回 true；否则返回 false。</returns>
        private bool Contains(PluginInfo info)
        {
            return GetPluginDetails(info.PluginStartupDll) is not null;
        }

        /// <summary>
        /// 根据给定的启动 DLL 名称获取 ModDetails 对象。
        /// </summary>
        /// <param name="modStartDLLName">启动 DLL 的名称。</param>
        /// <returns>如果找到匹配的 ModDetails 对象，则返回该对象；否则返回 null。</returns>
        public PluginDetails? GetPluginDetails(string modStartDLLName)
        {
            if (string.IsNullOrEmpty(modStartDLLName)) return null;

            for (int i = 0; i < _pluginStore.Count; i++)
            {
                if (_pluginStore[i].StartupDll == modStartDLLName) return _pluginStore[i];
            }
            return null;
        }

        /// <summary>
        /// 加载插件信息
        /// </summary>
        /// <param name="modFolderPath">插件文件夹路径</param>
        /// <exception cref="ArgumentException">如果插件文件夹路径为空</exception>
        public void LoadPluginInfo(string modFolderPath)
        {
            string.IsNullOrEmpty(modFolderPath).ArgumentFalse(nameof(IPluginService), "加载模组地址不能为空");
            var modPaths = Directory.GetDirectories(modFolderPath);

            foreach (var dir in modPaths)
            {
                var fileInfo = new LocalFileInfo(Path.Combine(dir, MOD_INIT_FILE_NAME));
                if (!fileInfo.Exists) continue;

                //解析模组的信息
                PluginInfo info = _jsonParser.Read<PluginInfo>(new FileOperateInfo(fileInfo));

                if (Contains(info)) continue;

                //加载模组主程序集
                PluginDetails details = new PluginDetails(info);
                details.Path = dir;

                _pluginStore.Add(details);
            }
        }

        /// <summary>
        /// 加载插件
        /// </summary>
        /// <param name="details">插件详细信息</param>
        /// <exception cref="ArgumentNullException">如果插件详细信息为空或路径、启动DLL为空</exception>
        /// <exception cref="InvalidOperationException">如果未找到模组的启动项</exception>
        public void LoadPlugin(PluginDetails details)
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

            _pluginTransform.Details = details;
            var modStartup = _scopeExecutor.LoadScope<PluginEntityStartup>(startAssembly, _pluginTransform.AddServiceToPluginScope);

            if (modStartup == null)
                throw new InvalidOperationException(string.Format("未找到这个模组的启动项：{0}", details.Title));
            details.StartupType = modStartup.StartType;
            modStartup.ConfigureBinaryFormatterStore(_binaryFormatterStore);


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
        /// 卸载插件
        /// </summary>
        /// <param name="details">插件详细信息</param>
        public void UnloadPlugin(PluginDetails details)
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
        /// 用于加载插件的转换类
        /// </summary>
        private class LoadPluginTransform
        {
            /// <summary>
            /// 获取或设置插件详细信息
            /// </summary>
            public PluginDetails Details { get; set; }

            /// <summary>
            /// 将服务添加到插件的作用域
            /// </summary>
            /// <param name="options">作用域选项</param>
            /// <param name="services">服务集合</param>
            public void AddServiceToPluginScope(ScopeOptions options, IServiceCollection services)
            {
                Details.ModScope = options.ScopeName;

                services.AddSingleton<IServiceStore, ScopeServiceStore>();
                services.AddSingleton(Details);
            }
        }
    }
}
