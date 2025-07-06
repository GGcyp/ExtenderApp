using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    public struct TrackerRequest
    {
        /// <summary>
        /// 种子文件的哈希值，经过URL编码处理
        /// </summary>
        public InfoHash Hash { get; set; }

        /// <summary>
        /// 客户端的唯一标识符
        /// </summary>
        public PeerId Id { get; set; }

        /// <summary>
        /// 获取或设置连接ID。
        /// </summary>
        public long ConnectionId { get; set; }

        /// <summary>
        /// 客户端监听的端口号
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// 已上传的数据量（字节）
        /// </summary>
        public long Uploaded { get; set; }

        /// <summary>
        /// 已下载的数据量（字节）
        /// </summary>
        public long Downloaded { get; set; }

        /// <summary>
        /// 还需要下载的数据量（字节）
        /// </summary>
        public long Left { get; set; }

        /// <summary>
        /// 是否使用紧凑格式返回peer信息 (1=是, 0=否)
        /// </summary>
        public string Compact { get; set; }

        /// <summary>
        /// 是否不返回peer_id (1=是, 0=否)
        /// </summary>
        public string NoPeerId { get; set; }

        /// <summary>
        /// 当前事件类型（started, completed, stopped, empty）
        /// </summary>
        public byte Event { get; set; }

        /// <summary>
        /// 获取或设置事务ID。
        /// </summary>
        public int TransactionId { get; set; }
    }
}
