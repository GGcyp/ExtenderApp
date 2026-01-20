using CommunityToolkit.Mvvm.Input;
using ExtenderApp.Abstract;
using ExtenderApp.Media.Models;
using ExtenderApp.Media.Views;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Media.ViewModles
{
    public class MediaControlBarViewModle : ExtenderAppViewModel<MediaControlBarView, MediaModel>
    {
        #region Commands

        public RelayCommand<double> PositionChangeCommand { get; set; }

        #endregion Commands

        public MediaControlBarViewModle(IServiceStore serviceStore) : base(serviceStore)
        {
            PositionChangeCommand = new(OnPositionChange);
        }

        private void OnPositionChange(double obj)
        {
            Model.Seek(TimeSpan.FromSeconds(obj));
        }
    }
}