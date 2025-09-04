using ExtenderApp.Models;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents.Models
{
    /// <summary>
    /// 用于配置单个种子任务的高级参数设置的数据模型。
    /// </summary>
    public class TorrentSettingsBuilderModel : DataModel
    {
        /// <summary>
        /// MonoTorrent 提供的种子设置构建器实例。
        /// </summary>
        private readonly TorrentSettingsBuilder _builder;

        /// <summary>
        /// 是否允许使用 DHT 网络查找更多的节点（去中心化发现更多下载源）。
        /// 默认值为 true。
        /// </summary>
        public bool AllowDht
        {
            get => _builder.AllowDht;
            set => _builder.AllowDht = value;
        }

        /// <summary>
        /// 是否允许“初始做种”模式。
        /// 当没有其他做种者时，优先分享稀有分块，有助于资源首发。
        /// </summary>
        public bool AllowInitialSeeding
        {
            get => _builder.AllowInitialSeeding;
            set => _builder.AllowInitialSeeding = value;
        }

        /// <summary>
        /// 是否允许使用 Peer Exchange（PEX）协议与其他节点交换更多下载源信息。
        /// 默认值为 true。
        /// </summary>
        public bool AllowPeerExchange
        {
            get => _builder.AllowPeerExchange;
            set => _builder.AllowPeerExchange = value;
        }

        /// <summary>
        /// 多文件种子下载时，是否自动在保存目录下创建一个以种子名为名的文件夹。
        /// 默认值为 true。
        /// </summary>
        public bool CreateContainingDirectory
        {
            get => _builder.CreateContainingDirectory;
            set => _builder.CreateContainingDirectory = value;
        }

        /// <summary>
        /// 单个种子允许的最大同时连接数。
        /// 默认值为 60。
        /// </summary>
        public int MaximumConnections
        {
            get => _builder.MaximumConnections;
            set => _builder.MaximumConnections = value;
        }

        /// <summary>
        /// 单个种子的最大下载速度（字节/秒），0 表示不限速。
        /// 默认值为 0。
        /// </summary>
        public int MaximumDownloadRate
        {
            get => _builder.MaximumDownloadRate;
            set => _builder.MaximumDownloadRate = value;
        }

        /// <summary>
        /// 单个种子的最大上传速度（字节/秒），0 表示不限速。
        /// 默认值为 0。
        /// </summary>
        public int MaximumUploadRate
        {
            get => _builder.MaximumUploadRate;
            set => _builder.MaximumUploadRate = value;
        }

        /// <summary>
        /// 是否要求远程节点的 peer_id 必须和 tracker 上报告的一致。
        /// 默认值为 false，通常无需开启。
        /// </summary>
        public bool RequirePeerIdToMatch
        {
            get => _builder.RequirePeerIdToMatch;
            set => _builder.RequirePeerIdToMatch = value;
        }

        /// <summary>
        /// 单个种子允许同时上传的节点数，0 表示不限。
        /// 默认值为 8。
        /// </summary>
        public int UploadSlots
        {
            get => _builder.UploadSlots;
            set => _builder.UploadSlots = value;
        }

        /// <summary>
        /// 初始化 TorrentSettingsBuilderModel 实例。
        /// </summary>
        public TorrentSettingsBuilderModel()
        {
            _builder = new TorrentSettingsBuilder();
        }

        /// <summary>
        /// 生成MonoTorrent使用的种子设置对象。
        /// </summary>
        /// <returns>MonoTorrent种子设置对象</returns>
        public TorrentSettings ToSettings()
        {
            return _builder.ToSettings();
        }

        /// <summary>
        /// 检查当前TorrentSettings是否与Builder中的设置相同。
        /// </summary>
        /// <param name="settings">需要检查的Settings</param>
        /// <returns>如果相同则返回true,否则返回false</returns>
        public bool IsSettingsEqual(TorrentSettings settings)
        {
            return AllowDht == settings.AllowDht &&
                          AllowInitialSeeding == settings.AllowInitialSeeding &&
                          AllowPeerExchange == settings.AllowPeerExchange &&
                          CreateContainingDirectory == settings.CreateContainingDirectory &&
                          MaximumConnections == settings.MaximumConnections &&
                          MaximumDownloadRate == settings.MaximumDownloadRate &&
                          MaximumUploadRate == settings.MaximumUploadRate &&
                          RequirePeerIdToMatch == settings.RequirePeerIdToMatch &&
                          UploadSlots == settings.UploadSlots;
        }

        /// <summary>
        /// 复制到指定对象
        /// </summary>
        /// <param name="model">指定对象</param>
        public void CopyTo(TorrentSettingsBuilderModel model)
        {
            model.CreateContainingDirectory = CreateContainingDirectory;
            model.AllowDht = AllowDht;
            model.AllowPeerExchange = AllowPeerExchange;
            model.UploadSlots = UploadSlots;
            model.MaximumConnections = MaximumConnections;
            model.MaximumDownloadRate = MaximumDownloadRate;
            model.MaximumUploadRate = MaximumUploadRate;
            model.RequirePeerIdToMatch = RequirePeerIdToMatch;
            model.UploadSlots = UploadSlots;
        }
    }
}
