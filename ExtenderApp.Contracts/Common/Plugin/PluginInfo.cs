namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 表示插件信息的结构体。
    /// </summary>
    public struct PluginInfo : IEquatable<PluginInfo>
    {
        /// <summary>
        /// 获取或设置插件标题。
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 获取或设置插件描述。
        /// </summary>
        public string? Desc { get; set; }

        /// <summary>
        /// 获取或设置插件版本号。
        /// </summary>
        public string? Ver { get; set; }

        /// <summary>
        /// 获取或设置插件启动的 DLL 文件路径。
        /// </summary>
        public string? Startup { get; set; }

        /// <summary>
        /// 获取或设置插件包的路径。
        /// </summary>
        public string? Pack { get; set; }

        /// <summary>
        /// 获取或设置插件图标。
        /// </summary>
        /// <value>
        /// 插件图标，返回值为 null 表示没有设置图标。
        /// </value>
        public string? Icon { get; set; }

        /// <summary>
        /// 判断当前实例与指定实例是否相等。
        /// </summary>
        /// <param name="other">要比较的 <see cref="PluginInfo"/> 实例。</param>
        /// <returns>若相等则为 true，否则为 false。</returns>
        public bool Equals(PluginInfo other)
        {
            return Title == other.Title &&
                   Desc == other.Desc &&
                   Ver == other.Ver &&
                   Startup == other.Startup &&
                   Pack == other.Pack &&
                   Icon == other.Icon;
        }
    }
}