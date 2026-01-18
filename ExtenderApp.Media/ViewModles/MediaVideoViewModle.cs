using ExtenderApp.Abstract;
using ExtenderApp.Media.Models;
using ExtenderApp.Media.Views;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Media.ViewModles
{
    public class MediaVideoViewModle : ExtenderAppViewModel<MediaVideoView, MediaModel>
    {
        public MediaVideoViewModle(IServiceStore serviceStore) : base(serviceStore)
        {
        }
    }
}