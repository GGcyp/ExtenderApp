

using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 主视图窗口接口
    /// </summary>
    public interface IMainWindow : IWindow
    {
        /// <summary>
        /// 在主窗口显示消息通知
        /// </summary>
        /// <param name="message">要显示的文本消息内容</param>
        /// <param name="horizontalAlignment">
        /// 消息框的水平对齐方式，指定为以下值之一：
        /// <list type="bullet">
        /// <item>Left - 左对齐</item>
        /// <item>Center - 水平居中</item>
        /// <item>Right - 右对齐</item>
        /// <item>Stretch - 拉伸填充</item>
        /// </list>
        /// </param>
        /// <param name="verticalAlignment">
        /// 消息框的垂直对齐方式，指定为以下值之一：
        /// <list type="bullet">
        /// <item>Top - 顶部对齐</item>
        /// <item>Center - 垂直居中</item>
        /// <item>Bottom - 底部对齐</item>
        /// <item>Stretch - 拉伸填充</item>
        /// </list>
        /// </param>
        /// <param name="messageThickness">
        /// 消息框的外边距设置，使用 <see cref="ExThickness"/> 结构指定：
        /// <list type="table">
        /// <listheader>
        /// <term>属性</term>
        /// <description>说明</description>
        /// </listheader>
        /// <item>
        /// <term>Left</term>
        /// <description>左边距（像素）</description>
        /// </item>
        /// <item>
        /// <term>Top</term>
        /// <description>上边距（像素）</description>
        /// </item>
        /// <item>
        /// <term>Right</term>
        /// <description>右边距（像素）</description>
        /// </item>
        /// <item>
        /// <term>Bottom</term>
        /// <description>下边距（像素）</description>
        /// </item>
        /// </list>
        /// </param>
        void DisplayMessageToMainWindow(string message, 
            ExHorizontalAlignment horizontalAlignment, 
            ExVerticalAlignment verticalAlignment, 
            ExThickness messageThickness);
    }
}
