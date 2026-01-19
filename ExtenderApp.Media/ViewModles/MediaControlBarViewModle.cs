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

        public RelayCommand<double> PositionClickCommand { get; set; }

        #endregion Commands

        public MediaControlBarViewModle(IServiceStore serviceStore) : base(serviceStore)
        {
            PositionClickCommand = new(OnPositionClick);
        }

        private void OnPositionClick(double obj)
        {
            Model.Seek(TimeSpan.FromSeconds(obj));
        }
    }
}