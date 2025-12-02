using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines.Decoders;

namespace ExtenderApp.FFmpegEngines
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
        private const int VideoFrameOutTime = 15;

        /// <summary>
        /// 解码控制器，负责解码流程的启动、停止和跳转。
        /// </summary>
        private readonly FFmpegDecoderController _controller;

        /// <summary>
        /// 全局取消令牌源，用于控制播放流程的终止。
        /// </summary>

        private readonly CancellationTokenSource _allSource;

        private readonly FFmpegDecoder? _videoDecoder;
        private readonly FFmpegDecoder? _audioDecoder;

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
        /// 媒体播放任务。
        /// </summary>
        private Task? mediaTask;

        /// <summary>
        /// 媒体播放流程的取消令牌源。
        /// </summary>
        private CancellationTokenSource? mediaSource;

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
        /// 视频帧回调事件，每次播放视频帧时触发。
        /// </summary>
        public event Action<FFmpegFrame>? VideoFrameReceived;

        /// <summary>
        /// 音频帧回调事件，每次播放音频帧时触发。
        /// </summary>
        public event Action<FFmpegFrame>? AudioFrameReceived;

        /// <summary>
        /// 每次播放进度更新时触发，传递当前播放时间（微秒）。
        /// </summary>
        public event Action<long>? PlaybackReceived;

        #endregion Events

        /// <summary>
        /// 初始化 MediaPlayer 实例。
        /// </summary>
        /// <param name="contriller">解码控制器。</param>
        /// <param name="allSource">全局取消令牌源。</param>
        /// <param name="settings">解码器设置。</param>
        public MediaPlayer(FFmpegDecoderController contriller, CancellationTokenSource allSource, FFmpegDecoderSettings settings)
        {
            State = PlayerState.Uninitialized;
            _controller = contriller;
            _allSource = allSource;
            Settings = settings;
            Rate = 1;
            State = PlayerState.Initializing;

            var collection = _controller.DecoderCollection;
            _videoDecoder = collection.GetDecoder(FFmpegMediaType.VIDEO);
            _audioDecoder = collection.GetDecoder(FFmpegMediaType.AUDIO);
        }

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
            mediaTask = Task.Run(PlaybackLoop, _allSource.Token);
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
        public async Task StopAsync()
        {
            if (State == PlayerState.Stopped)
            {
                return;
            }

            await _controller.StopDecodeAsync(); // 停止解码流程
            State = PlayerState.Stopped;
            await FlushAsync();                // 释放播放任务资源
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
        public async Task PauseAsync()
        {
            if (State == PlayerState.Paused)
            {
                return;
            }

            await _controller.StopDecodeAsync(); // 停止解码流程
            State = PlayerState.Paused;
            await FlushAsync();                // 释放播放任务资源
        }

        /// <summary>
        /// 跳转到指定时间点（TimeSpan），内部按毫秒处理。
        /// </summary>
        /// <param name="timeSpan">目标跳转时间（TimeSpan）。</param>
        public async Task SeekAsync(TimeSpan timeSpan)
        {
            await SeekAsync((long)timeSpan.TotalMilliseconds);
        }

        /// <summary>
        /// 跳转到指定时间点（毫秒），重置解码器和播放状态，清空帧队列并触发进度回调。 若未初始化则直接返回，跳转超出范围时自动修正。
        /// </summary>
        /// <param name="position">目标跳转时间（毫秒）。</param>
        public async Task SeekAsync(long position)
        {
            if (State == PlayerState.Uninitialized)
            {
                return;
            }

            if (position > Info.Duration)
            {
                position = Info.Duration;
            }
            else if (position < 0)
            {
                position = 0;
            }
            await FlushAsync();                   // 释放播放任务资源
            await _controller.SeekDecoderAsync(position); // 跳转解码器

            Position = position;
            PlaybackReceived?.Invoke(Position);          // 触发进度回调
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
            var allToken = _allSource.Token;
            mediaSource = CancellationTokenSource.CreateLinkedTokenSource(allToken);
            var token = mediaSource.Token;

            // 使用当前播放速率计算基础帧间隔（毫秒）
            int frameInterval = (int)(Rate > 0 ? 1000.0 / Rate : 1000.0 / FFmpegEngine.DefaultFrameRate);
            int lastDelay = frameInterval;

            // 选择主解码器（优先音频）
            FFmpegDecoder? mainDecoder = _audioDecoder ?? _videoDecoder;
            if (mainDecoder is null)
            {
                // 无可用解码器，直接返回
                return;
            }

            Action<FFmpegFrame>? mainAction = mainDecoder == _audioDecoder ? AudioFrameReceived : VideoFrameReceived;

            while (!token.IsCancellationRequested)
            {
                // 如果两个流都没有帧，等待上一次的延时然后继续循环
                if (!HasFrames())
                {
                    Thread.Sleep(lastDelay);
                    continue;
                }

                // 处理主流（音频优先），返回与当前播放位置的时间差（毫秒）
                int mainDelay = ProcessMainFrame(mainDecoder, mainAction);

                // 处理非主流的帧（如果存在）
                ProcessFrame(_videoDecoder, VideoFrameReceived, VideoFrameOutTime);

                // 计算下一次等待时间：优先使用主流返回的时间差（若为正），否则根据是否有音频选择 audioDelay 或 frameInterval
                int waitTime = (mainDelay > 0) ? mainDelay : frameInterval;

                // 推进播放时间（以毫秒为单位）
                Position += waitTime;

                // 根据播放速率缩放实际等待时间
                int scaledWait = Math.Max(0, (int)(waitTime / Rate));
                PlaybackReceived?.Invoke(Position);

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

        private int ProcessMainFrame(FFmpegDecoder? decoder, Action<FFmpegFrame>? action)
        {
            if (decoder is null || !decoder.HasFrames)
            {
                return -1;
            }

            long timeDiff = 0;
            while (decoder.HasFrames)
            {
                timeDiff = decoder.NextFramePts - Position;

                if (timeDiff <= 0)
                {
                    decoder.TryDequeueFrame(out var frame);
                    action?.Invoke(frame);
                    frame.Dispose();
                }
                else
                {
                    break;
                }
            }
            return (int)timeDiff;
        }

        private int ProcessFrame(FFmpegDecoder? decoder, Action<FFmpegFrame>? action, int outTime)
        {
            if (decoder is null || !decoder.HasFrames)
            {
                return -1;
            }

            long timeDiff = 0;
            while (decoder.HasFrames)
            {
                timeDiff = decoder.NextFramePts - Position;

                if (Math.Abs(timeDiff) <= outTime)
                {
                    decoder.TryDequeueFrame(out var frame);
                    action?.Invoke(frame);
                    frame.Dispose();
                }
                else if (timeDiff < -outTime)
                {
                    decoder.TryDequeueFrame(out var frame);
                    frame.Dispose();
                }
                else
                {
                    break;
                }
            }
            return (int)timeDiff;
        }

        protected override void DisposeManagedResources()
        {
            _controller.Dispose();
        }
    }
}