using System.Diagnostics;
using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines.Decoders;

namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// FFmpeg 媒体播放器，负责音视频解码、播放、暂停、停止、跳转等控制。
    /// 支持多线程解码和帧调度，适用于 WPF 播放场景。
    /// </summary>
    public class MediaPlayer : DisposableObject
    {
        /// <summary>
        /// 跳帧等待阈值（毫秒），用于控制播放节奏。
        /// 小于该阈值的等待采用 <see cref="Thread.Sleep(int)"/> 的最小粒度或不等待，从而减少抖动。
        /// </summary>
        private const int SkipWaitingTime = 5;

        /// <summary>
        /// 每个视频帧的最大允许输出时间差（毫秒），用于同步音视频。
        /// 当下一帧时间戳与当前位置误差在此阈值内时输出，否则丢弃过期帧。
        /// </summary>
        private const int FrameTimeout = 15;

        /// <summary>
        /// 细等待阈值（毫秒）。当剩余等待时间很短时使用自旋来降低 Sleep(1) 带来的抖动。
        /// 建议在 1-3ms 之间权衡 CPU 与定时精度。
        /// </summary>
        private const int SpinThresholdMs = 2;

        /// <summary>
        /// 解码控制器，负责解码流程的启动、停止和跳转。
        /// </summary>
        private readonly FFmpegDecoderController _controller;

        /// <summary>
        /// 视频画面解码器实例（可为空，表示无视频流）。
        /// </summary>
        private readonly FFmpegDecoder? _videoDecoder;

        /// <summary>
        /// 音频解码器实例（可为空，表示无音频流）。
        /// </summary>
        private readonly FFmpegDecoder? _audioDecoder;

        private readonly ManualResetEventSlim _pauseEvent;


        private readonly FrameProcessController _frameProcessController;

        /// <summary>
        /// 媒体播放任务（后台循环），通过 <see cref="Task.Run(System.Action)"/> 启动。
        /// </summary>
        private Task? mediaTask;

        /// <summary>
        /// 声音输出接口实例。
        /// 通过 <see cref="SetAudioOutput(IAudioOutput)"/> 设置，播放过程中不可替换。
        /// </summary>
        public IAudioOutput? AudioOutput { get; private set; }

        /// <summary>
        /// 视频输出接口实例。
        /// 通过 <see cref="SetVideoOutput(IVideoOutput)"/> 设置，播放过程中不可替换。
        /// </summary>
        public IVideoOutput? VideoOutput { get; private set; }

        /// <summary>
        /// 解码器设置参数。
        /// </summary>
        public FFmpegDecoderSettings Settings => _controller.Settings;

        /// <summary>
        /// 当前播放时间（毫秒）。
        /// 注意：该值需与 <see cref="FFmpegDecoder.NextFramePts"/> 的单位保持一致。
        /// </summary>
        public long Position { get; private set; }

        private float volume;

        /// <summary>
        /// 音量（0.0-1.0）。越界时将被裁剪为 0 或 1。
        /// 设置时若已绑定音频输出会同步更新输出音量。
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
        /// 播放速率，1 表示正常速度，2 表示两倍速，0.5 表示半速。
        /// 最小值为 0.05，防止接近 0 导致时钟推进过慢。
        /// 设置时若已绑定音频输出会同步更新输出速率。
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

        /// <summary>
        /// 每次播放进度更新时触发（节流至帧率级别）。
        /// 仅在播放循环中调度；暂停或停止不触发。
        /// </summary>
        public event Action? Playback;

        /// <summary>
        /// 初始化 MediaPlayer 实例。
        /// </summary>
        /// <param name="contriller">解码控制器。</param>
        /// <param name="settings">解码器设置。</param>
        public MediaPlayer(FFmpegDecoderController contriller)
        {
            State = PlayerState.Uninitialized;
            _controller = contriller;
            SpeedRatio = 1;
            _pauseEvent = new(true);
            State = PlayerState.Initializing;
            _frameProcessController = new(_controller.DecoderCollection);
        }

        /// <summary>
        /// 设置音频输出接口。
        /// </summary>
        /// <param name="output">音频输出实现。</param>
        /// <exception cref="InvalidOperationException">当播放任务已启动时抛出。</exception>
        public void SetAudioOutput(IAudioOutput output)
        {
            if (mediaTask != null)
                throw new InvalidOperationException("在播放过程中无法更改音频输出。");

            AudioOutput = output;
        }

        /// <summary>
        /// 设置视频输出接口。
        /// </summary>
        /// <param name="output">视频输出实现。</param>
        /// <exception cref="InvalidOperationException">当播放任务已启动时抛出。</exception>
        public void SetVideoOutput(IVideoOutput output)
        {
            if (mediaTask != null)
                throw new InvalidOperationException("在播放过程中无法更改视频输出。");

            VideoOutput = output;
        }

        /// <summary>
        /// 启动媒体播放流程，开始解码和播放。
        /// 如果当前已在播放则直接返回。
        /// </summary>
        /// <remarks>
        /// - 会调用解码控制器启动解码，并创建与控制器令牌链接的取消源。
        /// - 在后台线程启动播放循环。
        /// - 同步通知音视频输出状态变更。
        /// </remarks>
        public void Play()
        {
            if (State == PlayerState.Playing || State == PlayerState.Uninitialized)
            {
                return;
            }
            else if (mediaTask != null)
            {
                _pauseEvent.Set();
                State = PlayerState.Playing;
                return;
            }
            else if (Position != 0)
            {
                Seek(Position);
            }
            State = PlayerState.Playing;

            _controller.StartDecode();
            mediaTask = Task.Run(PlaybackLoop, _controller.Token);
            AudioOutput?.PlayerStateChange(State);
            VideoOutput?.PlayerStateChange(State);
        }

        /// <summary>
        /// 停止媒体播放流程，终止解码和播放任务。
        /// </summary>
        /// <returns>用于等待停止完成的异步结果。</returns>
        /// <remarks>
        /// - 若状态为已停止或未初始化，直接完成不执行任何操作。
        /// - 此方法会并行停止解码并刷新播放循环，随后释放播放任务资源。
        /// - 同步通知音视频输出状态变更。
        /// </remarks>
        public ValueTask StopAsync()
        {
            if (State == PlayerState.Stopped || State == PlayerState.Uninitialized)
            {
                return ValueTask.CompletedTask;
            }

            State = PlayerState.Stopped;
            AudioOutput?.PlayerStateChange(State);
            VideoOutput?.PlayerStateChange(State);
            Position = 0;
            return new(_controller.StopDecodeAsync());
        }

        /// <summary>
        /// 暂停媒体播放流程。
        /// </summary>
        /// <returns>用于等待暂停完成的异步结果。</returns>
        /// <remarks>
        /// - 若状态为已暂停或未初始化，直接完成不执行任何操作。
        /// - 此方法会并行停止解码并刷新播放循环，随后释放播放任务资源。
        /// - 同步通知音视频输出状态变更。
        /// </remarks>
        public void Pause()
        {
            if (State == PlayerState.Paused || State == PlayerState.Uninitialized)
            {
                return;
            }

            _pauseEvent.Reset();
            State = PlayerState.Paused;
            AudioOutput?.PlayerStateChange(State);
            VideoOutput?.PlayerStateChange(State);
        }

        /// <summary>
        /// 跳转到指定时间。
        /// </summary>
        /// <param name="timeSpan">要跳转到的时间。</param>
        /// <remarks>内部换算为毫秒传递至 <see cref="Seek(long)"/>。</remarks>
        public void Seek(TimeSpan timeSpan)
        {
            Seek((long)timeSpan.TotalMilliseconds);
        }

        /// <summary>
        /// 跳转到指定位置（毫秒）。
        /// </summary>
        /// <param name="position">要跳转到的位置（毫秒）。超出范围将被裁剪至 [0, <see cref="FFmpegInfo.Duration"/>]。</param>
        /// <remarks>
        /// - 未初始化或位置未变化时不执行任何操作。
        /// - 跳转期间置位 <see cref="isPauseLoop"/>，重置主时钟基准，触发一次 <see cref="Playback"/> 更新以便 UI 同步。
        /// - 跳转完成后状态回到初始化中以等待解码器就绪。
        /// </remarks>
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

            Position = position;
            _controller.SeekDecoder(position);
        }

        /// <summary>
        /// 媒体播放主循环任务（强对齐版）：
        /// - 以 Stopwatch 为单调主时钟，将 Position 强制对齐到“起点 + 实际经过时间 * SpeedRatio”
        /// - 等待策略：Sleep(粗) + 最后 2ms 自旋(细)，避免长忙等同时减少 Sleep 过头抖动
        /// - Playback 回调按帧间隔节流
        /// </summary>
        /// <remarks>
        /// 后台线程运行。循环依赖 <see cref="mediaSource"/> 的取消令牌退出。
        /// 若音频可用则以音频为主时钟，否则降级为视频。
        /// </remarks>
        private void PlaybackLoop()
        {
            var token = _controller.Token;

            int frameInterval = (int)(Info.Rate > 0 ? 1000.0 / Info.Rate : 1000.0 / FFmpegEngine.DefaultFrameRate);
            if (frameInterval <= 0)
                frameInterval = FrameTimeout;

            //ProcessFrameResult audioResult = new(AudioOutput!, _audioDecoder!);
            //ProcessFrameResult videoResult = new(VideoOutput!, _videoDecoder!);

            //ProcessFrameResult mainResult;

            //if (!audioResult.IsEmpty)
            //{
            //    mainResult = audioResult;
            //}
            //else if (!videoResult.IsEmpty)
            //{
            //    mainResult = videoResult;
            //    videoResult = default;
            //}
            //else
            //{
            //    throw new ArgumentNullException("没有可用的解码器进行播放。");
            //}

            long basePositionMs = Position;
            var sw = Stopwatch.StartNew();
            int generation = _controller.GetCurrentGeneration();

            long nextPlaybackNotifyTicks = 0;
            long notifyIntervalTicks = frameInterval * Stopwatch.Frequency / 1000;

            while (!token.IsCancellationRequested)
            {
                if (WaitPause(token))
                {
                    sw.Restart();
                    continue;
                }

                int currentGeneration = _controller.GetCurrentGeneration();
                long lastPosition = Position;
                long position = lastPosition;
                if (generation != currentGeneration)
                {
                    basePositionMs = position;
                    generation = currentGeneration;
                    sw.Restart();
                }
                else
                {
                    // 强对齐：以真实时间推进 Position（倍率影响）
                    long elapsedMs = (long)(sw.Elapsed.TotalMilliseconds * SpeedRatio);
                    position = basePositionMs + elapsedMs;
                    Position = position;
                }

                //int mainDelay = mainResult.Process(frameInterval, position, generation);
                //videoResult.Process(frameInterval, position, generation);

                int mainDelay = 0; // TODO: 先注释掉音视频输出相关代码以便测试播放循环逻辑
                if (mainDelay < 0)
                {
                    basePositionMs = lastPosition;
                    sw.Restart();
                    Thread.Sleep(frameInterval);
                    continue;
                }

                // mainDelay > 0：下一帧还未到时间；<=0：按照帧率给一个节奏下限
                int waitMs = (mainDelay > 0) ? (int)(mainDelay / SpeedRatio) : frameInterval;
                if (waitMs >= SkipWaitingTime)
                {
                    SleepWithFineTuning(token, waitMs);
                }
                else if (waitMs >= 0)
                {
                    Thread.Sleep(1);
                }

                // 节流触发进度回调（按帧率级别触发即可）
                long nowTicks = sw.ElapsedTicks;
                if (nowTicks >= nextPlaybackNotifyTicks)
                {
                    nextPlaybackNotifyTicks = nowTicks + notifyIntervalTicks;
                    Playback?.Invoke();
                }
            }
        }

        private bool WaitPause(CancellationToken token)
        {
            if (!_pauseEvent.IsSet)
            {
                _pauseEvent.Wait(token);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 混合等待：大部分时间用 Sleep 释放 CPU，最后短窗口用自旋减少 Sleep 过头导致的抖动（零分配）。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <param name="waitMs">等待的毫秒数。</param>
        private static void SleepWithFineTuning(CancellationToken token, int waitMs)
        {
            if (waitMs <= 0)
                return;

            int sleepMs = waitMs - SpinThresholdMs;
            if (sleepMs > 0)
            {
                Thread.Sleep(sleepMs);
            }

            if (token.IsCancellationRequested || SpinThresholdMs <= 0)
                return;

            long start = Stopwatch.GetTimestamp();
            long thresholdTicks = SpinThresholdMs * Stopwatch.Frequency / 1000;
            int count = 0;

            while (!token.IsCancellationRequested &&
                (Stopwatch.GetTimestamp() - start) < thresholdTicks &&
                count < 10)
            {
                Thread.SpinWait(30);
                count++;
            }
        }

        /// <summary>
        /// 释放由 <see cref="MediaPlayer"/> 占用的托管资源。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            _pauseEvent.Dispose();
            _controller.Dispose();
        }
    }
}