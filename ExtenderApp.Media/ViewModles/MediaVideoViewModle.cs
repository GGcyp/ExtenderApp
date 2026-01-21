using ExtenderApp.Abstract;
using ExtenderApp.Media.Models;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Media.ViewModles
{
    public class MediaVideoViewModle : ExtenderAppViewModel<MediaModel>
    {
        public MediaVideoViewModle(IServiceStore serviceStore) : base(serviceStore)
        {
        }
    }
}