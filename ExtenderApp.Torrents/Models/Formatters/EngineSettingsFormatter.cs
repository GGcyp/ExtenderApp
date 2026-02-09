using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Serializations.Binary.Formatters;
using ExtenderApp.Contracts;
using MonoTorrent.Client;
using MonoTorrent.Connections;
using MonoTorrent.PieceWriter;

namespace ExtenderApp.Torrents.Models
{
    internal class EngineSettingsFormatter : ResolverFormatter<EngineSettingsBuilderModel>
    {
        public override int DefaultLength => 1;

        private readonly IBinaryFormatter<int> _int;
        private readonly IBinaryFormatter<bool> _bool;
        private readonly IBinaryFormatter<string> _string;
        private readonly IBinaryFormatter<TimeSpan> _timeSpan;
        private readonly IBinaryFormatter<IPEndPoint> _ip;
        private readonly IBinaryFormatter<Dictionary<string, IPEndPoint>> _dictIPEndPoint;

        public EngineSettingsFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = resolver.GetFormatter<int>();
            _bool = resolver.GetFormatter<bool>();
            _string = resolver.GetFormatter<string>();
            _timeSpan = resolver.GetFormatter<TimeSpan>();
            _ip = resolver.GetFormatter<IPEndPoint>();
            _dictIPEndPoint = resolver.GetFormatter<Dictionary<string, IPEndPoint>>();
        }

        public override EngineSettingsBuilderModel Deserialize(ref ByteBuffer buffer)
        {
            EngineSettingsBuilderModel builder = new();
            if (TryReadNil(ref buffer))
            {
                return builder;
            }

            // AllowedEncryption
            int encCount = _int.Deserialize(ref buffer);
            builder.AllowedEncryption = builder.AllowedEncryption ?? new(encCount);
            builder.AllowedEncryption.Clear();
            for (int i = 0; i < encCount; i++)
            {
                builder.AllowedEncryption.Add((EncryptionType)_int.Deserialize(ref buffer));
            }

            // 基础bool类型（顺序必须与序列化一致）
            builder.AllowHaveSuppression = _bool.Deserialize(ref buffer);
            builder.AllowLocalPeerDiscovery = _bool.Deserialize(ref buffer);
            builder.AllowPortForwarding = _bool.Deserialize(ref buffer);
            builder.AutoSaveLoadDhtCache = _bool.Deserialize(ref buffer);
            builder.AutoSaveLoadFastResume = _bool.Deserialize(ref buffer);
            builder.AutoSaveLoadMagnetLinkMetadata = _bool.Deserialize(ref buffer);
            builder.UsePartialFiles = _bool.Deserialize(ref buffer);

            // string类型
            builder.CacheDirectory = _string.Deserialize(ref buffer);
            builder.HttpStreamingPrefix = _string.Deserialize(ref buffer);

            // int类型
            builder.DiskCacheBytes = _int.Deserialize(ref buffer);
            builder.MaximumConnections = _int.Deserialize(ref buffer);
            builder.MaximumDownloadRate = _int.Deserialize(ref buffer);
            builder.MaximumHalfOpenConnections = _int.Deserialize(ref buffer);
            builder.MaximumUploadRate = _int.Deserialize(ref buffer);
            builder.MaximumOpenFiles = _int.Deserialize(ref buffer);
            builder.MaximumDiskReadRate = _int.Deserialize(ref buffer);
            builder.MaximumDiskWriteRate = _int.Deserialize(ref buffer);
            builder.WebSeedSpeedTrigger = _int.Deserialize(ref buffer);

            // TimeSpan类型
            builder.ConnectionTimeout = _timeSpan.Deserialize(ref buffer);
            builder.StaleRequestTimeout = _timeSpan.Deserialize(ref buffer);
            builder.WebSeedConnectionTimeout = _timeSpan.Deserialize(ref buffer);
            builder.WebSeedDelay = _timeSpan.Deserialize(ref buffer);

            // 枚举类型
            builder.DiskCachePolicy = (CachePolicy)_int.Deserialize(ref buffer);
            builder.FastResumeMode = (FastResumeMode)_int.Deserialize(ref buffer);

            // IPEndPoint类型
            builder.DhtEndPoint = _ip.Deserialize(ref buffer);

            // Dictionary类型
            builder.ListenEndPoints = _dictIPEndPoint.Deserialize(ref buffer);
            builder.ReportedListenEndPoints = _dictIPEndPoint.Deserialize(ref buffer);

            return builder;
        }

