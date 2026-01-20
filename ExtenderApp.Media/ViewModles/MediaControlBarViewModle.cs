using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using ExtenderApp.Abstract;
using ExtenderApp.FFmpegEngines;
using ExtenderApp.Media.Models;
using ExtenderApp.Media.Views;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Media.ViewModles
{
    public class MediaControlBarViewModle : ExtenderAppViewModel<MediaControlBarView, MediaModel>
    {
        #region Commands

        public RelayCommand<double> PositionChangeCommand { get; set; }

        public RelayCommand StopCommand { get; set; }

        public RelayCommand MediaStateChangeCommand { get; set; }

        public RelayCommand FastForwardCommand { get; set; }

        #endregion Commands

        public MediaControlBarViewModle(IServiceStore serviceStore) : base(serviceStore)
        {
            PositionChangeCommand = new(value => Model.Seek(TimeSpan.FromSeconds(value)));
            StopCommand = new(Model.Stop);
            MediaStateChangeCommand = new(Model.ChangeMediaState);
            FastForwardCommand = new(() => Model.ReverseOrForward());
        }
    }
}