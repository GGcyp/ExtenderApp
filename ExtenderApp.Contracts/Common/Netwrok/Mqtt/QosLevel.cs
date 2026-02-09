

namespace ExtenderApp.Contracts
{
    public enum QosLevel : byte
    {
        /// <summary>
        /// 最多一次。消息发布完全依赖底层网络的能力。消息可能到达一次也可能根本没到达。
        /// </summary>
        AtMostOnce = 1,

        /// <summary>
        /// 至少一次。确保消息至少到达一次，但消息可能会重复。
        /// </summary>
        AtLeastOnce = 1 << 1,

        /// <summary>
        /// 只有一次。确保消息到达一次且仅到达一次。
        /// </summary>
        ExactlyOnce = 1 << 2
    }
}
