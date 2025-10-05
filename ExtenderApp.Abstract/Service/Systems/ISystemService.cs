

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供对系统级服务的抽象访问接口
    /// </summary>
    public interface ISystemService
    {
        /// <summary>
        /// 获取系统剪贴板服务的实例。
        /// </summary>
        /// <value>
        /// 返回 <see cref="IClipboard"/> 接口的实现实例，用于操作剪贴板内容。
        /// </value>
        IClipboard Clipboard { get; }

        /// <summary>
        /// 获取键盘捕获服务的实例。
        /// </summary>
        /// <value>
        /// 返回 <see cref="IKeyCapture"/> 接口的实现实例，用于捕获和处理键盘输入事件。
        /// </value>
        IKeyCapture KeyCapture { get; }
    }
}
