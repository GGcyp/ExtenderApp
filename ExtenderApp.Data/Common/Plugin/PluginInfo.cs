namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示插件信息的结构体。
    /// </summary>
    public struct PluginInfo : IEquatable<PluginInfo>
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
        /// 获取或设置插件启动的DLL文件路径。
        /// </summary>
        public string? PluginStartupDll { get; set; }

        /// <summary>
        /// 获取或设置插件包的路径。
        /// </summary>
        public string? PackPath { get; set; }

        /// <summary>
        /// 获取或设置插件图标。
        /// </summary>
        /// <value>
        /// 插件图标，返回值为null表示没有设置图标。
        /// </value>
        public string? PluginIcon { get; set; }

        public bool Equals(PluginInfo other)
        {
            return PluginTitle == other.PluginTitle &&
                   PluginDescription == other.PluginDescription &&
                   PluginVersion == other.PluginVersion &&
                   PluginStartupDll == other.PluginStartupDll &&
                   PackPath == other.PackPath &&
                   PluginIcon == other.PluginIcon;
        }
    }
}