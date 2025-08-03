using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ExtenderApp.Data;

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
            //var modDetails = (PluginDetails)DataContext;
            //Title = modDetails.Title;
            //Description = modDetails.Description;
            //Version = modDetails.Version is null ? "未知版本" : modDetails.Version.ToString();
            //PluginIcon = modDetails.PluginIcon is null ? DefaltPluginPathGeometry : Geometry.Parse(modDetails.PluginIcon);
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

        #endregion

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

        #endregion

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

        #endregion

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

        #endregion
    }
}
