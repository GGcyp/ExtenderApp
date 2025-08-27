

using System.ComponentModel;

namespace ExtenderApp.Torrents.Models
{
    public class TorrentPiece : INotifyPropertyChanged
    {
        public TorrentPieceStateType State { get; set; }
        public string? Name { get; set; }
        public string? Messagetype { get; set; }

        public TorrentPiece() : this(TorrentPieceStateType.DontDownloaded)
        {
            Name = null;
        }

        public TorrentPiece(TorrentPieceStateType state)
        {
            State = state;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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
