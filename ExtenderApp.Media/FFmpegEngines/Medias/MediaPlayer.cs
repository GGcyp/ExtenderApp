using System.Diagnostics;
using ExtenderApp.Data;

namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// FFmpeg 媒体播放器。
    /// <para>负责驱动解码（通过 <see cref="IFFmpegDecoderController"/>）、调度音视频帧（通过 <see cref="FrameProcessController"/>），并提供播放控制能力： 播放/暂停/停止/跳转/变速/音量。</para>
    /// <para>播放时钟：使用 <see cref="Stopwatch"/> 作为单调时钟推进 <see cref="Position"/>，并通过代际（generation）机制处理 Seek 后的旧数据丢弃。</para>
    /// </summary>
    public class MediaPlayer : DisposableObject, IMediaPlayer
    {
        /// <summary>
        /// 跳帧等待阈值（毫秒）。
        /// <para>小于该阈值时将采用 <see cref="Thread.Sleep(int)"/> 的最小粒度或不等待，减少抖动。</para>
        /// </summary>
        private const int SkipWaitingTime = 5;

        /// <summary>
        /// 允许输出的最大时间误差（毫秒）。
        /// <para>当某帧 PTS 与当前 <see cref="Position"/> 的差值在该阈值内时输出，否则视为过期帧丢弃。</para>
        /// </summary>
        private const int FrameTimeout = 15;

        /// <summary>
        /// 细等待阈值（毫秒）。
        /// <para>等待末尾使用自旋方式减少 <see cref="Thread.Sleep(int)"/> 过头带来的抖动。</para>
        /// </summary>
        private const int SpinThresholdMs = 2;

        /// <summary>
        /// 解码控制器：负责启动/停止解码任务、Seek、维护 generation 等运行状态。
        /// </summary>
        private readonly IFFmpegDecoderController _decoderController;

        /// <summary>
        /// 暂停同步事件。处于暂停时播放循环阻塞在此事件上，恢复播放时 Set。
        /// </summary>
        private readonly ManualResetEventSlim _pauseEvent;

        /// <summary>
        /// 帧调度控制器：从解码器队列按时间戳挑选可输出帧，并写入对应的输出（音频/视频）。
        /// </summary>
        private readonly IFrameProcessController _frameProcessController;

        /// <summary>
        /// 播放循环后台任务。非空表示已经启动过播放循环。
        /// </summary>
        private Task? mediaTask;

        /// <summary>
        /// 当前解码设置（从控制器读取）。
        /// </summary>
        public FFmpegDecoderSettings Settings => _decoderController.Settings;

        private float volume;
        private double speedRatio;
        private long seekPosition;

        /// <summary>
        /// 当前播放时间（毫秒）。
        /// <para>由播放循环根据 <see cref="Stopwatch"/> 推进，并在 Seek 时直接重置。</para>
        /// </summary>
        public long Position { get; private set; }

        /// <summary>
        /// 音量（0.0-1.0）。
        /// <para>赋值会被裁剪到 [0,1]，并同步到音频输出（若存在）。</para>
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
                var output = FrameProcessCollection.GetMediaOutput(FFmpegMediaType.AUDIO);
                if (output != null && output is IAudioOutput audioOutput)
                {
                    audioOutput.Volume = volume;
                }
            }
        }

        /// <summary>
        /// 播放速率倍率。
        /// <para>1 表示正常速度；最小值限制为 0.05，避免过慢导致时钟推进过小。</para>
        /// <para>设置后会同步到音频输出（若存在）。</para>
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
                var output = FrameProcessCollection.GetMediaOutput(FFmpegMediaType.AUDIO);
                if (output != null && output is IAudioOutput audioOutput)
                {
                    audioOutput.SpeedRatio = speedRatio;
                }
            }
        }

        /// <summary>
        /// 媒体信息（时长、帧率等）。
        /// </summary>
        public FFmpegInfo Info => _decoderController.Info;

        /// <summary>
        /// 播放器状态。
        /// </summary>
        public PlayerState State { get; private set; }

        /// <summary>
        /// 帧处理集合（外部用于注册/替换音视频输出，以及根据类型查询输出）。
        /// </summary>
        public IFrameProcessCollection FrameProcessCollection => _frameProcessController.FrameProcessCollection;

        /// <summary>
        /// 播放进度回调事件（按帧间隔节流触发）。
        /// </summary>
        public event Action? Playback;

        /// <summary>
        /// 构造播放器实例。
        /// </summary>
        /// <param name="decoderController">解码控制器。</param>
        /// <param name="frameProcessController">帧处理控制器。</param>
        public MediaPlayer(IFFmpegDecoderController decoderController, IFrameProcessController frameProcessController)
        {
            State = PlayerState.Uninitialized;
            _decoderController = decoderController;
            _frameProcessController = frameProcessController;
            _pauseEvent = new(true);
            SpeedRatio = 1;
            seekPosition = -1;
            State = PlayerState.Initializing;
        }

        /// <summary>
        /// 开始播放。
        /// <para>首次调用会启动解码并启动播放循环；若此前已启动但处于暂停，则恢复播放。</para>
        /// </summary>
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

            _decoderController.StartDecode();
            mediaTask = Task.Run(PlaybackLoop, _decoderController.Token);
            FrameProcessCollection.PlayerStateChange(State);
        }

        /// <summary>
        /// 停止播放并停止解码。
        /// </summary>
        /// <returns>等待解码停止的异步任务。</returns>
        public ValueTask StopAsync()
        {
            if (State == PlayerState.Stopped || State == PlayerState.Uninitialized)
            {
                return ValueTask.CompletedTask;
            }

            State = PlayerState.Stopped;
            FrameProcessCollection.PlayerStateChange(State);
            return new(_decoderController.StopDecodeAsync());
        }

        /// <summary>
        /// 暂停播放（播放循环阻塞等待恢复）。
        /// </summary>
        public void Pause()
        {
            if (State == PlayerState.Paused || State == PlayerState.Uninitialized)
            {
                return;
            }

            _pauseEvent.Reset();
            State = PlayerState.Paused;
            FrameProcessCollection.PlayerStateChange(State);
        }

        /// <summary>
        /// 跳转到指定时间（TimeSpan）。
        /// </summary>
        public void Seek(TimeSpan timeSpan)
        {
            Seek((long)timeSpan.TotalMilliseconds);
        }

        /// <summary>
        /// 跳转到指定播放位置（毫秒）。
        /// <para>会裁剪到 [0, <see cref="FFmpegInfo.Duration"/>] 并调用控制器 Seek 触发 generation 递增。</para>
        /// </summary>
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

            Interlocked.Exchange(ref seekPosition, position);
            _decoderController.SeekDecoder(position);
        }

        /// <summary>
        /// 播放循环主逻辑：
        /// <list type="bullet">
        /// <item>
        /// <description>使用 <see cref="Stopwatch"/> 推进 <see cref="Position"/>，并乘以 <see cref="SpeedRatio"/> 实现变速。</description>
        /// </item>
        /// <item>
        /// <description>通过 decoderController 的 generation 识别 Seek，重置基准时间点。</description>
        /// </item>
        /// <item>
        /// <description>调用 <see cref="FrameProcessController.Processing(int, long, int)"/> 处理音视频帧，并返回距离下一帧的剩余时间以决定等待策略。</description>
        /// </item>
        /// <item>
        /// <description>等待策略：Sleep(粗) + 自旋(细) 以降低抖动。</description>
        /// </item>
        /// <item>
        /// <description>按帧间隔节流触发 <see cref="Playback"/> 回调。</description>
        /// </item>
        /// </list>
        /// </summary>
        private void PlaybackLoop()
        {
            var token = _decoderController.Token;

            /// 帧间隔计算：使用 Info.Rate，若无效则使用默认值。
            int frameInterval = (int)(Info.Rate > 0 ? 1000.0 / Info.Rate : 1000.0 / FFmpegEngine.DefaultFrameRate);
            if (frameInterval <= 0)
                frameInterval = FrameTimeout;

            long lastPosition = Position;
            long basePositionMs = lastPosition;
            int generation = _decoderController.GetCurrentGeneration();
            long lastTicks = 0;
            long notifyIntervalTicks = frameInterval * Stopwatch.Frequency / 1000;

            _frameProcessController.WaitFirstFrameAligned(generation, frameInterval, out long pts);
            Position = lastPosition = pts;
            var sw = Stopwatch.StartNew();
            while (!token.IsCancellationRequested)
            {
                if (WaitPause(token))
                {
                    basePositionMs = Position;
                    lastTicks = 0;
                    sw.Restart();
                    continue;
                }

                int currentGeneration = _decoderController.GetCurrentGeneration();
                long position = Position;
                if (generation != currentGeneration)
                {
                    long pendingSeek = Interlocked.Exchange(ref seekPosition, -1);
                    if (pendingSeek >= 0)
                        basePositionMs = position = pendingSeek;
                    else
                        basePositionMs = position;

                    lastPosition = position;
                    generation = currentGeneration;
                    lastTicks = 0;
                    sw.Restart();
                }
                else
                {
                    long elapsedMs = (long)(sw.Elapsed.TotalMilliseconds * SpeedRatio);
                    position = basePositionMs + elapsedMs;
                }

                int waitDelay = _frameProcessController.Processing(frameInterval, position, generation);
                //Debug.Print($"PlaybackLoop: pos={position} waitDelay={waitDelay} gen={generation}");
                if (waitDelay < 0)
                {
                    if (_decoderController.Completed)
                    {
                        break;
                    }
                    Position = basePositionMs = lastPosition;
                    Thread.Sleep(1);
                    sw.Restart();
                    continue;
                }
                Position = lastPosition = position;

                int waitMs = (waitDelay > 0) ? (int)(waitDelay / SpeedRatio) : frameInterval;
                if (waitMs >= SkipWaitingTime)
                {
                    SleepWithFineTuning(token, waitMs);
                }
                else if (waitMs >= 0)
                {
                    Thread.Sleep(1);
                }

                if (sw.ElapsedTicks - lastTicks >= notifyIntervalTicks)
                {
                    lastTicks = sw.ElapsedTicks;
                    Playback?.Invoke();
                }
            }

            Position = Info.Duration;
            Playback?.Invoke();

#if DEBUG
            Debug.Print("PlaybackLoop: exited");
#endif
        }

        /// <summary>
        /// 若当前处于暂停状态，则阻塞等待直到恢复或取消。
        /// </summary>
        /// <returns>若发生过等待则返回 true，否则返回 false。</returns>
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
        /// 混合等待：先 Sleep 大部分时间以释放 CPU，末尾再自旋短时间以减少 Sleep 过头导致的抖动。
        /// </summary>
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
        /// 释放托管资源：暂停事件与解码控制器。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            _pauseEvent.Dispose();
            _decoderController.Dispose();
        }
    }
}