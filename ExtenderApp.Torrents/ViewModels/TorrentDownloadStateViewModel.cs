using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtenderApp.Abstract;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentDownloadStateViewModel : ExtenderAppViewModel<TorrentDownloadStateView, TorrentModel>
    {
        public TorrentDownloadStateViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
        }
    }
}
