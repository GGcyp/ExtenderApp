using System.Windows;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Models;
using ExtenderApp.Services;
using ExtenderApp.Views;

namespace ExtenderApp.MainViews.Models
{
    /// <summary>
    /// 主视图模型类，实现了INotifyPropertyChanged接口用于属性变更通知
    /// </summary>
    public class MainModel : ExtenderAppModel
    {
        /// <summary>
        /// 当前主视图接口
        /// </summary>
        public IView? CurrentMainView { get; set; }

        /// <summary>
        /// 当前过场动画视图接口
        /// </summary>
        public IView? CurrentCutsceneView { get; set; }

        /// <summary>
        /// 当前视图接口
        /// </summary>
        public IView? CurrentView { get; set; }

        /// <summary>
        /// 返回主页的动作委托
        /// </summary>
        public Action? ToHomeAction { get; set; }

        /// <summary>
        /// 执行运行的动作委托
        /// </summary>
        public Action? ToRunAction { get; set; }

        /// <summary>
        /// 选中的插件详情信息
        /// </summary>
        public PluginDetails? SelectedModDetails { get; set; }

        /// <summary>
        /// 插件商店实例
        /// </summary>
        public PluginStore? PluginStore { get; set; }

        #region Message

        /// <summary>
        /// 提示词文本
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 提示词边距设置
        /// </summary>
        public Thickness MessageMargin { get; set; }

        public HorizontalAlignment MessageHorizontalAlignment { get; set; }

        public VerticalAlignment MessageVerticalAlignment { get; set; }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="store">插件商店实例</param>
        public MainModel(PluginStore store)
        {
            PluginStore = store;
            MessageHorizontalAlignment = HorizontalAlignment.Center;
            MessageVerticalAlignment = VerticalAlignment.Center;
        }

        public void ShowMessage(string message,
            ExHorizontalAlignment horizontalAlignment,
            ExVerticalAlignment verticalAlignment,
            ExThickness messageThickness)
        {
            Message = message;
            MessageMargin = messageThickness.ToThickness();
            MessageHorizontalAlignment = horizontalAlignment.ToHorizontalAlignment();
            MessageVerticalAlignment = verticalAlignment.ToVerticalAlignment();
        }
    }
}
