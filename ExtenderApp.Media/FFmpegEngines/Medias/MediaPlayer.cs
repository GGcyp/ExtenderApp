using System.Diagnostics;
using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines.Decoders;

namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// FFmpeg 媒体播放器，负责音视频解码、播放、暂停、停止、跳转等控制。 支持多线程解码和帧调度，适用于 WPF 播放场景。
    /// </summary>
    public class MediaPlayer : DisposableObject
    {
        /// <summary>
        /// 跳帧等待阈值（毫秒），用于控制播放节奏。
        /// </summary>
        private const int SkipWaitingTime = 5;

        /// <summary>
        /// 每个视频帧的最大允许输出时间差（毫秒），用于同步音视频。
        /// </summary>
        private const int FrameOutTime = 15;

        /// <summary>
        /// 当需要等待且剩余时间很短时，使用自旋以减少 Sleep(1) 带来的抖动。
        /// </summary>
        private const int SpinThresholdMs = 2;

        /// <summary>
        /// 解码控制器，负责解码流程的启动、停止和跳转。
        /// </summary>
        private readonly FFmpegDecoderController _controller;

        /// <summary>
        /// 视频画面解码器实例。
        /// </summary>
        private readonly FFmpegDecoder? _videoDecoder;

        /// <summary>
        /// 音频解码器实例。
        /// </summary>
        private readonly FFmpegDecoder? _audioDecoder;

        /// <summary>
        /// 媒体播放任务。
        /// </summary>
        private Task? mediaTask;

        /// <summary>
        /// 媒体播放流程的取消令牌源。
        /// </summary>
        private CancellationTokenSource? mediaSource;

        /// <summary>
        /// 是否正在跳转。
        /// </summary>
        private volatile bool isSeeking;

        /// <summary>
        /// 声音输出接口实例。
        /// </summary>
        public IAudioOutput? AudioOutput { get; private set; }

        /// <summary>
        /// 视频输出接口实例。
        /// </summary>
        public IVideoOutput? VideoOutput { get; private set; }

        /// <summary>
        /// 解码器设置参数。
        /// </summary>
        public FFmpegDecoderSettings Settings { get; }

        /// <summary>
        /// 当前播放时间（微秒）。
        /// </summary>
        public long Position { get; private set; }

        private float volume;

        /// <summary>
        /// 音量
        /// </summary>
        public float Volume
        {
            get => volume;
            set
            {
                if (value < 0f)
                {
                    if (volume == 0f)
                        return;
                    value = 0f;
                }
                else if (value > 1f)
                {
                    if (volume == 1f)
                        return;
                    value = 1f;
                }

                volume = value;
                if (AudioOutput != null)
                {
                    AudioOutput.Volume = volume;
                }
            }
        }

        private double speedRatio;

        /// <summary>
        /// 播放速率，1表示正常速度，2表示两倍速，0.5表示半速。
        /// </summary>
        public double SpeedRatio
        {
            get => speedRatio;
            set
            {
                if (speedRatio == value)
                    return;
                else if (value < 0.05d)
                    value = 0.05d;

                speedRatio = value;
                if (AudioOutput != null)
                {
                    AudioOutput.SpeedRatio = speedRatio;
                }
            }
        }

        /// <summary>
        /// 媒体流基本信息（如时长、帧率等）。
        /// </summary>
        public FFmpegInfo Info => _controller.Info;

        /// <summary>
        /// 媒体播放器当前状态。
        /// </summary>
        public PlayerState State { get; private set; }

        #region Events

        /// <summary>
        /// 每次播放进度更新时触发，传递当前播放时间（微秒）。
        /// </summary>
        public event Action? Playback;

        #endregion Events

        /// <summary>
        /// 初始化 MediaPlayer 实例。
        /// </summary>
        /// <param name="contriller">解码控制器。</param>
        /// <param name="allSource">全局取消令牌源。</param>
        /// <param name="settings">解码器设置。</param>
        public MediaPlayer(FFmpegDecoderController contriller, FFmpegDecoderSettings settings)
        {
            State = PlayerState.Uninitialized;
            _controller = contriller;
            Settings = settings;
            SpeedRatio = 1;
            State = PlayerState.Initializing;

            var collection = _controller.DecoderCollection;
            _videoDecoder = collection.GetDecoder(FFmpegMediaType.VIDEO);
            _audioDecoder = collection.GetDecoder(FFmpegMediaType.AUDIO);
        }

        /// <summary>
        /// 设置音频输出接口。
        /// </summary>
        /// <param name="output">音频输出接口实例。</param>
        /// <exception cref="InvalidOperationException">如果在播放过程中尝试更改输出接口，则抛出此异常。</exception>
        public void SetAudioOutput(IAudioOutput output)
        {
            if (mediaTask != null)
                throw new InvalidOperationException("在播放过程中无法更改音频输出。");

            AudioOutput = output;
        }

        /// <summary>
        /// 设置视频输出接口。
        /// </summary>
        /// <param name="output">视频输出接口实例。</param>
        /// <exception cref="InvalidOperationException">如果在播放过程中尝试更改输出接口，则抛出此异常。</exception>
        public void SetVideoOutput(IVideoOutput output)
        {
            if (mediaTask != null)
                throw new InvalidOperationException("在播放过程中无法更改视频输出。");
            VideoOutput = output;
        }

        #region Output Methods

        /// <summary>
        /// 启动媒体播放流程，开始解码和播放。
        /// </summary>
        public void Play()
        {
            if (State == PlayerState.Playing)
            {
                return;
            }
            State = PlayerState.Playing;
            _controller.StartDecode();
            mediaSource = CancellationTokenSource.CreateLinkedTokenSource(_controller.AllSource.Token);
            mediaTask = Task.Run(PlaybackLoop, mediaSource.Token);
            AudioOutput?.PlayerStateChange(State);
            VideoOutput?.PlayerStateChange(State);
        }

        /// <summary>
        /// 停止媒体播放流程，终止解码和播放任务，释放相关资源并清空帧队列。 若当前已处于停止状态则直接返回。
        /// </summary>
        public ValueTask StopAsync()
        {
            if (State == PlayerState.Stopped || State == PlayerState.Uninitialized)
            {
                return ValueTask.CompletedTask;
            }

            State = PlayerState.Stopped;
            AudioOutput?.PlayerStateChange(State);
            VideoOutput?.PlayerStateChange(State);
            // 停止解码流程 释放播放任务资源
            return new(Task.WhenAll(_controller.StopDecodeAsync(), FlushAsync()));
        }

        /// <summary>
        /// 暂停媒体播放流程，终止解码和播放任务，释放相关资源。 若当前已处于暂停状态则直接返回。
        /// </summary>
        public ValueTask PauseAsync()
        {
            if (State == PlayerState.Paused || State == PlayerState.Uninitialized)
            {
                return ValueTask.CompletedTask;
            }

            State = PlayerState.Paused;
            AudioOutput?.PlayerStateChange(State);
            VideoOutput?.PlayerStateChange(State);
            return new(Task.WhenAll(_controller.StopDecodeAsync(), FlushAsync()));
        }

        /// <summary>
        /// 跳转到指定时间点（TimeSpan），内部按毫秒处理。
        /// </summary>
        /// <param name="timeSpan">目标跳转时间（TimeSpan）。</param>
        public void Seek(TimeSpan timeSpan)
        {
            Seek((long)timeSpan.TotalMilliseconds);
        }

        /// <summary>
        /// 跳转到指定时间点（毫秒），重置解码器和播放状态，清空帧队列并触发进度回调。 若未初始化则直接返回，跳转超出范围时自动修正。
        /// </summary>
        /// <param name="position">目标跳转时间（毫秒）。</param>
        public void Seek(long position)
        {
            if (State == PlayerState.Uninitialized || position == Position)
                return;

            if (position > Info.Duration)
            {
                position = Info.Duration;
            }
            else if (position < 0)
            {
                position = 0;
            }

            // 跳转解码器 释放播放任务资源
            isSeeking = true;
            _controller.SeekDecoder(position);
            Position = position;
            Playback?.Invoke();          // 触发进度回调
            State = PlayerState.Initializing;
            isSeeking = false;
        }

        /// <summary>
        /// 释放媒体播放任务相关资源，取消并等待播放任务完成，释放取消令牌。 若播放任务不存在则直接返回。
        /// </summary>
        private async Task FlushAsync()
        {
            mediaSource?.Cancel();

            try
            {
                if (mediaTask == null)
                {
                    return;
                }

                await mediaTask.ConfigureAwait(false);
            }
            finally
            {
                mediaSource?.Dispose();
                mediaSource = null;
                mediaTask = null;
            }
        }

        /// <summary>
        /// 媒体播放主循环任务，负责音视频帧的调度与播放。
        /// </summary>
        private void PlaybackLoop()
        {
            var token = mediaSource!.Token;
            Func<bool> getTokenCancellationRequested = () => token.IsCancellationRequested;

            // 使用当前播放速率计算基础帧间隔（毫秒）
            // 如果无法获取帧率，则使用默认值
            int frameInterval = (int)(Info.Rate > 0 ? 1000.0 / Info.Rate : 1000.0 / FFmpegEngine.DefaultFrameRate);
            frameInterval = frameInterval < SkipWaitingTime ? SkipWaitingTime : frameInterval;

            ProcessFrameResult audioResult = new(AudioOutput!, _audioDecoder!, this);
            ProcessFrameResult videoResult = new(VideoOutput!, _videoDecoder!, this);

            ProcessFrameResult mainResult;

            if (!audioResult.IsEmpty)
            {
                mainResult = audioResult;
            }
            else if (!videoResult.IsEmpty)
            {
                mainResult = videoResult;
                videoResult = default; // 视频已作为主时钟处理，此处置空
            }
            else
            {
                throw new ArgumentNullException("没有可用的解码器进行播放。");
            }

            Stopwatch sw = Stopwatch.StartNew();
            long nextPlaybackNotifyTicks = 0;
            long notifyIntervalTicks = frameInterval * Stopwatch.Frequency / 1000;

            while (!token.IsCancellationRequested)
            {
                if (!HasFrames() || isSeeking)
                {
                    Thread.Sleep(1);
                    continue;
                }

                int mainDelay = mainResult.Process(0);
                videoResult.Process(FrameOutTime);

                int mediaInterval = (mainDelay > 0) ? mainDelay : frameInterval;
                Position += mediaInterval;

                // 计算实际等待时间，考虑播放速率影响
                int realWaitTime = (int)(mediaInterval / SpeedRatio);
                if (realWaitTime >= SkipWaitingTime)
                {
                    Thread.Sleep(realWaitTime);
                }
                else if (token.IsCancellationRequested)
                    break;
                else// 极短等待，让出时间片即可
                    Thread.Sleep(1);

                // 节流触发进度回调（按帧率级别触发即可）
                long nowTicks = sw.ElapsedTicks;
                if (nowTicks >= nextPlaybackNotifyTicks)
                {
                    nextPlaybackNotifyTicks = nowTicks + notifyIntervalTicks;
                    Playback?.Invoke();
                }
            }

            sw.Stop();
        }

        /// <summary>
        /// 检查当前是否有可用的音频或视频帧。
        /// </summary>
        /// <returns>如果有任意一个解码器包含帧（或解码器不存在），则返回 true；否则返回 false。</returns>
        private bool HasFrames()
        {
            return HasFrames(_audioDecoder) || HasFrames(_videoDecoder);
        }

        /// <summary>
        /// 检查指定解码器是否有可用帧。
        /// </summary>
        /// <param name="decoder">要检查的解码器。</param>
        /// <returns>如果解码器为空（视为不阻塞）或有帧，则返回 true；否则返回 false。</returns>
        private bool HasFrames(FFmpegDecoder? decoder)
        {
            return decoder is null ? true : decoder.HasFrames;
        }

        #endregion Output Methods

        #region Process Frame Methods

        /// <summary>
        /// 帧处理结果结构体，用于封装解码器与输出目标之间的帧同步逻辑。
        /// </summary>
        private struct ProcessFrameResult
        {
            private readonly IMediaOutput _output;
            private readonly FFmpegDecoder _decoder;
            private readonly MediaPlayer _player;

            /// <summary>
            /// 获取一个值，该值指示此结果是否包含有效的输出和解码器。
            /// </summary>
            public bool IsEmpty => _output == null || _decoder == null;

            /// <summary>
            /// 初始化 <see cref="ProcessFrameResult"/> 结构的新实例。
            /// </summary>
            /// <param name="output">媒体输出接口。</param>
            /// <param name="decoder">FFmpeg 解码器。</param>
            /// <param name="player">媒体播放器实例。</param>
            public ProcessFrameResult(IMediaOutput output, FFmpegDecoder decoder, MediaPlayer player)
            {
                _output = output;
                _decoder = decoder;
                _player = player;
            }

            /// <summary>
            /// 处理帧同步与输出。
            /// 根据当前播放位置与帧的 PTS（显示时间戳）计算时间差，决定是渲染帧、丢弃过期帧还是等待。
            /// </summary>
            /// <param name="outTime">允许的最大输出时间误差（毫秒）。</param>
            /// <returns>当前帧与播放位置的时间差（毫秒）。</returns>
            public int Process(int outTime)
            {
                if (IsEmpty)
                    return 0;

                long timeDiff = 0;
                while (_decoder.HasFrames)
                {
                    timeDiff = _decoder.NextFramePts - _player.Position;

                    if (timeDiff > 0)
                    {
                        break;
                    }
                    else if (Math.Abs(timeDiff) <= outTime)
                    {
                        _decoder.TryDequeueFrame(out var frame);
                        _output.WriteFrame(frame);
                        frame.Dispose();
                    }
                    else if (timeDiff < -outTime)
                    {
                        _decoder.TryDequeueFrame(out var frame);
                        frame.Dispose();
                    }
                    else
                    {
                        break;
                    }
                }
                return (int)timeDiff;
            }
        }

        #endregion Process Frame Methods

        /// <summary>
        /// 释放托管资源，包括销毁解码控制器。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            _controller.Dispose();
        }
    }
}