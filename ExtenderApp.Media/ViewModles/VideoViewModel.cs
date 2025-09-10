using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExtenderApp.Abstract;
using ExtenderApp.Media.Models;
using ExtenderApp.Media.FFmpegEngines;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Media.ViewModels
{
    public class VideoViewModel : ExtenderAppViewModel<VideoView, MediaModel>
    {


        public WriteableBitmap Bitmap { get; set; }

        public VideoViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            if (!Environment.Is64BitProcess)
            {
                Error("仅支持64位系统", new Exception());
            }

            //VideoEngines.VideoEngine engine = new VideoEngines.VideoEngine(GetPluginFolder(ffmpegFolderName));
            //VideoCodec video = engine.CreateVideo("D:\\迅雷下载\\国产网红.推特_一条肌肉狗_后入爆肏极品黑丝高跟骚母狗_1.mp4", new VideoOutSettings());
            //var info = video.Info;
            //Bitmap = new(info.Width, info.Height, 94, 94, PixelFormats.Bgr24, null);
            //video.ProgressChanged += (s, e) =>
            //{
            //    DispatcherInvoke(() =>
            //    {
            //        Bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, info.Width, info.Height), e, info.Width * 3, 0);
            //    });
            //};
            //Task.Run(async () =>
            //{
            //    await video.InitAsync();
            //    video.StartPlayback();
            //});
        }
    }
}
