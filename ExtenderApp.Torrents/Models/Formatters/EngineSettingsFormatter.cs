using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;
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

        public override EngineSettingsBuilderModel Deserialize(ref ExtenderBinaryReader reader)
        {
            EngineSettingsBuilderModel builder = new();
            if (TryReadNil(ref reader))
            {
                return builder;
            }

            // AllowedEncryption
            int encCount = _int.Deserialize(ref reader);
            builder.AllowedEncryption = builder.AllowedEncryption ?? new(encCount);
            builder.AllowedEncryption.Clear();
            for (int i = 0; i < encCount; i++)
            {
                builder.AllowedEncryption.Add((EncryptionType)_int.Deserialize(ref reader));
            }

            // 基础bool类型（顺序必须与序列化一致）
            builder.AllowHaveSuppression = _bool.Deserialize(ref reader);
            builder.AllowLocalPeerDiscovery = _bool.Deserialize(ref reader);
            builder.AllowPortForwarding = _bool.Deserialize(ref reader);
            builder.AutoSaveLoadDhtCache = _bool.Deserialize(ref reader);
            builder.AutoSaveLoadFastResume = _bool.Deserialize(ref reader);
            builder.AutoSaveLoadMagnetLinkMetadata = _bool.Deserialize(ref reader);
            builder.UsePartialFiles = _bool.Deserialize(ref reader);

            // string类型
            builder.CacheDirectory = _string.Deserialize(ref reader);
            builder.HttpStreamingPrefix = _string.Deserialize(ref reader);

            // int类型
            builder.DiskCacheBytes = _int.Deserialize(ref reader);
            builder.MaximumConnections = _int.Deserialize(ref reader);
            builder.MaximumDownloadRate = _int.Deserialize(ref reader);
            builder.MaximumHalfOpenConnections = _int.Deserialize(ref reader);
            builder.MaximumUploadRate = _int.Deserialize(ref reader);
            builder.MaximumOpenFiles = _int.Deserialize(ref reader);
            builder.MaximumDiskReadRate = _int.Deserialize(ref reader);
            builder.MaximumDiskWriteRate = _int.Deserialize(ref reader);
            builder.WebSeedSpeedTrigger = _int.Deserialize(ref reader);

            // TimeSpan类型
            builder.ConnectionTimeout = _timeSpan.Deserialize(ref reader);
            builder.StaleRequestTimeout = _timeSpan.Deserialize(ref reader);
            builder.WebSeedConnectionTimeout = _timeSpan.Deserialize(ref reader);
            builder.WebSeedDelay = _timeSpan.Deserialize(ref reader);

            // 枚举类型
            builder.DiskCachePolicy = (CachePolicy)_int.Deserialize(ref reader);
            builder.FastResumeMode = (FastResumeMode)_int.Deserialize(ref reader);

            // IPEndPoint类型
            builder.DhtEndPoint = _ip.Deserialize(ref reader);

            // Dictionary类型
            builder.ListenEndPoints = _dictIPEndPoint.Deserialize(ref reader);
            builder.ReportedListenEndPoints = _dictIPEndPoint.Deserialize(ref reader);

            return builder;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, EngineSettingsBuilderModel value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            // AllowedEncryption
            _int.Serialize(ref writer, value.AllowedEncryption.Count);
            for (int i = 0; i < value.AllowedEncryption.Count; i++)
            {
                _int.Serialize(ref writer, (int)value.AllowedEncryption[i]);
            }

            // 基础bool类型
            _bool.Serialize(ref writer, value.AllowHaveSuppression);
            _bool.Serialize(ref writer, value.AllowLocalPeerDiscovery);
            _bool.Serialize(ref writer, value.AllowPortForwarding);
            _bool.Serialize(ref writer, value.AutoSaveLoadDhtCache);
            _bool.Serialize(ref writer, value.AutoSaveLoadFastResume);
            _bool.Serialize(ref writer, value.AutoSaveLoadMagnetLinkMetadata);
            _bool.Serialize(ref writer, value.UsePartialFiles);

            // string类型
            _string.Serialize(ref writer, value.CacheDirectory);
            _string.Serialize(ref writer, value.HttpStreamingPrefix);

            // int类型
            _int.Serialize(ref writer, value.DiskCacheBytes);
            _int.Serialize(ref writer, value.MaximumConnections);
            _int.Serialize(ref writer, value.MaximumDownloadRate);
            _int.Serialize(ref writer, value.MaximumHalfOpenConnections);
            _int.Serialize(ref writer, value.MaximumUploadRate);
            _int.Serialize(ref writer, value.MaximumOpenFiles);
            _int.Serialize(ref writer, value.MaximumDiskReadRate);
            _int.Serialize(ref writer, value.MaximumDiskWriteRate);
            _int.Serialize(ref writer, value.WebSeedSpeedTrigger);

            // TimeSpan类型
            _timeSpan.Serialize(ref writer, value.ConnectionTimeout);
            _timeSpan.Serialize(ref writer, value.StaleRequestTimeout);
            _timeSpan.Serialize(ref writer, value.WebSeedConnectionTimeout);
            _timeSpan.Serialize(ref writer, value.WebSeedDelay);

            // 枚举类型
            _int.Serialize(ref writer, (int)value.DiskCachePolicy);
            _int.Serialize(ref writer, (int)value.FastResumeMode);

            // IPEndPoint类型
            _ip.Serialize(ref writer, value.DhtEndPoint);

            // Dictionary类型
            _dictIPEndPoint.Serialize(ref writer, value.ListenEndPoints);
            _dictIPEndPoint.Serialize(ref writer, value.ReportedListenEndPoints);
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
