using System.ComponentModel;
using ExtenderApp.Torrents.Models;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents
{
    public class TorrentModel : INotifyPropertyChanged
    {
        public ClientEngine Engine { get; set; }

        public string SaveDirectory { get; set; }
        public TorrentInfo SelectedTorrent { get; set; }

        public Action PlayDowning { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
