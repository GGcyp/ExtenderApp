using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Contracts;
using ExtenderApp.Models;

namespace ExtenderApp.Torrents.Models
{
    public class TorrentPiece : DataModel
    {
        public TorrentPieceStateType State { get; set; }
        public ValueOrList<DataBuffer<int, string>> PieceNames { get; set; }
        public string? Messagetype { get; private set; }

        public TorrentPiece() : this(TorrentPieceStateType.DontDownloaded, new())
        {
        }

        public TorrentPiece(TorrentPieceStateType state, ValueOrList<DataBuffer<int, string>> values)
        {
            State = state;
            PieceNames = values;
        }

        public void UpdateMessageType()
        {
            switch (State)
            {
                case TorrentPieceStateType.DontDownloaded:
                    Messagetype = "不下载";
                    break;
                case TorrentPieceStateType.ToBeDownloaded:
                    Messagetype = "要下载";
                    break;
                case TorrentPieceStateType.Downloading:
                    Messagetype = "正在下载";
                    break;
                case TorrentPieceStateType.Complete:
                    Messagetype = "已完成";
                    break;
            }
        }
    }
}
