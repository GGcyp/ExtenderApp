using System.Collections.ObjectModel;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Torrents.Models.Formatters
{
    internal class TorrentInfoFormatter_1 : VersionDataFormatter<TorrentInfo>, IVersionDataFormatter<TorrentInfo>
    {
        protected readonly IBinaryFormatter<bool> _bool;
        protected readonly IBinaryFormatter<int> _int;
        protected readonly IBinaryFormatter<long> _long;
        protected readonly IBinaryFormatter<string> _string;
        protected readonly IBinaryFormatter<DateTime> _dateTime;
        protected readonly IBinaryFormatter<TimeSpan> _timeSpan;
        protected readonly IBinaryFormatter<double> _double;
        protected readonly IBinaryFormatter<ValueOrList<TorrentFileInfoNode>> _fileInfoNodeList;
        protected readonly IBinaryFormatter<TorrentPieceStateType> _pieceStateType;
        protected readonly IDispatcherService _dispatcherService;

        public override Version FormatterVersion { get; } = new Version(1, 0, 0, 0);

        public override int DefaultLength => 1;

        public TorrentInfoFormatter_1(IBinaryFormatterResolver resolver, IDispatcherService dispatcherService) : base(resolver)
        {
            _dispatcherService = dispatcherService;
            _bool = GetFormatter<bool>();
            _int = GetFormatter<int>();
            _long = GetFormatter<long>();
            _string = GetFormatter<string>();
            _dateTime = GetFormatter<DateTime>();
            _timeSpan = GetFormatter<TimeSpan>();
            _double = GetFormatter<double>();
            _fileInfoNodeList = GetFormatter<ValueOrList<TorrentFileInfoNode>>();
            _pieceStateType = GetFormatter<TorrentPieceStateType>();
        }

        public override TorrentInfo Deserialize(ref ExtenderBinaryReader reader)
        {
            TorrentInfo info = new TorrentInfo(_dispatcherService);
            if (TryReadNil(ref reader))
                return info;

            info.Name = _string.Deserialize(ref reader);
            info.Size = _long.Deserialize(ref reader);
            info.PieceLength = _int.Deserialize(ref reader);
            info.PieceCount = _int.Deserialize(ref reader);
            info.Progress = _double.Deserialize(ref reader);
            info.SelectedFileCount = _int.Deserialize(ref reader);
            info.SelectedFileLength = _long.Deserialize(ref reader);
            info.SelectedFileCompleteCount = _int.Deserialize(ref reader);
            info.SelectedFileCompleteLength = _long.Deserialize(ref reader);
            info.TorrentPath = _string.Deserialize(ref reader);
            info.SavePath = _string.Deserialize(ref reader);
            info.TorrentMagnetLink = _string.Deserialize(ref reader);
            info.CreateTime = _dateTime.Deserialize(ref reader);
            info.TorrentCreateTime = _dateTime.Deserialize(ref reader);
            info.CreatedBy = _string.Deserialize(ref reader);
            info.Comment = _string.Deserialize(ref reader);
            info.Encoding = _string.Deserialize(ref reader);
            info.RemainingTime = _timeSpan.Deserialize(ref reader);
            info.Files = _fileInfoNodeList.Deserialize(ref reader);
            info.FileCount = _int.Deserialize(ref reader);
            info.SelecrAll = _bool.Deserialize(ref reader);
            info.TrueCount = _int.Deserialize(ref reader);
            info.SelectedBitfieldCount = _int.Deserialize(ref reader);

            int pieceCount = _int.Deserialize(ref reader);
            ObservableCollection<TorrentPiece> pieces = new();
            info.Bitfield = pieces;
            for (int i = 0; i < pieceCount; i++)
            {
                pieces.Add(new TorrentPiece(_pieceStateType.Deserialize(ref reader)));
            }

            return info;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, TorrentInfo value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            _string.Serialize(ref writer, value.Name);
            _long.Serialize(ref writer, value.Size);
            _int.Serialize(ref writer, value.PieceLength);
            _int.Serialize(ref writer, value.PieceCount);
            _double.Serialize(ref writer, value.Progress);
            _int.Serialize(ref writer, value.SelectedFileCount);
            _long.Serialize(ref writer, value.SelectedFileLength);
            _int.Serialize(ref writer, value.SelectedFileCompleteCount);
            _long.Serialize(ref writer, value.SelectedFileCompleteLength);
            _string.Serialize(ref writer, value.TorrentPath);
            _string.Serialize(ref writer, value.SavePath);
            _string.Serialize(ref writer, value.TorrentMagnetLink);
            _dateTime.Serialize(ref writer, value.CreateTime);
            _dateTime.Serialize(ref writer, value.TorrentCreateTime);
            _string.Serialize(ref writer, value.CreatedBy);
            _string.Serialize(ref writer, value.Comment);
            _string.Serialize(ref writer, value.Encoding);
            _timeSpan.Serialize(ref writer, value.RemainingTime);
            _fileInfoNodeList.Serialize(ref writer, value.Files);
            _int.Serialize(ref writer, value.FileCount);
            _bool.Serialize(ref writer, value.SelecrAll);
            _int.Serialize(ref writer, value.TrueCount);
            _int.Serialize(ref writer, value.SelectedBitfieldCount);
            _int.Serialize(ref writer, value.Bitfield.Count);
            for (int i = 0; i < value.Bitfield.Count; i++)
            {
                _pieceStateType.Serialize(ref writer, value.Bitfield[i].State);
            }
        }
    }
}
