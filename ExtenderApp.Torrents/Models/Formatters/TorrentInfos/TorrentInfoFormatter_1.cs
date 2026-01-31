using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Serializations.Binary.Formatters;
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
        protected readonly IBinaryFormatter<byte> _byte;
        protected readonly IBinaryFormatter<HashValue> _hashValue;
        protected readonly IBinaryFormatter<ValueOrList<TorrentFileInfoNode>> _fileInfoNodeList;
        protected readonly IBinaryFormatter<Dictionary<int, string>> _intStringDict;

        protected readonly IDispatcherService _dispatcherService;

        public override Version FormatterVersion { get; } = TorrentFormatterVersion.Version_1;

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
            _byte = GetFormatter<byte>();
            _hashValue = GetFormatter<HashValue>();
            _intStringDict = GetFormatter<Dictionary<int, string>>();
            _fileInfoNodeList = GetFormatter<ValueOrList<TorrentFileInfoNode>>();
        }

        public override TorrentInfo Deserialize(ref ByteBuffer buffer)
        {
            TorrentInfo info = new TorrentInfo(_dispatcherService);
            if (TryReadNil(ref buffer))
                return info;

            info.Name = _string.Deserialize(ref buffer);
            info.Size = _long.Deserialize(ref buffer);
            info.PieceLength = _int.Deserialize(ref buffer);
            info.PieceCount = _int.Deserialize(ref buffer);
            info.Progress = _double.Deserialize(ref buffer);
            info.SelectedFileCount = _int.Deserialize(ref buffer);
            info.SelectedFileLength = _long.Deserialize(ref buffer);
            info.SelectedFileCompleteCount = _int.Deserialize(ref buffer);
            info.SelectedFileCompleteLength = _long.Deserialize(ref buffer);
            info.TorrentPath = _string.Deserialize(ref buffer);
            info.SavePath = _string.Deserialize(ref buffer);
            info.TorrentMagnetLink = _string.Deserialize(ref buffer);
            info.CreateTime = _dateTime.Deserialize(ref buffer);
            info.TorrentCreateTime = _dateTime.Deserialize(ref buffer);
            info.CreatedBy = _string.Deserialize(ref buffer);
            info.Comment = _string.Deserialize(ref buffer);
            info.Encoding = _string.Deserialize(ref buffer);
            info.RemainingTime = _timeSpan.Deserialize(ref buffer);
            info.Files = _fileInfoNodeList.Deserialize(ref buffer);
            info.FileCount = _int.Deserialize(ref buffer);
            info.SelecrAll = _bool.Deserialize(ref buffer);
            info.TrueCount = _int.Deserialize(ref buffer);
            info.SelectedBitfieldCount = _int.Deserialize(ref buffer);
            info.V1 = _hashValue.Deserialize(ref buffer);
            info.V2 = _hashValue.Deserialize(ref buffer);

            var dict = _intStringDict.Deserialize(ref buffer);

            var pieceCount = _int.Deserialize(ref buffer);
            List<TorrentPiece> pieces = info.Pieces = new(pieceCount);
            for (int i = 0; i < pieceCount; i++)
            {
                TorrentPieceStateType state = (TorrentPieceStateType)_byte.Deserialize(ref buffer);
                var nameCount = _int.Deserialize(ref buffer);
                ValueOrList<DataBuffer<int, string>> names = new(nameCount);
                for (int j = 0; j < nameCount; j++)
                {
                    var index = _int.Deserialize(ref buffer);
                    dict.TryGetValue(index, out var name);
                    DataBuffer<int, string> data = DataBuffer<int, string>.Get();
                    data.Item1 = index;
                    data.Item2 = name ?? string.Empty;
                    names.Add(data);
                }
                TorrentPiece piece = new(state, names);
                piece.UpdateMessageType();
                pieces.Add(piece);
            }

            return info;
        }

        public override void Serialize(ref ByteBuffer buffer, TorrentInfo value)
        {
            if (value == null)
            {
                WriteNil(ref buffer);
                return;
            }

            _string.Serialize(ref buffer, value.Name);
            _long.Serialize(ref buffer, value.Size);
            _int.Serialize(ref buffer, value.PieceLength);
            _int.Serialize(ref buffer, value.PieceCount);
            _double.Serialize(ref buffer, value.Progress);
            _int.Serialize(ref buffer, value.SelectedFileCount);
            _long.Serialize(ref buffer, value.SelectedFileLength);
            _int.Serialize(ref buffer, value.SelectedFileCompleteCount);
            _long.Serialize(ref buffer, value.SelectedFileCompleteLength);
            _string.Serialize(ref buffer, value.TorrentPath);
            _string.Serialize(ref buffer, value.SavePath);
            _string.Serialize(ref buffer, value.TorrentMagnetLink);
            _dateTime.Serialize(ref buffer, value.CreateTime);
            _dateTime.Serialize(ref buffer, value.TorrentCreateTime);
            _string.Serialize(ref buffer, value.CreatedBy);
            _string.Serialize(ref buffer, value.Comment);
            _string.Serialize(ref buffer, value.Encoding);
            _timeSpan.Serialize(ref buffer, value.RemainingTime);
            _fileInfoNodeList.Serialize(ref buffer, value.Files);
            _int.Serialize(ref buffer, value.FileCount);
            _bool.Serialize(ref buffer, value.SelecrAll);
            _int.Serialize(ref buffer, value.TrueCount);
            _int.Serialize(ref buffer, value.SelectedBitfieldCount);
            _hashValue.Serialize(ref buffer, value.V1);
            _hashValue.Serialize(ref buffer, value.V2);

            Dictionary<int, string>? dict = CreateIntStringDict(value.Pieces);
            List<TorrentPiece> pieces = value.Pieces;
            _intStringDict.Serialize(ref buffer, dict);

            if (pieces != null)
            {
                _int.Serialize(ref buffer, pieces.Count);
                for (int i = 0; i < pieces.Count; i++)
                {
                    var piece = pieces[i];
                    _byte.Serialize(ref buffer, (byte)piece.State);

                    var names = piece.PieceNames;
                    _int.Serialize(ref buffer, names.Count);
                    for (int j = 0; j < names.Count; j++)
                    {
                        _int.Serialize(ref buffer, names[j].Item1);
                    }
                }
            }
            else
            {
                _int.Serialize(ref buffer, 0);
            }
        }

        public override long GetLength(TorrentInfo value)
        {
            if (value == null)
            {
                return 1;
            }

            var result = _int.DefaultLength * 8 + _long.DefaultLength * 4 + _double.DefaultLength +
                _string.GetLength(value.Name) +
                _string.GetLength(value.TorrentPath) +
                _string.GetLength(value.SavePath) +
                _string.GetLength(value.TorrentMagnetLink) +
                _dateTime.DefaultLength * 2 +
                _string.GetLength(value.CreatedBy) +
                _string.GetLength(value.Comment) +
                _string.GetLength(value.Encoding) +
                _timeSpan.DefaultLength +
                _fileInfoNodeList.GetLength(value.Files) +
                _bool.DefaultLength +
                _int.DefaultLength * 4 +
                _int.DefaultLength +
                value.Pieces.Count +
                _hashValue.GetLength(value.V1) +
                _hashValue.GetLength(value.V2) +
                _intStringDict.GetLength(CreateIntStringDict(value.Pieces));

            for (int i = 0; i < value.Pieces.Count; i++)
            {
                var piece = value.Pieces[i];
                result += _byte.DefaultLength + _int.DefaultLength + piece.PieceNames.Count * _int.DefaultLength;
            }

            return result;
        }

        private Dictionary<int, string>? CreateIntStringDict(List<TorrentPiece> pieces)
        {
            if (pieces == null || pieces.Count == 0)
                return null;

            Dictionary<int, string> dict = new Dictionary<int, string>();
            for (int i = 0; i < pieces.Count; i++)
            {
                var piece = pieces[i];
                var names = piece.PieceNames;
                for (int j = 0; j < piece.PieceNames.Count; j++)
                {
                    if (!dict.ContainsKey(names[j].Item1))
                        dict.Add(names[j].Item1, names[j].Item2);
                }
            }
            return dict;
        }
    }
}
