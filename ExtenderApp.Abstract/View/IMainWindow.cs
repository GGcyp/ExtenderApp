using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 主视图窗口接口
    /// </summary>
    public interface IMainWindow : IWindow
    {
        /// <summary>
        /// 绘制消息到主窗口
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="horizontalAlignment">水平对齐方式</param>
        /// <param name="verticalAlignment">垂直对齐方式</param>
        /// <param name="messageThickness">消息边距</param>
        void DisplayMessageToMainWindow(string message,
            ExHorizontalAlignment horizontalAlignment,
            ExVerticalAlignment verticalAlignment,
            ExThickness messageThickness);
    }
}