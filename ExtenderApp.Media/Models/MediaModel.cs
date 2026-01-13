using System.Collections.ObjectModel;
using System.Numerics;
using System.Windows.Media.Imaging;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.FFmpegEngines;
using ExtenderApp.FFmpegEngines.Medias;
using ExtenderApp.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Media.Models
{
    /// <summary>
    /// 媒体数据模型（可作为 GetViewModel 使用）。
    /// <para>
    /// 负责：
    /// <list type="bullet">
    /// <item><description>通过 <see cref="MediaEngine"/> 打开媒体并维护当前 <see cref="IMediaPlayer"/> 实例。</description></item>
    /// <item><description>持有视频输出 <see cref="Bitmap"/>（<see cref="BitmapSource"/>），供 WPF 绑定显示。</description></item>
    /// <item><description>订阅 <see cref="IMediaPlayer.Playback"/> 事件，在 UI 线程更新 <see cref="Position"/>。</description></item>
    /// <item><description>对外暴露播放控制：播放/暂停/停止/跳转/音量/倍速。</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public class MediaModel : ExtenderAppModel
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
        private MediaEngine mediaEngine;

        /// <summary>
        /// UI 调度服务：用于从后台回调切回 UI 线程更新绑定属性。
        /// </summary>
        private IDispatcherService dispatcherService;

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
        /// 当前视频列表视图（用于导航/显示视频列表）。
        /// </summary>
        public IView? CurrentVideoListView { get; set; }

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
        /// 是否正在 Seek（业务/UI 状态标记）。
        /// </summary>
        public bool IsSeeking { get; set; }

        /// <summary>
        /// 快进/快退跳转秒数（业务/UI 设定）。
        /// </summary>
        public int JumpTime { get; set; }

        public MediaModel()
        {
            // 保存一个固定 Action，供 Dispatcher 回调使用
            _positionAction = UpdatePosition;

            dispatcherService = default!;
            JumpTime = 10;
            speedRatio = 1.0d;
            mediaEngine = default!;
        }

        /// <summary>
        /// 初始化：从插件容器获取依赖服务。
        /// </summary>
        protected override void Init(IPuginServiceStore store)
        {
            dispatcherService = store.DispatcherService;
            mediaEngine = store.ServiceProvider.GetService<MediaEngine>() ?? throw new InvalidOperationException("MediaEngine服务未注册。");
            MediaInfos ??= new();
        }

        /// <summary>
        /// 打开媒体并创建新的播放器实例，同时配置音视频输出。
        /// <para>若已有旧播放器，会先安全释放旧播放器（DisposeSafe）。</para>
        /// </summary>
        public void OpenMedia(string mediaUri)
        {
            var player = mediaEngine.OpenMedia(mediaUri);
            SelectedVideoInfo = player.Info;
            OpenMedia(player);
        }

        public void OpenMedia(MediaInfo mediaInfo)
        {
            var player = mediaEngine.OpenMedia(mediaInfo.MediaUri);
            SelectedVideoInfo = mediaInfo;
            OpenMedia(player);
        }

        public void OpenMedia(IMediaPlayer mediaPlayer)
        {
            if (MPlayer != null)
            {
                MPlayer.DisposeSafe();
            }

            MPlayer = mediaPlayer;
            Bitmap = MPlayer.GetBitmapSource() ?? MPlayer.SetVideoOutput();
            MPlayer.SetAudioOutput();

            Volume = volume;
            MPlayer.Playback += UpdatePlayback;
            OnPropertyChanged(nameof(TotalTime));
            Position = TimeSpan.FromMilliseconds(MPlayer.Position);
        }

        /// <summary>
        /// 开始或恢复播放。
        /// </summary>
        public void Play()
        {
            MPlayer?.Play();
        }

        /// <summary>
        /// 暂停播放。
        /// </summary>
        public void Pause()
        {
            MPlayer?.Pause();
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
            }
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
            dispatcherService.InvokeAsync(_positionAction);
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

        /// <summary>
        /// 释放托管资源：安全释放播放器实例。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            MPlayer?.DisposeSafe();
        }
    }
}