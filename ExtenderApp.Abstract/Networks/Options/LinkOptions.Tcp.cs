using ExtenderApp.Abstract.Options;

namespace ExtenderApp.Abstract.Networks
{
    /// <summary>
    /// TCP 链接相关的选项标识符集合，用于在设置 TCP 链接选项时提供类型安全的标识符。
    /// </summary>
    public static partial class LinkOptions
    {
        /// <summary>
        /// 设置 TCP 链接的 NoDelay 选项，启用或禁用 Nagle 算法。
        /// </summary>
        public static readonly OptionIdentifier<bool> NoDelayIdentifier = new(nameof(ITcpLink.NoDelay), false);
    }
}