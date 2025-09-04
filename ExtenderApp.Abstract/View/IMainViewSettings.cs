using System.Collections;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义主视图设置接口，用于设置和管理主视图的配置和布局。
    /// </summary>
    public interface IMainViewSettings
    {
        /// <summary>
        /// 获取对应的设置视图。
        /// </summary>
        IView SettingsView { get; }

        /// <summary>
        /// 生成设置导航按钮。
        /// </summary>
        /// <param name="textUI">需要被生成的标识</param>
        /// <returns>生成的按钮</returns>
        object CreateSettingsNavigationButton(object textUI);

        /// <summary>
        /// 配置设置视图的导航项。
        /// </summary>
        /// <param name="list">设置导航项集合，通常为可绑定的列表对象。</param>
        void SettingNavigationConfig(IList list);

        /// <summary>
        /// 设置顶部设置项，将指定的列表作为参数传入，用于添加或修改顶部的设置选项。
        /// </summary>
        /// <param name="list">顶部设置列表，通常为可绑定的列表对象。</param>
        void TopSetting(IList list);
    }
}
