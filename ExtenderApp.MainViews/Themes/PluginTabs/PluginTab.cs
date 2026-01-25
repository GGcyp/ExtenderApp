using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExtenderApp.MainViews.Themes
{
    public class PluginTab : Button
    {
        static PluginTab()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PluginTab),
                new FrameworkPropertyMetadata(typeof(PluginTab)));
        }

        public PluginTab()
        {
        }

        #region 名称

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title",
                typeof(string),
                typeof(PluginTab));

        #endregion 名称

        #region 简介

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description",
                typeof(string),
                typeof(PluginTab));

        #endregion 简介

        #region 版本号

        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version",
                typeof(string),
                typeof(PluginTab));

        #endregion 版本号

        #region 图标

        public Geometry PluginIcon
        {
            get { return (Geometry)GetValue(PluginIconProperty); }
            set { SetValue(PluginIconProperty, value); }
        }

        public static readonly DependencyProperty PluginIconProperty =
            DependencyProperty.Register("PluginIcon",
                typeof(Geometry),
                typeof(PluginTab),
                new PropertyMetadata());

        #endregion 图标
    }
}