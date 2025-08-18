namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 主窗口工厂接口，用于创建主窗口实例。
    /// </summary>
    public interface IMainWindowFactory
    {
        /// <summary>
        /// 创建一个主窗口实例。
        /// </summary>
        /// <returns>返回创建的主窗口实例。</returns>
        IMainWindow CreateMainWindow();
    }
}
