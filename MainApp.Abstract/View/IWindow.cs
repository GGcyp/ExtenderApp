

namespace MainApp.Abstract
{
    /// <summary>
    /// 定义一个窗口接口。
    /// </summary>
    public interface IWindow
    {
        /// <summary>
        /// 显示窗口。
        /// </summary>
        void Show();

        /// <summary>
        /// 以对话框形式显示窗口，并返回用户交互的结果。
        /// </summary>
        /// <returns>如果用户接受对话框中的操作，则返回 true；如果用户取消操作，则返回 false；如果用户关闭对话框，则返回 null。</returns>
        bool? ShowDialog();
    }
}
