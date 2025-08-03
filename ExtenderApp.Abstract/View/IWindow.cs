

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义一个窗口接口。
    /// </summary>
    public interface IWindow : IView
    {
        /// <summary>
        /// 获取或设置窗口的最小宽度。
        /// </summary>
        double MinWidth { get; set; }

        /// <summary>
        /// 获取或设置窗口的最大宽度。
        /// </summary>
        double MaxWidth { get; set; }

        /// <summary>
        /// 获取或设置窗口的宽度。
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// 获取或设置窗口的最小高度。
        /// </summary>
        double MinHeight { get; set; }

        /// <summary>
        /// 获取或设置窗口的最大高度。
        /// </summary>
        double MaxHeight { get; set; }

        /// <summary>
        /// 获取或设置窗口的高度。
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// 获取或设置标题
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// 显示窗口。
        /// </summary>
        void Show();

        /// <summary>
        /// 以对话框形式显示窗口，并返回用户交互的结果。
        /// </summary>
        /// <returns>如果用户接受对话框中的操作，则返回 true；如果用户取消操作，则返回 false；如果用户关闭对话框，则返回 null。</returns>
        bool? ShowDialog();

        /// <summary>
        /// 显示主视图。
        /// </summary>
        /// <param name="mainView">主视图接口对象。</param>
        void ShowView(IMainView mainView);
    }
}
