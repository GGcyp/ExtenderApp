using ExtenderApp.Abstract;

namespace ExtenderApp.Common
{
    /// <summary>
    /// LinkerFactory 扩展类
    /// </summary>
    public static class LinkerFactoryExtensions
    {
        /// <summary>
        /// 从 LinkerFactory 中获取 ITcpLinker 实例
        /// </summary>
        /// <param name="factory">LinkerFactory 实例</param>
        /// <returns>返回 ITcpLinker 实例</returns>
        public static ITcpLinker GetTcpLinker(this ILinkerFactory factory)
        {
            return factory.GetLinker<ITcpLinker>();
        }
    }
}
