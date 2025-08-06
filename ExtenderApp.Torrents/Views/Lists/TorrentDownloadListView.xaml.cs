using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtenderApp.Torrents.ViewModels;
using ExtenderApp.Views;

namespace ExtenderApp.Torrents.Views
{
    /// <summary>
    /// DownloadListView.xaml 的交互逻辑
    /// </summary>
    public partial class TorrentDownloadListView : ExtenderAppView
    {
        public TorrentDownloadListView(TorrentDownloadListViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
