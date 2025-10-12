using System.Collections.ObjectModel;
using System.Xml.Linq;
using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.IO.Binary.Formatters;
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
            info.V1 = _hashValue.Deserialize(ref reader);
            info.V2 = _hashValue.Deserialize(ref reader);

            var dict = _intStringDict.Deserialize(ref reader);

            var pieceCount = _int.Deserialize(ref reader);
            List<TorrentPiece> pieces = info.Pieces = new(pieceCount);
            for (int i = 0; i < pieceCount; i++)
            {
                TorrentPieceStateType state = (TorrentPieceStateType)_byte.Deserialize(ref reader);
                var nameCount = _int.Deserialize(ref reader);
                ValueOrList<DataBuffer<int, string>> names = new(nameCount);
                for (int j = 0; j < nameCount; j++)
                {
                    var index = _int.Deserialize(ref reader);
                    dict.TryGetValue(index, out var name);
                    DataBuffer<int, string> data = DataBuffer<int, string>.GetDataBuffer();
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
            _hashValue.Serialize(ref writer, value.V1);
            _hashValue.Serialize(ref writer, value.V2);

            Dictionary<int, string>? dict = CreateIntStringDict(value.Pieces);
            List<TorrentPiece> pieces = value.Pieces;
            _intStringDict.Serialize(ref writer, dict);

            if (pieces != null)
            {
                _int.Serialize(ref writer, pieces.Count);
                for (int i = 0; i < pieces.Count; i++)
                {
                    var piece = pieces[i];
                    _byte.Serialize(ref writer, (byte)piece.State);

                    var names = piece.PieceNames;
                    _int.Serialize(ref writer, names.Count);
                    for (int j = 0; j < names.Count; j++)
                    {
                        _int.Serialize(ref writer, names[j].Item1);
                    }
                }
            }
            else
            {
                _int.Serialize(ref writer, 0);
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