        public override void Serialize(ref ByteBuffer buffer, EngineSettingsBuilderModel value)
        {
            if (value == null)
            {
                WriteNil(ref buffer);
                return;
            }

            // AllowedEncryption
            _int.Serialize(ref buffer, value.AllowedEncryption.Count);
            for (int i = 0; i < value.AllowedEncryption.Count; i++)
            {
                _int.Serialize(ref buffer, (int)value.AllowedEncryption[i]);
            }

            // 基础bool类型
            _bool.Serialize(ref buffer, value.AllowHaveSuppression);
            _bool.Serialize(ref buffer, value.AllowLocalPeerDiscovery);
            _bool.Serialize(ref buffer, value.AllowPortForwarding);
            _bool.Serialize(ref buffer, value.AutoSaveLoadDhtCache);
            _bool.Serialize(ref buffer, value.AutoSaveLoadFastResume);
            _bool.Serialize(ref buffer, value.AutoSaveLoadMagnetLinkMetadata);
            _bool.Serialize(ref buffer, value.UsePartialFiles);

            // string类型
            _string.Serialize(ref buffer, value.CacheDirectory);
            _string.Serialize(ref buffer, value.HttpStreamingPrefix);

            // int类型
            _int.Serialize(ref buffer, value.DiskCacheBytes);
            _int.Serialize(ref buffer, value.MaximumConnections);
            _int.Serialize(ref buffer, value.MaximumDownloadRate);
            _int.Serialize(ref buffer, value.MaximumHalfOpenConnections);
            _int.Serialize(ref buffer, value.MaximumUploadRate);
            _int.Serialize(ref buffer, value.MaximumOpenFiles);
            _int.Serialize(ref buffer, value.MaximumDiskReadRate);
            _int.Serialize(ref buffer, value.MaximumDiskWriteRate);
            _int.Serialize(ref buffer, value.WebSeedSpeedTrigger);

            // TimeSpan类型
            _timeSpan.Serialize(ref buffer, value.ConnectionTimeout);
            _timeSpan.Serialize(ref buffer, value.StaleRequestTimeout);
            _timeSpan.Serialize(ref buffer, value.WebSeedConnectionTimeout);
            _timeSpan.Serialize(ref buffer, value.WebSeedDelay);

            // 枚举类型
            _int.Serialize(ref buffer, (int)value.DiskCachePolicy);
            _int.Serialize(ref buffer, (int)value.FastResumeMode);

            // IPEndPoint类型
            _ip.Serialize(ref buffer, value.DhtEndPoint);

            // Dictionary类型
            _dictIPEndPoint.Serialize(ref buffer, value.ListenEndPoints);
            _dictIPEndPoint.Serialize(ref buffer, value.ReportedListenEndPoints);
        }

        public override long GetLength(EngineSettingsBuilderModel value)
        {
            if (value == null)
            {
                return 1;
            }

            return _bool.DefaultLength * 7 + _int.DefaultLength * 13 + _string.GetLength(value.CacheDirectory) + _string.GetLength(value.HttpStreamingPrefix) +
                _timeSpan.DefaultLength * 4 + _ip.DefaultLength + _dictIPEndPoint.GetLength(value.ListenEndPoints) +
                _dictIPEndPoint.GetLength(value.ReportedListenEndPoints) +
                (value.AllowedEncryption?.Count ?? 0) * _int.DefaultLength + _int.DefaultLength;
        }
    }
}
