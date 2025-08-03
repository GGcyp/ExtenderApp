namespace ExtenderApp.Abstract.View
{
    /// <summary>
    /// 主题管理器接口
    /// </summary>
    public interface IThemeManager
    {
        /// <summary>
        /// 应用指定的主题
        /// </summary>
        /// <param name="themeName">主题名称</param>
        void ApplyTheme(string themeName);

        /// <summary>
        /// 注册主题。
        /// </summary>
        /// <param name="themeName">主题名称。</param>
        /// <param name="resourcePath">资源路径。</param>
        void RegisterTheme(string themeName, string resourcePath);

        /// <summary>
        /// 注册主题。
        /// </summary>
        /// <param name="themeName">主题名称。</param>
        /// <param name="assemblyName">程序集名称。</param>
        /// <param name="resourcePath">资源路径。</param>
        void RegisterTheme(string themeName, string assemblyName, string resourcePath);
    }
}
