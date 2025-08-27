

namespace ExtenderApp.Torrents.Models
{
    /// <summary>
    /// 表示 BT（BitTorrent）协议中单个数据块（Piece）的下载状态枚举
    /// </summary>
    /// <remarks>
    /// 该枚举覆盖数据块从“初始未下载”到“最终完成”的全生命周期，
    /// 用于 <see cref="PieceManager"/> 等组件跟踪数据块状态，
    /// 并作为 UI 可视化（如区块进度图）和下载策略调度的核心依据。
    /// 枚举类型声明为 byte，仅占用 1 字节内存，适合大规模存储（单个种子可能包含数千个数据块）。
    /// </remarks>
    public enum TorrentPieceStateType : byte
    {
        /// <summary>
        /// 未下载
        /// </summary>
        DontDownloaded,
        /// <summary>
        /// 待下载
        /// </summary>
        ToBeDownloaded,
        /// <summary>
        /// 正在下载
        /// </summary>
        Downloading,
        /// <summary>
        /// 已完成
        /// </summary>
        Complete
    }
}
