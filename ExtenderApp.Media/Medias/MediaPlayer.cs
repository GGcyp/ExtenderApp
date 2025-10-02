using System.Collections.Concurrent;
using ExtenderApp.Common;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 媒体播放器，负责音视频解码、播放、暂停、停止、跳转等控制。
    /// 支持多线程解码和帧调度，适用于 WPF 播放场景。
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
        /// 解码器设置参数。
        /// </summary>
        public FFmpegDecoderSettings Settings { get; }

        /// <summary>
        /// 视频帧队列，缓存待播放的视频帧。
        /// </summary>
        private readonly ConcurrentQueue<VideoFrame> _videoFrameQueue;

        /// <summary>
        /// 音频帧队列，缓存待播放的音频帧。
        /// </summary>
        private readonly ConcurrentQueue<AudioFrame> _audioFrameQueue;

        /// <summary>
        /// 解码控制器，负责解码流程的启动、停止和跳转。
        /// </summary>
        private readonly FFmpegDecoderController _controller;

        /// <summary>
        /// 全局取消令牌源，用于控制播放流程的终止。
        /// </summary>
        private readonly CancellationTokenSource _allSource;

        /// <summary>
        /// 当前播放时间（微秒）。
        /// </summary>
        public long Position { get; private set; }

        /// <summary>
        /// 播放速率，1表示正常速度，2表示两倍速，0.5表示半速。
        /// </summary>
        public double RateSpeed { get; set; }

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
        public event Action<VideoFrame>? OnVideoFrame;

        /// <summary>
        /// 音频帧回调事件，每次播放音频帧时触发。
        /// </summary>
        public event Action<AudioFrame>? OnAudioFrame;

        /// <summary>
        /// 每次播放进度更新时触发，传递当前播放时间（微秒）。
        /// </summary>
        public event Action<long>? OnPlayback;

        #endregion Events

        /// <summary>
        /// 初始化 MediaPlayer 实例。
        /// </summary>
        /// <param name="collection">解码控制器。</param>
        /// <param name="allSource">全局取消令牌源。</param>
        /// <param name="settings">解码器设置。</param>
        public MediaPlayer(FFmpegDecoderController collection, CancellationTokenSource allSource, FFmpegDecoderSettings settings)
        {
            State = PlayerState.Uninitialized;
            _videoFrameQueue = new();
            _audioFrameQueue = new();
            _controller = collection;
            _allSource = allSource;
            Settings = settings;
            settings.VideoScheduling += VideoSchedule;
            settings.AudioScheduling += AudioSchedule;
            RateSpeed = 1;
            State = PlayerState.Initializing;
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
        /// 停止媒体播放流程。
        /// 以异步方式调用 StopAsync，终止解码和播放任务，释放相关资源并清空帧队列。
        /// </summary>
        public void Stop()
        {
            Task.Run(StopAsync);
        }

        /// <summary>
        /// 停止媒体播放流程，终止解码和播放任务，释放相关资源并清空帧队列。
        /// 若当前已处于停止状态则直接返回。
        /// </summary>
        public async Task StopAsync()
        {
            if (State == PlayerState.Stopped)
            {
                return;
            }

            await _controller.StopDecodeAsync(); // 停止解码流程
            State = PlayerState.Stopped;
            await ReleaseAsync();                // 释放播放任务资源
            Clear();                             // 清空音视频帧队列
        }

        /// <summary>
        /// 暂停媒体播放流程。
        /// 以异步方式调用 PauseAsync，终止解码和播放任务，释放相关资源。
        /// </summary>
        public void Pause()
        {
            Task.Run(PauseAsync);
        }

        /// <summary>
        /// 暂停媒体播放流程，终止解码和播放任务，释放相关资源。
        /// 若当前已处于暂停状态则直接返回。
        /// </summary>
        public async Task PauseAsync()
        {
            if (State == PlayerState.Paused)
            {
                return;
            }

            await _controller.StopDecodeAsync(); // 停止解码流程
            State = PlayerState.Paused;
            await ReleaseAsync();                // 释放播放任务资源
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
        /// 跳转到指定时间点（毫秒），重置解码器和播放状态，清空帧队列并触发进度回调。
        /// 若未初始化则直接返回，跳转超出范围时自动修正。
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
            await ReleaseAsync();                   // 释放播放任务资源
            await _controller.SeekDecoderAsync(position); // 跳转解码器

            Position = position;
            Clear();                               // 清空音视频帧队列
            OnPlayback?.Invoke(Position);          // 触发进度回调
            State = PlayerState.Initializing;
        }

        /// <summary>
        /// 释放媒体播放任务相关资源，取消并等待播放任务完成，释放取消令牌。
        /// 若播放任务不存在则直接返回。
        /// </summary>
        private async Task ReleaseAsync()
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

            await task;      // 等待播放任务完成
            task.Dispose();  // 释放任务资源

            //try
            //{
            //    Task.WaitAll(task);
            //    task.Dispose();
            //}
            //catch (AggregateException ex)
            //{
            //    // 忽略任务取消异常
            //}
        }

        /// <summary>
        /// 媒体播放主循环任务，负责音视频帧的调度与播放。
        /// </summary>
        private async Task PlaybackLoop()
        {
            var allToken = _allSource.Token;
            mediaSource = mediaSource ?? CancellationTokenSource.CreateLinkedTokenSource(allToken);
            var token = mediaSource.Token;
            int frameInterval = (int)(Info.Rate > 0 ? 1000.0 / Info.Rate : 25); // 毫秒
            int lastDelay = frameInterval;

            while (!token.IsCancellationRequested && !allToken.IsCancellationRequested)
            {
                bool hasAudio = !_audioFrameQueue.IsEmpty;
                bool hasVideo = !_videoFrameQueue.IsEmpty;
                if (!hasAudio && !hasVideo)
                {
                    try
                    {
                        await Task.Delay(lastDelay, token);
                    }
                    catch (TaskCanceledException ex)
                    {

                    }
                    continue;
                }

                int audioDelay = 0;
                while (_audioFrameQueue.TryPeek(out var audioFrame))
                {
                    long timeDiff = audioFrame.Pts - Position;

                    if (timeDiff <= 0)
                    {
                        _audioFrameQueue.TryDequeue(out audioFrame);
                        OnAudioFrame?.Invoke(audioFrame);
                        _controller.OnAudioFrameRemoved();

                        Position = audioFrame.Pts;

                        audioFrame.Dispose();
                    }
                    else
                    {
                        audioDelay = (int)timeDiff;
                        break;
                    }
                }

                while (_videoFrameQueue.TryPeek(out var videoFrame))
                {
                    long videoTimeDiff = videoFrame.Pts - Position;

                    if (Math.Abs(videoTimeDiff) <= VideoFrameOutTime)
                    {
                        _videoFrameQueue.TryDequeue(out videoFrame);
                        OnVideoFrame?.Invoke(videoFrame);
                        _controller.OnVideoFrameRemoved();
                        videoFrame.Dispose();
                    }
                    else if (videoTimeDiff < -VideoFrameOutTime)
                    {
                        _videoFrameQueue.TryDequeue(out videoFrame);
                        _controller.OnVideoFrameRemoved();
                        videoFrame.Dispose();
                    }
                    else
                    {
                        break;
                    }
                }

                int waitTime = hasAudio ? audioDelay : frameInterval;
                Position += waitTime;
                lastDelay = waitTime;
                waitTime = (int)(waitTime / RateSpeed);
                OnPlayback?.Invoke(Position);

                if (waitTime >= SkipWaitingTime)
                {
                    try
                    {
                        await Task.Delay(waitTime, token);
                    }
                    catch (TaskCanceledException ex)
                    {

                    }
                }
            }
        }

        /// <summary>
        /// 音频帧调度回调，将音频帧加入队列并通知解码器。
        /// </summary>
        /// <param name="audioFrame">待调度的音频帧。</param>
        private void AudioSchedule(AudioFrame audioFrame)
        {
            if (audioFrame.IsEmpty)
                throw new Exception("传入音频帧不能为空");

            _audioFrameQueue.Enqueue(audioFrame);
            _controller.OnAudioFrameAdded();
        }

        /// <summary>
        /// 视频帧调度回调，将视频帧加入队列并通知解码器。
        /// </summary>
        /// <param name="item">待调度的视频帧。</param>
        private void VideoSchedule(VideoFrame item)
        {
            if (item.IsEmpty)
                throw new Exception("传入视频帧不能为空");

            _videoFrameQueue.Enqueue(item);
            _controller.OnVideoFrameAdded();
        }

        /// <summary>
        /// 清空音视频帧队列，释放所有帧资源。
        /// </summary>
        private void Clear()
        {
            while (_videoFrameQueue.TryDequeue(out var videoFrame))
            {
                videoFrame.Dispose();
            }
            while (_audioFrameQueue.TryDequeue(out var audioFrame))
            {
                audioFrame.Dispose();
            }
        }

        /// <summary>
        /// 释放播放器相关资源，包括解码器和帧队列。
        /// </summary>
        /// <param name="disposing">指示是否由 Dispose 方法调用。</param>
        protected override void Dispose(bool disposing)
        {
            _controller.Dispose();
            Clear();
        }
    }
}