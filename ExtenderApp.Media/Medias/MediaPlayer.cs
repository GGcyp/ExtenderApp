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

        public IAudioOutput? AudioOutput { get; private set; }

        public IVideoOutput? VideoOutput { get; private set; }

        /// <summary>
        /// 解码器设置参数。
        /// </summary>
        public FFmpegDecoderSettings Settings { get; }

        /// <summary>
        /// 当前播放时间（微秒）。
        /// </summary>
        public long Position { get; private set; }

        /// <summary>
        /// 播放速率，1表示正常速度，2表示两倍速，0.5表示半速。
        /// </summary>
        public double Rate { get; set; }

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
            Rate = 1;
            State = PlayerState.Initializing;

            var collection = _controller.DecoderCollection;
            _videoDecoder = collection.GetDecoder(FFmpegMediaType.VIDEO);
            _audioDecoder = collection.GetDecoder(FFmpegMediaType.AUDIO);
        }

        public void SetAudioOutput(IAudioOutput output)
        {
            if (mediaTask != null)
                throw new InvalidOperationException("在播放过程中无法更改音频输出。");

            AudioOutput = output;
        }

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
        /// 停止媒体播放流程。 以同步方式调用 StopAsync，终止解码和播放任务，释放相关资源并清空帧队列。
        /// </summary>
        public void Stop()
        {
            // 修正：同步等待异步方法完成，以确保在 Dispose 期间正确清理。
            StopAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 停止媒体播放流程，终止解码和播放任务，释放相关资源并清空帧队列。 若当前已处于停止状态则直接返回。
        /// </summary>
        public ValueTask StopAsync()
        {
            if (State == PlayerState.Stopped)
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
        /// 暂停媒体播放流程。 以同步方式调用 PauseAsync，终止解码和播放任务，释放相关资源。
        /// </summary>
        public void Pause()
        {
            // 修正：同步等待异步方法完成。
            PauseAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 暂停媒体播放流程，终止解码和播放任务，释放相关资源。 若当前已处于暂停状态则直接返回。
        /// </summary>
        public ValueTask PauseAsync()
        {
            if (State == PlayerState.Paused)
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
        public ValueTask SeekAsync(TimeSpan timeSpan)
        {
            return SeekAsync((long)timeSpan.TotalMilliseconds);
        }

        /// <summary>
        /// 跳转到指定时间点（毫秒），重置解码器和播放状态，清空帧队列并触发进度回调。 若未初始化则直接返回，跳转超出范围时自动修正。
        /// </summary>
        /// <param name="position">目标跳转时间（毫秒）。</param>
        public async ValueTask SeekAsync(long position)
        {
            if (State == PlayerState.Uninitialized)
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
            await Task.WhenAll(_controller.SeekDecoderAsync(position), FlushAsync()).ConfigureAwait(false);

            Position = position;
            Playback?.Invoke();          // 触发进度回调
            State = PlayerState.Initializing;
        }

        /// <summary>
        /// 释放媒体播放任务相关资源，取消并等待播放任务完成，释放取消令牌。 若播放任务不存在则直接返回。
        /// </summary>
        private async Task FlushAsync()
        {
            mediaSource?.Cancel();
            mediaSource?.Dispose();
            mediaSource = null;
            if (mediaTask == null)
            {
                return;
            }

            Task task = mediaTask;
            mediaTask = null;

            try
            {
                await task.ConfigureAwait(false);      // 等待播放任务完成
                task.Dispose();  // 释放任务资源
            }
            catch (TaskCanceledException)
            {
                // 忽略任务取消异常
            }
        }

        /// <summary>
        /// 媒体播放主循环任务，负责音视频帧的调度与播放。
        /// </summary>
        private void PlaybackLoop()
        {
            const int ResultCount = 1;

            var token = mediaSource!.Token;

            // 使用当前播放速率计算基础帧间隔（毫秒）
            int frameInterval = (int)(Rate > 0 ? 1000.0 / Rate : 1000.0 / FFmpegEngine.DefaultFrameRate);

            // 选择主解码器（优先音频）
            FFmpegDecoder? mainDecoder = _audioDecoder ?? _videoDecoder;

            ProcessFrameResult audioResult = new(AudioOutput!, _audioDecoder!, this);
            ProcessFrameResult videoResult = new(VideoOutput!, _videoDecoder!, this);

            ProcessFrameResult mainResult = audioResult.IsEmpty ? videoResult : audioResult;
            List<ProcessFrameResult>? otherResults = new(ResultCount);
            if (mainResult.IsEmpty)
            {
                throw new ArgumentNullException("没有可用的解码器进行播放。");
            }
            else
            {
            }

            while (!token.IsCancellationRequested)
            {
                // 如果两个流都没有帧，等待上一次的延时然后继续循环
                if (!HasFrames())
                {
                    Thread.Sleep(frameInterval);
                    continue;
                }

                int mainDelay = mainResult.Process(0);
                videoResult.Process(FrameOutTime);

                // 计算下一次等待时间
                int waitTime = (mainDelay > 0) ? mainDelay : frameInterval;

                // 推进播放时间（以毫秒为单位）
                Position += waitTime;

                // 根据播放速率缩放实际等待时间
                int scaledWait = Math.Max(0, (int)(waitTime / Rate));
                Playback?.Invoke();

                if (scaledWait >= SkipWaitingTime)
                {
                    Thread.Sleep(scaledWait);
                }
                else
                {
                    // 若等待时间过短，yield 给线程池以避免 busy-loop
                    Thread.Yield();
                }
            }
        }

        private bool HasFrames()
        {
            return HasFrames(_audioDecoder) || HasFrames(_videoDecoder);
        }

        private bool HasFrames(FFmpegDecoder? decoder)
        {
            return decoder is null ? true : decoder.HasFrames;
        }

        #endregion Output Methods

        #region Process Frame Methods

        private struct ProcessFrameResult
        {
            private readonly IMediaOutput _output;
            private readonly FFmpegDecoder _decoder;
            private readonly MediaPlayer _player;

            public bool IsEmpty => _output == null || _decoder == null;

            public ProcessFrameResult(IMediaOutput output, FFmpegDecoder decoder, MediaPlayer player)
            {
                _output = output;
                _decoder = decoder;
                _player = player;
            }

            public int Process(int outTime)
            {
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

        protected override void DisposeManagedResources()
        {
            _controller.Dispose();
        }
    }
}