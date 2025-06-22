

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示链接状态的枚举类型。
    /// </summary>
    public enum LinkState : byte
    {
        /// <summary>
        /// 请求尚未发送。
        /// </summary>
        Unknown,
        /// <summary>
        /// 正在发送请求。
        /// </summary>
        Connecting,
        /// <summary>
        /// 最近的请求成功完成。
        /// </summary>
        Ok,
        /// <summary>
        /// 无法访问/离线。
        /// </summary>
        Offline,
        /// <summary>
        /// 可以访问，但它发送的响应无效。
        /// </summary>
        InvalidResponse
    }
}
