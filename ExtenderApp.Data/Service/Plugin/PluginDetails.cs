using System.IO;
using System.Runtime.Loader;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 插件详细信息类
    /// </summary>
    public sealed class PluginDetails
    {
        /// <summary>
        /// 插件信息对象
        /// </summary>
        private readonly PluginInfo _pluginInfo;

        /// <summary>
        /// 获取插件标题
        /// </summary>
        public string? Title => _pluginInfo.PluginTitle;

        /// <summary>
        /// 获取插件描述
        /// </summary>
        public string? Description => _pluginInfo.PluginDescription;

        /// <summary>
        /// 获取插件版本号
        /// </summary>
        public Version? Version { get; }

        /// <summary>
        /// 获取插件启动DLL文件路径
        /// </summary>
        public string? StartupDll => _pluginInfo.PluginStartupDll;

        /// <summary>
        /// 获取插件打包路径
        /// </summary>
        public string? PackPath => _pluginInfo.PackPath;

        /// <summary>
        /// 获取或设置插件加载上下文
        /// </summary>
        public AssemblyLoadContext? LoadContext { get; set; }

        /// <summary>
        /// 获取或设置插件文件夹目录路径
        /// </summary>
        public string? PluginDirectoryPath { get; set; }

        /// <summary>
        /// 获取或设置插件启动类型
        /// </summary>
        public Type? StartupType { get; set; }

        /// <summary>
        /// 获取或设置过场动画视图类型。
        /// </summary>
        /// <value>
        /// 返回或设置一个表示过场动画视图类型的 <see cref="Type"/> 对象。
        /// 如果该属性为 null，则表示没有设置过场动画视图类型。
        /// </value>
        public Type? CutsceneViewType { get; set; }

        /// <summary>
        /// 获取或设置插件作用域
        /// </summary>
        public string PluginScope { get; set; }

        /// <summary>
        /// 获取插件图标路径。
        /// </summary>
        /// <returns>返回插件图标路径的字符串。</returns>
        public string? PluginIcon { get; set; }

        /// <summary>
        /// 初始化一个插件详细信息实例
        /// </summary>
        /// <param name="modeInfo">插件信息对象</param>
        public PluginDetails(PluginInfo modeInfo) : this()
        {
            _pluginInfo = modeInfo;
            PluginIcon = modeInfo.PluginIcon;
            Version = string.IsNullOrEmpty(modeInfo.PluginVersion) ? null : new Version(modeInfo.PluginVersion);
        }

        /// <summary>
        /// 初始化一个插件详细信息实例
        /// </summary>
        public PluginDetails()
        {
            PluginScope = string.Empty;
            PluginDirectoryPath = string.Empty;
            StartupType = null;
            LoadContext = null;
            Version = _pluginInfo.PluginVersion == null ? null : new Version(_pluginInfo.PluginVersion);
        }
    }
}
