

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示插件信息的结构体。
    /// </summary>
    public struct PluginInfo
    {
        /// <summary>
        /// 获取或设置插件的标题。
        /// </summary>
        public string? PluginTitle { get; set; }

        /// <summary>
        /// 获取或设置插件的描述。
        /// </summary>
        public string? PluginDescription { get; set; }

        /// <summary>
        /// 获取或设置插件的版本号。
        /// </summary>
        public string? PluginVersion { get; set; }

        /// <summary>
        /// 获取或设置插件版本的详细信息。
        /// </summary>
        public string? PluginVersionInformation { get; set; }

        /// <summary>
        /// 获取或设置插件启动的DLL文件路径。
        /// </summary>
        public string? PluginStartupDll { get; set; }

        /// <summary>
        /// 获取或设置插件包的路径。
        /// </summary>
        public string? PackPath { get; set; }
    }
}
