using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Views.Commands;

namespace ExtenderApp.Views
{
    /// <summary>
    /// 主视图设置抽象类，实现主视图的设置导航、顶部按钮等相关功能。
    /// </summary>
    public abstract class MainViewSettings : IMainViewSettings
    {
        /// <summary>
        /// 服务仓库，提供各种应用服务。
        /// </summary>
        protected IServiceStore ServiceStore { get; }

        /// <summary>
        /// 顶部按钮样式。
        /// </summary>
        protected Style TopButtonStyle { get; }

        /// <summary>
        /// 获取主视图对应的设置视图。
        /// </summary>
        public abstract IView SettingsView { get; }

        /// <summary>
        /// 构造函数，初始化服务仓库和顶部按钮样式。
        /// </summary>
        /// <param name="serviceStore">服务仓库实例。</param>
        protected MainViewSettings(IServiceStore serviceStore)
        {
            ServiceStore = serviceStore;

            var dict = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/ExtenderApp.Views;component/Styles/Button.xaml", UriKind.Absolute)
            };
            TopButtonStyle = (Style)dict["EllipticalButton"];
        }

        /// <summary>
        /// 设置顶部设置项，子类可重写实现具体逻辑。
        /// </summary>
        /// <param name="list">顶部设置项集合。</param>
        public virtual void TopSetting(IList list)
        {

        }

        /// <summary>
        /// 配置主视图的导航项，需由子类实现。
        /// </summary>
        /// <param name="list">导航项集合。</param>
        public abstract void SettingNavigationConfig(IList list);

        /// <summary>
        /// 创建设置导航按钮，并注册对应的视图元素。
        /// </summary>
        /// <param name="obj">导航目标元素，必须为TextBlock。</param>
        /// <returns>生成的导航按钮。</returns>
        /// <exception cref="InvalidOperationException">参数类型错误或名称重复时抛出异常。</exception>
        public virtual object CreateSettingsNavigationButton(object obj)
        {
            if (obj == null || obj is not TextBlock textBlock)
            {
                throw new InvalidOperationException("只能传入TextBlock");
            }

            string tag = textBlock.Name;

            if (string.IsNullOrEmpty(tag))
            {
                throw new InvalidOperationException("SettingsView的Name不能为空");
            }

            Button button = new Button
            {
                Content = textBlock.Text,
                Tag = tag,
                Margin = new Thickness(5),
                FontSize = 16,
                CommandParameter = textBlock,
            };
            return button;
        }

        /// <summary>
        /// 获取当前插件的详细信息
        /// </summary>
        /// <returns>插件详细信息</returns>
        public PluginDetails? GetPluginDetails()
        {
            if (ServiceStore is IPuginServiceStore store)
                return store.PuginDetails;

            return null;
        }

        #region CreateTopButton

        /// <summary>
        /// 创建带有Action的顶部按钮。
        /// </summary>
        /// <param name="content">按钮文本。</param>
        /// <param name="action">点击执行的操作。</param>
        /// <returns>生成的按钮。</returns>
        protected Button CreateButton(string content, Action action)
        {
            ICommand command = action == null ? null : new NoValueCommand(action);
            return CreateButton(content, command, TopButtonStyle);
        }

        /// <summary>
        /// 创建带有命令的顶部按钮。
        /// </summary>
        /// <param name="content">按钮文本。</param>
        /// <param name="command">按钮命令。</param>
        /// <returns>生成的按钮。</returns>
        protected Button CreateButton(string content, ICommand command)
        {
            return CreateButton(content, command, TopButtonStyle);
        }

        /// <summary>
        /// 创建带有命令和样式名称的顶部按钮。
        /// </summary>
        /// <param name="content">按钮文本。</param>
        /// <param name="command">按钮命令。</param>
        /// <param name="styleName">样式资源名称。</param>
        /// <returns>生成的按钮。</returns>
        protected Button CreateButton(string content, ICommand command, string styleName)
        {
            if (string.IsNullOrEmpty(styleName))
            {
                return CreateButton(content, command, TopButtonStyle);
            }

            var style = Application.Current.FindResource(styleName) as Style;

            return CreateButton(content, command, style ?? TopButtonStyle);
        }

        /// <summary>
        /// 创建带有命令和样式的顶部按钮。
        /// </summary>
        /// <param name="content">按钮文本。</param>
        /// <param name="command">按钮命令。</param>
        /// <param name="style">按钮样式。</param>
        /// <returns>生成的按钮。</returns>
        protected Button CreateButton(string content, ICommand command, Style style)
        {
            return new Button
            {
                Content = content,
                Command = command,
                Margin = new Thickness(5, 0, 0, 0),
                Style = style,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        #endregion
    }

    /// <summary>
    /// 泛型主视图设置抽象类，自动创建指定类型的设置视图。
    /// </summary>
    /// <typeparam name="TView">视图类型，必须实现IView接口。</typeparam>
    public abstract class MainViewSettings<TView> : MainViewSettings
        where TView : IView
    {
        /// <summary>
        /// 获取主视图对应的设置视图实例。
        /// </summary>
        public override IView SettingsView => View;

        /// <summary>
        /// 视图实例。
        /// </summary>
        protected TView View { get; }

        /// <summary>
        /// 构造函数，初始化服务仓库并创建设置视图。
        /// </summary>
        /// <param name="serviceStore">服务仓库实例。</param>
        public MainViewSettings(IServiceStore serviceStore) : base(serviceStore)
        {
            View = CreateSettingsView();
        }

        /// <summary>
        /// 创建设置视图实例，需由子类实现。
        /// </summary>
        /// <returns>创建的视图实例。</returns>
        protected abstract TView CreateSettingsView();
    }
}
