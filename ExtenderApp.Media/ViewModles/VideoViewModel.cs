using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Media.Audios;
using ExtenderApp.Media.Models;
using ExtenderApp.ViewModels;
using Microsoft.Win32;
using SoundTouch;

namespace ExtenderApp.Media.ViewModels
{
    public class VideoViewModel : ExtenderAppViewModel<VideoView, MediaModel>
    {
        public VideoViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            if (!Environment.Is64BitProcess)
            {
                Error("仅支持64位系统", new Exception());
            }
        }
    }
}