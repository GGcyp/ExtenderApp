using System.Collections.ObjectModel;
using System.Numerics;
using System.Windows.Media.Imaging;
using ExtenderApp.Abstract;
using ExtenderApp.FFmpegEngines;
using ExtenderApp.Media.Audios;
using ExtenderApp.Models;

namespace ExtenderApp.Media.Models
{
    /// <summary>
    /// 媒体数据类
    /// </summary>
    public class MediaModel : ExtenderAppModel
    {
        private readonly Action<VideoFrame> _videoFrameAction;
        private readonly Action<long> _playbackAction;
        private IDispatcherService? dispatcherService;

        /// <summary>
        /// 视频信息集合
        /// </summary>
        public ObservableCollection<MediaInfo>? MediaInfos { get; set; }

        public MediaPlayer? MPlayer { get; set; }
        public AudioPlayer? APlayer { get; set; }
        public WriteableBitmap? Bitmap { get; set; }
        public IView? CurrentVideoView { get; set; }
        public IView? CurrentVideoListView { get; set; }

        public MediaInfo? SelectedVideoInfo { get; set; }

        private double volume;
        /// <summary>
        /// 音量
        /// </summary>
        public double Volume
        {
            get => volume;
            set
            {
                volume = value;
                if (APlayer != null)
                {
                    APlayer.Volume = (float)volume;
                }
            }
        }

        /// <summary>
        /// 是否记录观看时间
        /// </summary>
        public bool RecordWatchingTime { get; set; }

        /// <summary>
        /// 视频文件不存在则删除
        /// </summary>
        public bool VideoNotExist { get; set; }

        /// <summary>
        /// 当前时间
        /// </summary>
        public TimeSpan Position { get; set; }

        /// <summary>
        /// 总时间
        /// </summary>
        public TimeSpan TotalTime { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值指示媒体是否正在寻求新的播放位置。
        /// </summary>
        public bool IsSeeking { get; set; }

        public MediaModel()
        {
            _videoFrameAction = OnVideoFrame;
            _playbackAction = OnPlayback;
        }

        protected override void Init(IPuginServiceStore store)
        {
            dispatcherService = store.DispatcherService;
            MediaInfos ??= new();
        }

        public void SetPlayer(MediaPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));


            APlayer = new AudioPlayer(player.Settings, (float)volume);
            MPlayer = player;

            TotalTime = MPlayer.Info.DurationTimeSpan;

            Bitmap = new(player.Info.Width, player.Info.Height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null);

            player.OnAudioFrame += APlayer.AddSamples;
            player.OnPlayback += _playbackAction;
        }

        public void Play()
        {
            if (MPlayer == null) return;

            MPlayer.OnVideoFrame += _videoFrameAction;

            MPlayer?.Play();
            APlayer?.Play();
        }

        public void Pause()
        {
            if (MPlayer == null) return;

            MPlayer.OnVideoFrame -= _videoFrameAction;
            MPlayer?.Pause();
            APlayer?.Pause();
        }

        public void Stop()
        {
            MPlayer?.Stop();
            APlayer?.Stop();
        }

        public void Seek(TimeSpan position, bool auotPlay = true)
        {
            Seek((long)position.TotalMilliseconds, auotPlay);
        }

        public void Seek(long position, bool auotPlay = true)
        {
            if (position > TotalTime.TotalMilliseconds)
                position = (long)TotalTime.TotalMilliseconds;

            APlayer?.Pause();

            if (MPlayer == null) return;

            Task.Run(async () =>
            {
                await MPlayer.SeekAsync(position);
                APlayer?.Clear();

                if (!auotPlay)
                    return;

                Play();
            });
        }

        private void OnVideoFrame(VideoFrame frame)
        {
            dispatcherService.Invoke(() =>
            {
                Bitmap?.WritePixels(new System.Windows.Int32Rect(0, 0, frame.Width, frame.Height), frame.Data, frame.Stride, 0);
            });
        }

        private void OnPlayback(long current)
        {
            if (IsSeeking)
                return;

            dispatcherService.Invoke(() =>
            {
                Position = TimeSpan.FromMilliseconds(current);
            });
        }

        protected override void Dispose(bool disposing)
        {
            Stop();
            MPlayer?.Dispose();
            APlayer?.Dispose();
        }
    }
}