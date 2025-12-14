using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ExtenderApp.Abstract;
using ExtenderApp.FFmpegEngines;
using ExtenderApp.Media.Audios;
using ExtenderApp.Models;
using ExtenderApp.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Media.Models
{
    /// <summary>
    /// 媒体数据类
    /// </summary>
    public class MediaModel : ExtenderAppModel
    {
        private readonly Action<FFmpegFrame> _videoFrameAction;
        private readonly Action<long> _playbackAction;
        private readonly Action _positionAction;
        private MediaEngine mediaEngine;
        private IDispatcherService dispatcherService;

        /// <summary>
        /// 视频信息集合
        /// </summary>
        public ObservableCollection<MediaInfo>? MediaInfos { get; set; }

        public MediaPlayer? MPlayer { get; set; }
        public AudioPlayer? APlayer { get; set; }

        public WriteableBitmap? Bitmap => nativeMemoryBitmap?.Bitmap;
        public NativeMemoryBitmap? nativeMemoryBitmap { get; set; }

        public IView? CurrentVideoListView { get; set; }

        public MediaInfo? SelectedVideoInfo { get; set; }

        private double volume;

        /// <summary>
        /// 音量
        /// </summary>
        public double Volume
        {
            get => volume * 100f;
            set
            {
                volume = value / 100f;
                volume = volume >= 1f ? 1f : volume <= 0f ? 0f : volume;
                if (APlayer != null)
                {
                    APlayer.Volume = (float)volume;
                    Debug.Print(volume.ToString());
                }
            }
        }

        private double rate;

        public double Rate
        {
            get => rate;
            set
            {
                if (MPlayer != null)
                {
                    MPlayer.Rate = value;
                }
                if (APlayer != null)
                {
                    APlayer.Tempo = value;
                }
                rate = value;
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
            _videoFrameAction = OnVideoFrame;
            _playbackAction = OnPlayback;
            _positionAction = () => Position = position;
            dispatcherService = default!;
            JumpTime = 10;
            rate = 1.0;
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

            SelectedVideoInfo = new(mediaUri);

            OpenMedia(player);
        }

        private void OpenMedia(MediaPlayer player)
        {
            ArgumentNullException.ThrowIfNull(player, nameof(player));

            APlayer = new AudioPlayer(player.Settings, (float)volume);
            MPlayer = player;

            TotalTime = MPlayer.Info.DurationTimeSpan;

            nativeMemoryBitmap = new(player.Info.Width, player.Info.Height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null);

            player.AudioFrameReceived += APlayer.AddSamples;
        }

        public void Play()
        {
            if (MPlayer != null)
            {
                MPlayer.VideoFrameReceived += _videoFrameAction;
                MPlayer.PlaybackReceived += _playbackAction;
            }

            MPlayer?.Play();
            APlayer?.Play();
        }

        public void Pause()
        {
            if (MPlayer != null)
            {
                MPlayer.VideoFrameReceived -= _videoFrameAction;
                MPlayer.PlaybackReceived -= _playbackAction;
            }

            APlayer?.Pause();
            MPlayer?.PauseAsync().ConfigureAwait(false);
        }

        public void Stop()
        {
            if (MPlayer != null)
            {
                MPlayer.Stop();
                MPlayer.VideoFrameReceived -= _videoFrameAction;
                MPlayer.PlaybackReceived -= _playbackAction;
            }

            if (APlayer != null)
                APlayer?.Stop();
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
            APlayer?.Pause();
            APlayer?.Clear();

            if (!auotPlay)
                return;
            Play();
        }

        public void Forward(double rate)
        {
            if (IsSeeking || MPlayer == null)
                return;

            MPlayer.Rate = rate;

            if (APlayer != null)
            {
                APlayer.Tempo = rate;
            }
        }

        private unsafe void OnVideoFrame(FFmpegFrame frame)
        {
            if (IsSeeking)
                return;

            nativeMemoryBitmap!.Write(frame.Block.UnreadSpan);
            nativeMemoryBitmap.UpdateBitmap();
        }

        private void OnPlayback(long current)
        {
            if (IsSeeking)
                return;
            position = TimeSpan.FromMilliseconds(current);
            dispatcherService.InvokeAsync(_positionAction);
        }

        protected override void DisposeManagedResources()
        {
            Stop();
            MPlayer?.Dispose();
            APlayer?.Dispose();
        }
    }
}