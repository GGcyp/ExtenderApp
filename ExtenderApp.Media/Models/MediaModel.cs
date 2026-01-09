using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using ExtenderApp.Abstract;
using ExtenderApp.FFmpegEngines;
using ExtenderApp.FFmpegEngines.Medias;
using ExtenderApp.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Media.Models
{
    /// <summary>
    /// 媒体数据类
    /// </summary>
    public class MediaModel : ExtenderAppModel
    {
        private const float VOLUME_STEP = 0.05f;
        private readonly Action _positionAction;
        private MediaEngine mediaEngine;
        private IDispatcherService dispatcherService;

        /// <summary>
        /// 视频信息集合
        /// </summary>
        public ObservableCollection<MediaInfo>? MediaInfos { get; set; }

        public MediaPlayer? MPlayer { get; set; }

        public BitmapSource? Bitmap { get; set; }

        public IView? CurrentVideoListView { get; set; }

        public MediaInfo? SelectedVideoInfo { get; set; }

        private float volume;

        /// <summary>
        /// 音量
        /// </summary>
        public float Volume
        {
            get => volume;
            set
            {
                if (MPlayer != null)
                {
                    MPlayer.Volume = value;
                    volume = MPlayer.Volume;
                }
                else
                {
                    volume = value;
                }
            }
        }

        private double speedRatio;

        public double SpeedRatio
        {
            get => speedRatio;
            set
            {
                if (MPlayer != null)
                {
                    MPlayer.SpeedRatio = value;
                    speedRatio = MPlayer.SpeedRatio;
                }
                else
                {
                    speedRatio = value;
                }
            }
        }

        public PlayerState? State
        {
            get
            {
                if (MPlayer is not null)
                    return MPlayer.State;

                return PlayerState.Uninitialized;
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
        /// 播放进度
        /// </summary>
        public TimeSpan Position { get; set; }

        /// <summary>
        /// 总时间
        /// </summary>
        public TimeSpan TotalTime
        {
            get
            {
                if (MPlayer != null)
                    return TimeSpan.FromMilliseconds(MPlayer.Info.Duration);
                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示媒体是否正在寻求新的播放位置。
        /// </summary>
        public bool IsSeeking { get; set; }

        public int JumpTime { get; set; }

        public MediaModel()
        {
            _positionAction = () => Position = TimeSpan.FromMilliseconds(MPlayer!.Position);
            dispatcherService = default!;
            JumpTime = 10;
            speedRatio = 1.0d;
            mediaEngine = default!;
        }

        protected override void Init(IPuginServiceStore store)
        {
            dispatcherService = store.DispatcherService;
            mediaEngine = store.ServiceProvider.GetService<MediaEngine>() ?? throw new InvalidOperationException("MediaEngine服务未注册。");
            MediaInfos ??= new();
        }

        public void OpenMedia(string mediaUri)
        {
            var player = mediaEngine.OpenMedia(mediaUri);

            MPlayer = player;

            // 使用 Bgr24 以匹配 FFmpegDecoderSettings 的默认输出格式 (PIX_FMT_BGR24)
            // 解决颜色通道互换（偏蓝）的问题
            Bitmap = MPlayer.SetVideoOutput();
            MPlayer.SetAudioOutput();

            Volume = volume;

            MPlayer.Playback += () =>
            {
                dispatcherService.InvokeAsync(_positionAction);
            };
        }

        public void Play()
        {
            MPlayer?.Play();
        }

        public void Pause()
        {
            MPlayer?.Pause();
        }

        public void Stop()
        {
            MPlayer?.StopAsync();
        }

        public void Seek(TimeSpan position)
        {
            Seek((long)position.TotalMilliseconds);
        }

        public void Seek(long position)
        {
            if (MPlayer == null)
                return;

            if (position > TotalTime.TotalMilliseconds)
                position = (long)TotalTime.TotalMilliseconds;
            else if (position < 0)
                position = 0;

            MPlayer.Seek(position);
        }

        public void UpdateVolume(bool isIncrease, float volume = VOLUME_STEP)
        {
            Volume = isIncrease ? Volume + volume : Volume - volume;
        }

        protected override void DisposeManagedResources()
        {
            Stop();
            MPlayer?.Dispose();
        }
    }
}