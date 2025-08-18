

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义一个服务，用于管理与主应用程序窗口的交互。
    /// </summary>
    /// <remarks>
    /// 此接口提供方法和属性来控制或检索有关主应用程序窗口的信息。
    /// 实现此接口的类应该处理与平台相关或应用程序特定的主窗口管理细节。
    /// </remarks>
    public interface IMainWindowService
    {
        /// <summary>
        /// 获取当前的主窗口实例。可能返回 null，如果没有主窗口实例。
        /// </summary>
        IMainWindow? CurrentMainWindow { get; }

        /// <summary>
        /// 创建一个主窗口实例。
        /// </summary>
        /// <returns>返回主窗口的实例。</returns>
        IMainWindow CreateMainWindow();
    }
}
