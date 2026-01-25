using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using ExtenderApp.Common;
using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines;
using ExtenderApp.FFmpegEngines.Medias;
using ExtenderApp.Media.Models;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Media.ViewModels
{
    public class MediaMainViewModel : ExtenderAppViewModel
    {
        /// <summary>
        /// 音量调整步长（UI 上一次加/减的默认增量）。
        /// </summary>
        private const float VOLUME_STEP = 0.05f;

        /// <summary>
        /// 用于在 UI 线程执行的“更新播放进度”动作。
        /// <para>避免每次回调都分配新的 lambda。</para>
        /// </summary>
        private readonly Action _positionAction;

        /// <summary>
        /// 媒体引擎：负责创建播放器实例、构建解码/播放链路。
        /// </summary>
        private readonly MediaEngine _mediaEngine;

        /// <summary>
        /// 视频信息集合（例如本地库/列表）。
        /// </summary>
        public ObservableCollection<MediaInfo>? MediaInfos { get; set; }

        /// <summary>
        /// 当前播放器实例。
        /// <para>注意：该引用会在 <see cref="OpenMedia(string)"/> 中被替换。</para>
        /// </summary>
        public IMediaPlayer? MPlayer { get; set; }

        /// <summary>
        /// 当前视频源的可绑定位图输出。
        /// <para>通常由 <see cref="IMediaPlayer.SetVideoOutput"/> 创建并返回。</para>
        /// </summary>
        public BitmapSource? Bitmap { get; set; }

        /// <summary>
        /// 当前选中的视频信息。
        /// </summary>
        public MediaInfo? SelectedVideoInfo { get; set; }

        private float volume;

        /// <summary>
        /// 音量（0.0 ~ 1.0）。
        /// <para>若已打开播放器，则同步写入播放器并回读裁剪后的实际值。</para>
        /// </summary>
        public float Volume
        {
            get => volume;
            set
            {
                if (value == volume)
                    return;

                if (MPlayer != null)
                {
                    MPlayer.SetVolume(value);
                    volume = MPlayer.GetVolume();
                }
                else
                {
                    volume = value;
                }
                OnPropertyChanged(nameof(VolumePercent));
            }
        }

        /// <summary>
        /// 暴露给 UI 的整型百分比（0-100），无小数点。设置该属性会更新内部 <see cref="Volume"/>。
        /// </summary>
        public int VolumePercent
        {
            get => (int)Math.Round(Volume * 100.0f);
            set
            {
                var percent = Math.Clamp(value, 0, 100);
                Volume = percent / 100f;
            }
        }

        private double speedRatio;

        /// <summary>
        /// 播放速率倍率。
        /// <para>若已打开播放器，则同步写入播放器并回读内部修正后的实际值。</para>
        /// </summary>
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

        /// <summary>
        /// 当前播放器状态（只读透传）。
        /// <para>播放器未创建时返回 <see cref="PlayerState.Uninitialized"/>。</para>
        /// </summary>
        public PlayerState? State
        {
            get
            {
                if (MPlayer is not null)
                    return MPlayer.State;

                return PlayerState.Uninitialized;
            }
        }

        public bool IsPlaying
        {
            get
            {
                var state = State;
                return state == PlayerState.Playing || state == PlayerState.Buffering;
            }
        }

        /// <summary>
        /// 是否记录观看时间（业务开关）。
        /// </summary>
        public bool RecordWatchingTime { get; set; }

        /// <summary>
        /// 视频文件不存在则删除（业务开关）。
        /// </summary>
        public bool VideoNotExist { get; set; }

        /// <summary>
        /// 当前播放进度（用于 UI 绑定显示）。
        /// <para>由 <see cref="IMediaPlayer.Playback"/> 回调驱动更新。</para>
        /// </summary>
        public TimeSpan Position { get; set; }

        /// <summary>
        /// 媒体总时长（用于 UI 绑定显示）。
        /// </summary>
        public TimeSpan TotalTime { get; set; }

        /// <summary>
        /// 快进/快退跳转秒数（业务/UI 设定）。
        /// </summary>
        public int JumpTime { get; set; }

        #region Commands

        public RelayCommand<double> PositionChangeCommand { get; set; }

        public RelayCommand StopCommand { get; set; }

        public RelayCommand MediaStateChangeCommand { get; set; }

        public RelayCommand FastForwardCommand { get; set; }

        public RelayCommand<MediaInfo> OpenMediaInfoCommand { get; set; }

        #endregion Commands

        public MediaMainViewModel(MediaEngine mediaEngine)
        {
            _mediaEngine = mediaEngine;
            _positionAction = UpdatePosition;
            JumpTime = 10;
            speedRatio = 1.0d;
            mediaEngine = default!;
            MediaInfos = new();

            PositionChangeCommand = new(Seek);
            StopCommand = new(Stop);
            MediaStateChangeCommand = new(ChangeMediaState);
            FastForwardCommand = new(Forward);
            OpenMediaInfoCommand = new(m =>
            {
                OpenMedia(m);
                Play();
            });
        }

        public override void Inject(IServiceProvider serviceProvider)
        {
            base.Inject(serviceProvider);
            SubscribeMessage<KeyDownEvent>(OnKeyDown);
            SubscribeMessage<KeyUpEvent>(OnKeyUp);
        }

        private void OnKeyUp(object? sender, KeyUpEvent e)
        {
            Key key = e.Key;
            bool isRepeat = e.IsRepeat;

            switch (key)
            {
                case Key.Left:
                    ReverseOrForward(false);
                    break;

                case Key.Right:
                    if (SpeedRatio != 1.0d)
                    {
                        SpeedRatio = 1.0d;
                    }
                    else
                    {
                        ReverseOrForward(true);
                    }
                    break;
            }
        }

        private void OnKeyDown(object? sender, KeyDownEvent e)
        {
            Key key = e.Key;
            bool isRepeat = e.IsRepeat;

            switch (key)
            {
                case Key.Right:
                    if (isRepeat)
                    {
                        SpeedRatio = 2.0d;
                    }
                    break;

                case Key.Space:
                    if (!isRepeat)
                        ChangeMediaState();
                    break;

                case Key.Up:
                    UpdateVolume(true);
                    break;

                case Key.Down:
                    UpdateVolume(false);
                    break;
            }
        }

        /// <summary>
        /// 打开媒体并创建新的播放器实例，同时配置音视频输出。
        /// <para>若已有旧播放器，会先安全释放旧播放器（DisposeSafe）。</para>
        /// </summary>
        public void OpenMedia(string mediaUri)
        {
            var player = _mediaEngine.OpenMedia(mediaUri);
            SelectedVideoInfo = player.Info;
            OpenMedia(player);
        }

        public void OpenMedia(MediaInfo? mediaInfo)
        {
            if (mediaInfo == null)
                return;
            var player = _mediaEngine.OpenMedia(mediaInfo.MediaUri);
            SelectedVideoInfo = mediaInfo;
            OpenMedia(player);
        }

        public void OpenMedia(IMediaPlayer mediaPlayer)
        {
            if (MPlayer != null)
            {
                if (mediaPlayer.Info.MediaUri == MPlayer.Info.MediaUri)
                {
                    // 相同媒体，无需重新打开
                    return;
                }
                MPlayer.DisposeSafeAsync();
            }

            MPlayer = mediaPlayer;
            Bitmap = MPlayer.GetBitmapSource() ?? MPlayer.SetVideoOutput();
            MPlayer.SetAudioOutput();

            Volume = volume;
            MPlayer.Playback += UpdatePlayback;
            TotalTime = TimeSpan.FromMilliseconds(MPlayer.Info.Duration);
            Position = TimeSpan.FromMilliseconds(MPlayer.Position);
        }

        public void AddMediaInfo(string[] mediaArray)
        {
            foreach (var mediaPath in mediaArray)
            {
                MediaInfos!.Add(_mediaEngine.CreateFFmpegInfo(mediaPath));
            }
        }

        public void ChangeMediaState()
        {
            switch (State)
            {
                case PlayerState.Playing:
                    Pause();
                    break;

                case PlayerState.Paused:
                case PlayerState.Stopped:
                case PlayerState.Initializing:
                    Play();
                    break;
            }
        }

        /// <summary>
        /// 开始或恢复播放。
        /// </summary>
        public void Play()
        {
            MPlayer?.Play();
            OnPropertyChanged(nameof(IsPlaying));
        }

        /// <summary>
        /// 暂停播放。
        /// </summary>
        public void Pause()
        {
            MPlayer?.Pause();
            OnPropertyChanged(nameof(IsPlaying));
        }

        /// <summary>
        /// 停止播放。
        /// <para>当前实现为“触发停止但不等待完成”。若需严格停止后再切换媒体，建议改为 async 并 await。</para>
        /// </summary>
        public void Stop()
        {
            if (MPlayer != null)
            {
                // 触发停止（不等待完成）
                MPlayer.StopAsync();

                // 解除订阅，避免停止过程中仍回调到 UI
                MPlayer.Playback -= UpdatePlayback;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

        public void Seek(double position)
        {
            Seek(TimeSpan.FromSeconds(position));
        }

        /// <summary>
        /// 跳转到指定时间点（TimeSpan）。
        /// </summary>
        public void Seek(TimeSpan position)
        {
            Seek((long)position.TotalMilliseconds);
        }

        /// <summary>
        /// 跳转到指定时间点（毫秒）。
        /// </summary>
        public void Seek(long position)
        {
            MPlayer?.Seek(position);
        }

        public void Forward()
        {
            ReverseOrForward(true);
        }

        public void ReverseOrForward(bool isForward = true)
        {
            TimeSpan jumpTime = TimeSpan.FromSeconds(JumpTime);
            TimeSpan targetTime = isForward ? Position + jumpTime : Position - jumpTime;
            Seek(targetTime);
        }

        /// <summary>
        /// 调整音量（按步长加/减）。
        /// </summary>
        public void UpdateVolume(bool isIncrease, float volume = VOLUME_STEP)
        {
            Volume = isIncrease ? Volume + volume : Volume - volume;
        }

        /// <summary>
        /// 播放器进度回调：切回 UI 线程更新 <see cref="Position"/>。
        /// </summary>
        private void UpdatePlayback()
        {
            DispatcherInvokeAsync(_positionAction);
        }

        /// <summary>
        /// UI 线程执行：根据播放器当前 Position 更新本模型的 <see cref="Position"/>。
        /// </summary>
        private void UpdatePosition()
        {
            if (MPlayer != null)
            {
                Position = TimeSpan.FromMilliseconds(MPlayer.Position);
            }
        }

        protected override void DisposeManagedResources()
        {
            MPlayer?.DisposeSafeAsync();
        }
    }
}