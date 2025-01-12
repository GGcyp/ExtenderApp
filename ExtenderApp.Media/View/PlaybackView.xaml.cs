using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtenderApp.Abstract;
using ExtenderApp.Views;

namespace ExtenderApp.Media
{
    /// <summary>
    /// PlaybackView.xaml 的交互逻辑
    /// </summary>
    public partial class PlaybackView : ExtenderAppView
    {
        private VideoModel _videoModel;

        public PlaybackView(VideoModel model)
        {
            InitializeComponent();

            _videoModel = model;

            model.PlayAction = Play;
            model.StopAction = Stop;
            model.PauseAction = Pause;
            model.FastForwardAction = FastForward;
            model.OpenVideoAction = OpenVideo;


            model.GetPosition = GetPosition;
            model.SetPosition = SetPosition;
            model.NaturalTimeSpanFunc = NaturalTimeSpan;
            model.SpeedRatioAction = SpeedRatio;

            model.SetVolume = SetVolume;
            model.GetVolume = GetVolume;

            mediaElement.MediaOpened += MediaElement_MediaOpened;
        }

        private TimeSpan GetPosition() => mediaElement.Position;

        private void SetPosition(TimeSpan span) => mediaElement.Position = span;

        private TimeSpan NaturalTimeSpan() => mediaElement.NaturalDuration.TimeSpan;

        private void SpeedRatio(double speed) => mediaElement.SpeedRatio = speed;

        private void Play() => mediaElement.Play();

        private void Pause() => mediaElement.Pause();

        private void Stop() => mediaElement.Stop();

        private void FastForward(double value)
        {
            if (mediaElement.Position < mediaElement.NaturalDuration.TimeSpan)
            {
                mediaElement.Position += TimeSpan.FromSeconds(value);
            }
        }

        private void SetVolume(double value)
        {
            mediaElement.Volume = value;
        }

        private double GetVolume()
        {
            return mediaElement.Volume;
        }

        private void OpenVideo()
        {
            if (_videoModel.CurrentVideoInfo is null)
            {
                throw new Exception("请先设置视频信息");
            }

            mediaElement.Source = new Uri(_videoModel.CurrentVideoInfo.VideoPath);

            //等他第一次打开视频后，再获取视频信息    
            if (!_videoModel.CurrentVideoInfo.IsConfiguration)
                _videoModel.CurrentVideoInfo = ConfigurationVideoInfo();
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            _videoModel.MediaOpened?.Invoke();
        }

        /// <summary>
        /// 创建一个视频信息对象
        /// </summary>
        /// <param name="path">视频文件路径</param>
        /// <returns>返回包含视频信息的VideoInfo对象</returns>
        private VideoInfo ConfigurationVideoInfo()
        {
            VideoInfo videoInfo = _videoModel.CurrentVideoInfo;

            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                videoInfo.TotalVideoDuration = mediaElement.NaturalDuration.TimeSpan;
            }

            videoInfo.VideoWidth = (int)mediaElement.ActualWidth;
            videoInfo.VideoHeight = (int)mediaElement.ActualHeight;

            FileInfo fileInfo = new FileInfo(videoInfo.VideoPath);
            if (fileInfo.Exists)
            {
                videoInfo.FileSize = fileInfo.Length;
            }

            // 以下部分信息暂时未完整填充，你可以根据实际情况拓展，比如从视频元数据获取标题、创建时间等
            videoInfo.VideoTitle = System.IO.Path.GetFileNameWithoutExtension(videoInfo.VideoPath);
            videoInfo.CreationTime = fileInfo.CreationTime;
            videoInfo.PlayCount = 0;
            videoInfo.IsFavorite = false;
            videoInfo.Category = "待分类";
            videoInfo.Rating = 0;

            return videoInfo;
        }
    }
}
