using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Media.Models;
using ExtenderApp.ViewModels;
using Microsoft.Win32;
using NAudio.Wave;

namespace ExtenderApp.Media.ViewModels
{
    public class VideoViewModel : ExtenderAppViewModel<VideoView, MediaModel>
    {
        private readonly MediaEngine _engine;

        public VideoViewModel(IServiceStore serviceStore, MediaEngine engine) : base(serviceStore)
        {
            if (!Environment.Is64BitProcess)
            {
                Error("仅支持64位系统", new Exception());
            }

            _engine = engine;

            // 创建文件选择对话框实例
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // 设置文件筛选器，仅显示 .torrent 文件
            openFileDialog.Filter = "MP4 文件 (*.mp4)|*.mp4";
            // 打开文件选择对话框并获取用户选择结果
            bool? result = openFileDialog.ShowDialog();

            string filePath = openFileDialog.FileName;
            OpenVideo(filePath);
        }

        public void OpenVideo(string filePath)
        {
            var player = _engine.OpenMedia(filePath);
            Model.SetPlayer(player);
            Model.Volume = 0.5f;
            player.OnVideoFrame += f =>
            {
                DispatcherInvoke(() =>
                {
                    Model.Bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, f.Width, f.Height), f.Data, f.Stride, 0);
                });
            };

            player.OnAudioFrame += f =>
            {
                try
                {
                    Model.BufferedWave?.AddSamples(f.Data, 0, f.Length);
                }
                catch (Exception ex)
                {
                    Model.BufferedWave?.ClearBuffer();
                    Error("音频播放出错，已清空缓冲区", ex);
                }
            };

            Model.WaveOut!.Play();
            player.Play();
        }
    }
}
