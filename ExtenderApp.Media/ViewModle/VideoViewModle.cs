using System.IO;
using System.Windows;
using System.Windows.Controls;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Media
{
    public class VideoViewModle : ExtenderAppViewModel<VideoView, MediaModel>
    {
        private MediaElement mediaElement;

        public VideoViewModle(IServiceStore serviceStore) : base(serviceStore)
        {

        }

        public override void InjectView(VideoView view)
        {
            base.InjectView(view);

            this.mediaElement = view.mediaElement;

            Model.PlayAction = Play;
            Model.StopAction = Stop;
            Model.PauseAction = Pause;
            Model.FastForwardAction = FastForward;
            Model.OpenVideoAction = OpenVideo;


            Model.GetPosition = GetPosition;
            Model.SetPosition = SetPosition;
            Model.NaturalDurationFunc = NaturalDuration;
            Model.SpeedRatioAction = SpeedRatio;

            Model.SetVolume = SetVolume;
            Model.GetVolume = GetVolume;

            mediaElement.MediaOpened += MediaElement_MediaOpened;
        }

        private TimeSpan GetPosition() => mediaElement.Position;

        private void SetPosition(TimeSpan span) => mediaElement.Position = span;

        private Duration NaturalDuration() => mediaElement.NaturalDuration;

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
            if (Model.CurrentVideoInfo is null)
            {
                throw new Exception("请先设置视频信息");
            }

            View.mediaElement.Source = Model.CurrentVideoInfo.VideoUri;

            //等他第一次打开视频后，再获取视频信息    
            if (!Model.CurrentVideoInfo.IsConfiguration)
                Model.CurrentVideoInfo = ConfigurationVideoInfo();
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            Model.MediaOpened?.Invoke();
        }

        /// <summary>
        /// 创建一个视频信息对象
        /// </summary>
        /// <param name="path">视频文件路径</param>
        /// <returns>返回包含视频信息的VideoInfo对象</returns>
        private VideoInfo ConfigurationVideoInfo()
        {
            VideoInfo videoInfo = Model.CurrentVideoInfo;

            var duration = mediaElement.NaturalDuration;
            if (duration.HasTimeSpan)
            {
                videoInfo.TotalVideoDuration = duration.TimeSpan;
            }

            videoInfo.VideoWidth = mediaElement.NaturalVideoWidth;
            videoInfo.VideoHeight = mediaElement.NaturalVideoHeight;

            if (videoInfo.VideoUri.IsFile)
            {
                FileInfo fileInfo = new FileInfo(videoInfo.VideoUri.LocalPath);
                if (fileInfo.Exists)
                {
                    videoInfo.FileSize = fileInfo.Length;
                }
                videoInfo.VideoTitle = System.IO.Path.GetFileNameWithoutExtension(fileInfo.FullName);
                videoInfo.CreationTime = fileInfo.CreationTime;
            }
            else
            {
                videoInfo.VideoTitle = videoInfo.VideoUri.ToString();
                videoInfo.CreationTime = DateTime.Now;
            }

            videoInfo.PlayCount = 0;
            videoInfo.IsFavorite = false;
            videoInfo.Category = "待分类";
            videoInfo.Rating = 0;
            //videoInfo.IsConfiguration = true;

            return videoInfo;
        }

    }
}
