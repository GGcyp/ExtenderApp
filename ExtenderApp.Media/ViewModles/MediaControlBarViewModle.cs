using ExtenderApp.Abstract;
using ExtenderApp.Media.Models;
using ExtenderApp.Media.Views;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;

namespace ExtenderApp.Media.ViewModles
{
    public class MediaControlBarViewModle : ExtenderAppViewModel<MediaControlBarView, MediaModel>
    {
        #region Commands

        public RelayCommand<double> ClickCommand { get; set; }

        #endregion

        public MediaControlBarViewModle(IServiceStore serviceStore) : base(serviceStore)
        {
        }

    }
}