using System.Net;
using ExtenderApp.Models;
using MonoTorrent.Client;
using MonoTorrent.Connections;
using MonoTorrent.PieceWriter;

namespace ExtenderApp.Torrents.Models
{
    /// <summary>
    /// 用于配置 <see cref="EngineSettings"/> 引擎参数的数据模型。
    /// </summary>
    public class EngineSettingsBuilderModel : DataModel
    {
        private readonly EngineSettingsBuilder _builder;

        /// <summary>
        /// 加密方法的优先级列表，包括纯文本和加密方式。
        /// 连接时会按此顺序尝试，默认包含 RC4Header、RC4Full 和 PlainText。
        /// </summary>
        public List<EncryptionType> AllowedEncryption
        {
            get => _builder.AllowedEncryption;
            set => _builder.AllowedEncryption = value;
        }

        /// <summary>
        /// 是否启用 Have 抑制，仅向未拥有该分块的节点发送 Have 消息。
        /// 可减少网络流量，默认 false。
        /// </summary>
        public bool AllowHaveSuppression
        {
            get => _builder.AllowHaveSuppression;
            set => _builder.AllowHaveSuppression = value;
        }

        /// <summary>
        /// 是否启用本地节点发现（LocalPeerDiscovery），用于发现局域网内的节点。
        /// 默认 true。
        /// </summary>
        public bool AllowLocalPeerDiscovery
        {
            get => _builder.AllowLocalPeerDiscovery;
            set => _builder.AllowLocalPeerDiscovery = value;
        }

        /// <summary>
        /// 是否自动端口转发（UPnP/NAT-PMP），用于自动映射端口。
        /// 默认 true。
        /// </summary>
        public bool AllowPortForwarding
        {
            get => _builder.AllowPortForwarding;
            set => _builder.AllowPortForwarding = value;
        }

        /// <summary>
        /// 是否自动保存/加载 DHT 节点缓存。
        /// 启用后可加快 DHT 启动速度，默认 true。
        /// </summary>
        public bool AutoSaveLoadDhtCache
        {
            get => _builder.AutoSaveLoadDhtCache;
            set => _builder.AutoSaveLoadDhtCache = value;
        }

        /// <summary>
        /// 是否自动保存/加载 FastResume 数据。
        /// 启用后停止任务时自动保存断点，下次启动自动加载，默认 true。
        /// </summary>
        public bool AutoSaveLoadFastResume
        {
            get => _builder.AutoSaveLoadFastResume;
            set => _builder.AutoSaveLoadFastResume = value;
        }

        /// <summary>
        /// 是否自动保存/加载磁力链接元数据。
        /// 启用后磁力任务元数据会缓存到目录，下次可快速启动，默认 true。
        /// </summary>
        public bool AutoSaveLoadMagnetLinkMetadata
        {
            get => _builder.AutoSaveLoadMagnetLinkMetadata;
            set => _builder.AutoSaveLoadMagnetLinkMetadata = value;
        }

        /// <summary>
        /// 引擎缓存目录，存储 DHT 表、磁力元数据、FastResume 数据等。
        /// 若为空则使用当前工作目录。
        /// </summary>
        public string CacheDirectory
        {
            get => _builder.CacheDirectory;
            set => _builder.CacheDirectory = value;
        }

        /// <summary>
        /// 连接超时时间，连接未完成则自动取消并尝试下一个节点。
        /// 推荐 7-15 秒，默认 10 秒。
        /// </summary>
        public TimeSpan ConnectionTimeout
        {
            get => _builder.ConnectionTimeout;
            set => _builder.ConnectionTimeout = value;
        }

        /// <summary>
        /// 磁盘缓存大小（字节），用于读写缓冲，0 表示禁用，默认 5MB。
        /// </summary>
        public int DiskCacheBytes
        {
            get => _builder.DiskCacheBytes;
            set => _builder.DiskCacheBytes = value;
        }

        /// <summary>
        /// 磁盘缓存策略，决定读写缓存方式。
        /// </summary>
        public CachePolicy DiskCachePolicy
        {
            get => _builder.DiskCachePolicy;
            set => _builder.DiskCachePolicy = value;
        }

        /// <summary>
        /// DHT 使用的 UDP 端口，0 表示随机端口，-1 禁用 DHT。
        /// </summary>
        public IPEndPoint? DhtEndPoint
        {
            get => _builder.DhtEndPoint;
            set => _builder.DhtEndPoint = value;
        }

        /// <summary>
        /// FastResume 模式，决定断点数据的准确性与启动速度。
        /// Accurate 更准确但可能需重新校验，BestEffort 启动更快但可能重下少量数据。
        /// </summary>
        public FastResumeMode FastResumeMode
        {
            get => _builder.FastResumeMode;
            set => _builder.FastResumeMode = value;
        }

        /// <summary>
        /// HTTP/HTTPS 流媒体前缀，设置流媒体服务绑定地址，默认 http://127.0.0.1:5555。
        /// </summary>
        public string HttpStreamingPrefix
        {
            get => _builder.HttpStreamingPrefix;
            set => _builder.HttpStreamingPrefix = value;
        }

