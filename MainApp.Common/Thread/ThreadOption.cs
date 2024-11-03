namespace MainApp.Common
{
    /// <summary>
    /// 线程选择
    /// </summary>
    public enum ThreadOption
    {
        /// <summary>
        /// 无选择线程
        /// </summary>
        None,

        /// <summary>
        /// 当前线程
        /// </summary>
        PublisherThread,

        /// <summary>
        /// 在UI线程
        /// </summary>
        UIThread,

        /// <summary>
        /// 网络线程
        /// </summary>
        NetworkThread,

        /// <summary>
        /// 创建一个新线程
        /// </summary>
        BackgroundThread
    }
}
