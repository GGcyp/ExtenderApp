using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using ExtenderApp.Abstract;
using ExtenderApp.FFmpegEngines;
using ExtenderApp.FFmpegEngines.Medias;
using ExtenderApp.FFmpegEngines.Medias.Outputs;
using ExtenderApp.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Media.Models
{
    /// <summary>
    /// 媒体数据类
    /// </summary>
    public class MediaModel : ExtenderAppModel
    {
        private readonly Action _positionAction;
        private MediaEngine mediaEngine;
        private IDispatcherService dispatcherService;

        /// <summary>
        /// 视频信息集合
        /// </summary>
        public ObservableCollection<MediaInfo>? MediaInfos { get; set; }

        public MediaPlayer? MPlayer { get; set; }

        public WriteableBitmap? Bitmap { get; set; }

        public IView? CurrentVideoListView { get; set; }

        public MediaInfo? SelectedVideoInfo { get; set; }

        private double volume;

        /// <summary>
        /// 音量
        /// </summary>
        public double Volume
        {
            get => volume;
            set => volume = value;
        }

        private double rate;

        public double Rate
        {
            get => rate;
            set => rate = value;
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

        private TimeSpan position;

        /// <summary>
        /// 播放进度
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

        public int JumpTime { get; set; }

        public MediaModel()
        {
            _positionAction = () => Position = TimeSpan.FromMilliseconds(MPlayer!.Position);
            dispatcherService = default!;
            JumpTime = 10;
            rate = 1.0;
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

            VideoOutput videoOutput = new(MPlayer.Info.Width, MPlayer.Info.Height, 96, 96, System.Windows.Media.PixelFormats.Rgb24, null);
            Bitmap = videoOutput.NativeMemoryBitmap.Bitmap;
            MPlayer.SetVideoOutput(videoOutput);

            AudioOutput audioOutput = new(MPlayer.Settings);
            MPlayer.SetAudioOutput(audioOutput);

            MPlayer.Playback += () =>
            {
                dispatcherService.InvokeAsync(_positionAction);
            };
            TotalTime = MPlayer.Info.DurationTimeSpan;
        }

        public void Play()
        {
            if (MPlayer == null) return;

            MPlayer?.Play();
        }

        public void Pause()
        {
            MPlayer?.PauseAsync().ConfigureAwait(false);
        }

        public void Stop()
        {
            if (MPlayer != null)
            {
                MPlayer.Stop();
            }
        }

        public Task Seek(TimeSpan position, bool auotPlay = true)
        {
            return Seek((long)position.TotalMilliseconds, auotPlay);
        }

        public async Task Seek(long position, bool auotPlay = true)
        {
            if (MPlayer == null)
                return;

            if (position > TotalTime.TotalMilliseconds)
                position = (long)TotalTime.TotalMilliseconds;
            else if (position < 0)
                position = 0;

            await MPlayer.SeekAsync(position).ConfigureAwait(false);

            if (!auotPlay)
                return;
            Play();
        }

        public void Forward(double rate)
        {
            if (IsSeeking || MPlayer == null)
                return;

            MPlayer.Rate = rate;
        }

        protected override void DisposeManagedResources()
        {
            Stop();
            MPlayer?.Dispose();
        }
    }
}