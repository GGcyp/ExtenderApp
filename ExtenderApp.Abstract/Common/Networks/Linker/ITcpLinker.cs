

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// ITcpLinker 接口继承自 ILinker 接口，代表一个 TCP 链接器接口。
    /// </summary>
    public interface ITcpLinker : ILinker
    {
        /// <summary>
        /// 是否禁用 Nagle 算法（仅对 TCP 有效）。
        /// </summary>
        /// <remarks>
        /// - true：小包将尽快发送，降低延迟但可能增加包数量；<br/>
        /// - false：允许合并小包，降低包数量但可能增加延迟。
        /// </remarks>
        bool NoDelay { get; set; }
    }
}