        /// <summary>
        /// TCP 监听端口集合，0 表示随机端口，-1 禁用监听。
        /// </summary>
        public Dictionary<string, IPEndPoint> ListenEndPoints
        {
            get => _builder.ListenEndPoints;
            set => _builder.ListenEndPoints = value;
        }

        /// <summary>
        /// 最大同时连接数，默认 150。
        /// </summary>
        public int MaximumConnections
        {
            get => _builder.MaximumConnections;
            set => _builder.MaximumConnections = value;
        }

        /// <summary>
        /// 最大下载速率（字节/秒），0 表示不限速，默认 0。
        /// </summary>
        public int MaximumDownloadRate
        {
            get => _builder.MaximumDownloadRate;
            set => _builder.MaximumDownloadRate = value;
        }

        /// <summary>
        /// 最大同时连接尝试数，默认 8。
        /// </summary>
        public int MaximumHalfOpenConnections
        {
            get => _builder.MaximumHalfOpenConnections;
            set => _builder.MaximumHalfOpenConnections = value;
        }

        /// <summary>
        /// 最大上传速率（字节/秒），0 表示不限速，默认 0。
        /// </summary>
        public int MaximumUploadRate
        {
            get => _builder.MaximumUploadRate;
            set => _builder.MaximumUploadRate = value;
        }

        /// <summary>
        /// 最大同时打开文件数，0 表示不限，默认 20。
        /// </summary>
        public int MaximumOpenFiles
        {
            get => _builder.MaximumOpenFiles;
            set => _builder.MaximumOpenFiles = value;
        }

        /// <summary>
        /// 最大磁盘读取速率（字节/秒），0 表示不限速，默认 0。
        /// </summary>
        public int MaximumDiskReadRate
        {
            get => _builder.MaximumDiskReadRate;
            set => _builder.MaximumDiskReadRate = value;
        }

        /// <summary>
        /// 最大磁盘写入速率（字节/秒），0 表示不限速，默认 0。
        /// </summary>
        public int MaximumDiskWriteRate
        {
            get => _builder.MaximumDiskWriteRate;
            set => _builder.MaximumDiskWriteRate = value;
        }

        /// <summary>
        /// Tracker 上报的监听端点集合，通常无需设置，默认 null。
        /// </summary>
        public Dictionary<string, IPEndPoint> ReportedListenEndPoints
        {
            get => _builder.ReportedListenEndPoints;
            set => _builder.ReportedListenEndPoints = value;
        }

        /// <summary>
        /// 分块请求超时时间，超时后关闭连接并取消请求，默认 40 秒。
        /// 必须大于 WebSeedConnectionTimeout。
        /// </summary>
        public TimeSpan StaleRequestTimeout
        {
            get => _builder.StaleRequestTimeout;
            set => _builder.StaleRequestTimeout = value;
        }

        /// <summary>
        /// 是否启用部分文件下载，未完成文件会加 .!mt 后缀，完成后移除。
        /// 默认 false。
        /// </summary>
        public bool UsePartialFiles
        {
            get => _builder.UsePartialFiles;
            set => _builder.UsePartialFiles = value;
        }

        /// <summary>
        /// WebSeed 连接超时时间。
        /// </summary>
        public TimeSpan WebSeedConnectionTimeout
        {
            get => _builder.WebSeedConnectionTimeout;
            set => _builder.WebSeedConnectionTimeout = value;
        }

        /// <summary>
        /// WebSeed 启动延迟，0 表示立即启用。
        /// </summary>
        public TimeSpan WebSeedDelay
        {
            get => _builder.WebSeedDelay;
            set => _builder.WebSeedDelay = value;
        }

        /// <summary>
        /// WebSeed 速度触发阈值，低于此速度时启用 WebSeed，0 表示总是启用。
        /// </summary>
        public int WebSeedSpeedTrigger
        {
            get => _builder.WebSeedSpeedTrigger;
            set => _builder.WebSeedSpeedTrigger = value;
        }

        /// <summary>
        /// 初始化 EngineSettingsBuilderModel 实例。
        /// </summary>
        public EngineSettingsBuilderModel()
        {
            _builder = new();

            UsePartialFiles = true;
            AllowPortForwarding = true;
            AutoSaveLoadDhtCache = true;
            AutoSaveLoadFastResume = true;
            AutoSaveLoadMagnetLinkMetadata = true;
            ListenEndPoints = ListenEndPoints ?? new();
            DhtEndPoint = DhtEndPoint ?? new(IPAddress.Any, 55123);
        }

        /// <summary>
        /// 将当前模型中的所有设置参数转换为 <see cref="EngineSettings"/> 实例。
        /// 用于创建 MonoTorrent 引擎所需的完整配置对象。
        /// </summary>
        /// <returns>返回包含所有参数的 EngineSettings 实例</returns>
        public EngineSettings ToSettings()
        {
            return _builder.ToSettings();
        }

