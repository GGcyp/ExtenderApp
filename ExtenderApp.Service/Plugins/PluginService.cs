﻿using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using AppHost.Builder.Extensions;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;
using ExtenderApp.Data;

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

            return _pluginStore.FirstOrDefault(m => m.StartupDll == modStartDLLName);
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
                details.PluginFolderPath = dir;

                _pluginStore.Add(details);
            }
        }

        /// <summary>
        /// 卸载插件
        /// </summary>
        /// <param name="details">插件详细信息</param>
        public void UnloadPlugin(PluginDetails details)
        {
            if (details.IsStandingModel) return;
            _scopeExecutor.UnLoadScope(details.PluginScope);
        }

        /// <summary>
        /// 异步加载插件
        /// </summary>
        /// <param name="details">插件详情信息</param>
        /// <exception cref="ArgumentNullException">当插件详情为null或关键路径为空时抛出</exception>
        /// <exception cref="InvalidOperationException">当未找到插件启动项时抛出</exception>
        public async Task LoadPluginAsync(PluginDetails details)
        {
            ArgumentNullException.ThrowIfNull(details, nameof(details));

            bool isStart = details.LoadContext == null;
            if (isStart)
            {
                await LoadAssemblyAsync(details);
            }

            var loadContext = details.LoadContext;

            var startAssembly = details.StartAssembly;

            _pluginTransform.Details = details;
            var modStartup = _scopeExecutor.LoadScope<PluginEntityStartup>(startAssembly, _pluginTransform.AddServiceToPluginScope);
            modStartup.ConfigureDetails(details);

            if (modStartup == null)
                throw new InvalidOperationException(string.Format("未找到这个模组的启动项：{0}", details.Title));
            details.StartupType = modStartup.StartType;
            details.CutsceneViewType = modStartup.CutsceneViewType;

            if (isStart)
            {
                modStartup.ConfigureBinaryFormatterStore(_binaryFormatterStore);
            }
        }

        private async Task LoadAssemblyAsync(PluginDetails details)
        {
            if (string.IsNullOrEmpty(details.PluginFolderPath) || string.IsNullOrEmpty(details.StartupDll))
                throw new ArgumentNullException("Mod详情中的路径或启动DLL不能为空");

            var loadContext = new AssemblyLoadContext(details.Title, true);
            details.LoadContext = loadContext;
            string dllPath = Path.Combine(details.PluginFolderPath, details.StartupDll);

            //添加模组依赖库
            string packName = string.IsNullOrEmpty(details.PackPath) ? _pathProvider.PackFolderName : details.PackPath;
            string packPath = Path.Combine(details.PluginFolderPath, packName);
            if (Directory.Exists(packPath))
            {
                await Task.Run(() =>
                {
                    foreach (var dir in Directory.GetFiles(packPath))
                    {
                        using (var stream = new FileStream(dir, FileMode.Open, FileAccess.Read))
                        {
                            loadContext.LoadFromStream(stream);
                        }
                    }
                });
            }

            details.StartAssembly = LoadAssembly(loadContext, dllPath);
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
            /// <param name="services">服务集合</param>
            /// <param name="options">作用域选项</param>
            public void AddServiceToPluginScope(IServiceCollection services, ScopeOptions options)
            {
                Details.PluginScope = options.ScopeName;

                services.AddSingleton<IServiceStore, PluginServiceStore>();
                services.AddSingleton(Details);
            }
        }
    }
}
