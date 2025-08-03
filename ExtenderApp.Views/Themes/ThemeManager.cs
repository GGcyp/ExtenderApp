using System.Reflection;
using System.Windows;
using ExtenderApp.Abstract.View;

namespace ExtenderApp.Views.Themes
{
    /// <summary>
    /// 主题管理器类，实现了IThemeManager接口
    /// </summary>
    internal class ThemeManager : IThemeManager
    {
        /// <summary>
        /// 存储主题的资源字典
        /// </summary>
        private readonly Dictionary<string, ResourceDictionary> _themes;

        /// <summary>
        /// 上次应用的主题名称
        /// </summary>
        private string? lastTheme;

        /// <summary>
        /// 主题管理器构造函数
        /// </summary>
        public ThemeManager()
        {
            _themes = new();
        }

        /// <summary>
        /// 应用指定的主题
        /// </summary>
        /// <param name="themeName">主题名称</param>
        public void ApplyTheme(string themeName)
        {
            if (themeName == lastTheme)
                return;

            ResourceDictionary theme = _themes[themeName];

            if (!string.IsNullOrEmpty(lastTheme))
            {
                ResourceDictionary last = _themes[lastTheme];
                Application.Current.Resources.MergedDictionaries.Remove(last);
            }

            Application.Current.Resources.MergedDictionaries.Add(theme);
            lastTheme = themeName;
        }

        /// <summary>
        /// 注册主题，使用当前程序集
        /// </summary>
        /// <param name="themeName">主题名称</param>
        /// <param name="resourcePath">资源路径</param>
        public void RegisterTheme(string themeName, string resourcePath)
        {
            string currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            RegisterTheme(themeName, currentAssemblyName, resourcePath);
        }

        /// <summary>
        /// 注册主题
        /// </summary>
        /// <param name="themeName">主题名称</param>
        /// <param name="assemblyName">程序集名称</param>
        /// <param name="resourcePath">资源路径</param>
        public void RegisterTheme(string themeName, string assemblyName, string resourcePath)
        {
            if (_themes.ContainsKey(themeName))
            {
                throw new InvalidOperationException("不能重复注册样式");
            }

            string urlString = $"/{assemblyName};component/{resourcePath}";

            ResourceDictionary resource = new();
            resource.Source = new Uri(urlString, UriKind.RelativeOrAbsolute);
            _themes.Add(themeName, resource);
        }
    }
}
