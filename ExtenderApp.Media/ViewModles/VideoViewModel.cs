using System.Threading.Channels;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExtenderApp.Abstract;
using ExtenderApp.Media.FFmpegEngines;
using ExtenderApp.Media.Models;
using ExtenderApp.ViewModels;
using FFmpeg.AutoGen;
using NAudio.Wave;

namespace ExtenderApp.Media.ViewModels
{
    public class VideoViewModel : ExtenderAppViewModel<VideoView, MediaModel>
    {


        public WriteableBitmap Bitmap { get; set; }
        private WaveOutEvent _waveOut;
        private BufferedWaveProvider _bufferedWave;

        public VideoViewModel(IServiceStore serviceStore, MediaEngine engine) : base(serviceStore)
        {
            if (!Environment.Is64BitProcess)
            {
                Error("仅支持64位系统", new Exception());
            }

            var waveFormat = new WaveFormat(44100, 16, 2);
            _bufferedWave = new(waveFormat);
            _waveOut = new WaveOutEvent();
            _waveOut.Init(_bufferedWave);
            _waveOut.Volume = 0.5f;
            _waveOut.Play();
            var player = engine.OpenMedia("E:\\迅雷下载\\糖心Vlog.苏小涵_黑丝魅魔性契约榨干人类精液_淫纹巨乳劲爆身材_饱满蜜鲍榨汁吸茎_再深一点内射宫腔.mp4");
            Bitmap = new(player.Info.Width, player.Info.Height, 96, 96, PixelFormats.Bgr24, null);
            player.OnVideoFrame += f =>
            {
                DispatcherInvoke(() =>
                {
                    Bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, f.Width, f.Height), f.Data, f.Stride, 0);
                });
            };
            player.OnAudioFrame += f =>
            {
                try
                {
                    _bufferedWave.AddSamples(f.Data, 0, f.Length);
                }
                catch(Exception ex)
                {
                    throw;
                }
            };
            player.Play();
        }
    }
}
