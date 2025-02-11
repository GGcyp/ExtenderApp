

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义一个流操作接口
    /// </summary>
    public interface IStreamOperation : IResettable
    {
        /// <summary>
        /// 执行流操作
        /// </summary>
        /// <param name="stream">待操作的流</param>
        void Execute(Stream stream);
    }
}
