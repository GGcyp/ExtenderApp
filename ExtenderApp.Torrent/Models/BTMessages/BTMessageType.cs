

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 表示BT协议中的消息类型。
    /// </summary>
    public enum BTMessageType : byte
    {
        /// <summary>
        /// 表示“拒绝”消息。
        /// </summary>
        Choke = 0,
        /// <summary>
        /// 表示“允许”消息。
        /// </summary>
        Unchoke = 1,
        /// <summary>
        /// 表示“感兴趣”消息。
        /// </summary>
        Interested = 2,
        /// <summary>
        /// 表示“不感兴趣”消息。
        /// </summary>
        NotInterested = 3,
        /// <summary>
        /// 表示“有”消息。
        /// </summary>
        Have = 4,
        /// <summary>
        /// 表示“位字段”消息。
        /// </summary>
        BitField = 5,
        /// <summary>
        /// 表示“请求”消息。
        /// </summary>
        Request = 6,
        /// <summary>
        /// 表示“片段”消息。
        /// </summary>
        Piece = 7,
        /// <summary>
        /// 表示“取消”消息。
        /// </summary>
        Cancel = 8,
        /// <summary>
        /// 表示“端口”消息。
        /// </summary>
        Port = 9,
        /// <summary>
        /// 表示“保持活跃”消息（空消息）。
        /// </summary>
        KeepAlive = 10,

    }
}