        /// <summary>
        /// 检查指定的 <see cref="EngineSettings"/> 实例与当前模型参数是否完全一致。
        /// 逐项比较所有配置项，包括加密方式列表，确保参数一致性。
        /// </summary>
        /// <param name="engineSettings">待比较的 EngineSettings 实例。</param>
        /// <returns>如果所有参数均一致则返回 true，否则返回false</returns>
        public bool IsSettingsEqual(EngineSettings engineSettings)
        {
            bool isSame = engineSettings.AutoSaveLoadMagnetLinkMetadata == AutoSaveLoadMagnetLinkMetadata &&
                     engineSettings.AutoSaveLoadDhtCache == AutoSaveLoadDhtCache &&
                     engineSettings.AutoSaveLoadFastResume == AutoSaveLoadFastResume &&
                     engineSettings.CacheDirectory == CacheDirectory &&
                     engineSettings.ConnectionTimeout == ConnectionTimeout &&
                     engineSettings.DiskCacheBytes == DiskCacheBytes &&
                     engineSettings.DiskCachePolicy == DiskCachePolicy &&
                     engineSettings.DhtEndPoint == DhtEndPoint &&
                     engineSettings.FastResumeMode == FastResumeMode &&
                     engineSettings.HttpStreamingPrefix == HttpStreamingPrefix &&
                     engineSettings.MaximumConnections == MaximumConnections &&
                     engineSettings.MaximumDownloadRate == MaximumDownloadRate &&
                     engineSettings.MaximumHalfOpenConnections == MaximumHalfOpenConnections &&
                     engineSettings.MaximumOpenFiles == MaximumOpenFiles &&
                     engineSettings.MaximumUploadRate == MaximumUploadRate &&
                     engineSettings.MaximumDiskReadRate == MaximumDiskReadRate &&
                     engineSettings.MaximumDiskWriteRate == MaximumDiskWriteRate &&
                     engineSettings.StaleRequestTimeout == StaleRequestTimeout &&
                     engineSettings.UsePartialFiles == UsePartialFiles &&
                     engineSettings.WebSeedConnectionTimeout == WebSeedConnectionTimeout &&
                     engineSettings.WebSeedDelay == WebSeedDelay &&
                     engineSettings.WebSeedSpeedTrigger == WebSeedSpeedTrigger;

            if (isSame == false)
                return false;

            for (int i = 0; i < AllowedEncryption.Count; i++)
            {
                if (AllowedEncryption[i] != engineSettings.AllowedEncryption[i])
                {
                    isSame = false;
                    break;
                }
            }
            return isSame;
        }

        /// <summary>
        /// 将当前模型的所有设置参数深拷贝到目标 <see cref="EngineSettingsBuilderModel"/> 实例。
        /// </summary>
        /// <param name="model">目标 EngineSettingsBuilderModel 实例</param>
        public void CopyTo(EngineSettingsBuilderModel model)
        {
            // 列表和字典类型需深拷贝
            model.AllowedEncryption.Clear();
            for (int i = 0; i < AllowedEncryption.Count; i++)
            {
                model.AllowedEncryption.Add(AllowedEncryption[i]);
            }
            model.ListenEndPoints.Clear();
            foreach (var item in ListenEndPoints)
            {
                model.ListenEndPoints.Add(item.Key, item.Value);
            }
            model.ReportedListenEndPoints.Clear();
            foreach (var item in ReportedListenEndPoints)
            {
                model.ReportedListenEndPoints.Add(item.Key, item.Value);
            }

            // 基本类型和结构体直接赋值
            model.MaximumConnections = MaximumConnections;
            model.MaximumDownloadRate = MaximumDownloadRate;
            model.MaximumHalfOpenConnections = MaximumHalfOpenConnections;
            model.MaximumOpenFiles = MaximumOpenFiles;
            model.MaximumUploadRate = MaximumUploadRate;
            model.MaximumDiskReadRate = MaximumDiskReadRate;
            model.MaximumDiskWriteRate = MaximumDiskWriteRate;
            model.AllowHaveSuppression = AllowHaveSuppression;
            model.AllowLocalPeerDiscovery = AllowLocalPeerDiscovery;
            model.AllowPortForwarding = AllowPortForwarding;
            model.AutoSaveLoadDhtCache = AutoSaveLoadDhtCache;
            model.AutoSaveLoadFastResume = AutoSaveLoadFastResume;
            model.AutoSaveLoadMagnetLinkMetadata = AutoSaveLoadMagnetLinkMetadata;
            model.CacheDirectory = CacheDirectory;
            model.ConnectionTimeout = ConnectionTimeout;
            model.DiskCacheBytes = DiskCacheBytes;
            model.DiskCachePolicy = DiskCachePolicy;
            model.DhtEndPoint = DhtEndPoint == null ? null : new IPEndPoint(DhtEndPoint.Address, DhtEndPoint.Port);
            model.FastResumeMode = FastResumeMode;
            model.HttpStreamingPrefix = HttpStreamingPrefix;
            model.StaleRequestTimeout = StaleRequestTimeout;
            model.UsePartialFiles = UsePartialFiles;
            model.WebSeedConnectionTimeout = WebSeedConnectionTimeout;
            model.WebSeedDelay = WebSeedDelay;
            model.WebSeedSpeedTrigger = WebSeedSpeedTrigger;
        }
    }
}
