using System.Collections.Concurrent;
using ExtenderApp.Common;

namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 媒体播放器，负责音视频解码、播放、暂停、停止、跳转等控制。
    /// 支持多线程解码和帧调度，适用于 WPF 播放场景。
    /// </summary>
    public class MediaPlayer : DisposableObject, IMediaPlayer
    {
        /// <summary>
        /// 跳帧等待阈值（毫秒），用于控制播放节奏。
        /// </summary>
        private const int SkipWaitingTime = 15;

        /// <summary>
        /// 每个视频帧的最大允许输出时间差（毫秒），用于同步音视频。
        /// </summary>
        private const int VideoFrameOutTime = 15;

        /// <summary>
        /// 解码器设置参数。
        /// </summary>
        private readonly FFmpegDecoderSettings _settings;

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
        public long CurrentTime { get; private set; }

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
            _videoFrameQueue = new();
            _audioFrameQueue = new();
            _controller = collection;
            _allSource = allSource;
            _settings = settings;
            settings.VideoScheduling += VideoSchedule;
            settings.AudioScheduling += AudioSchedule;
            RateSpeed = 1;
        }

        /// <summary>
        /// 启动媒体播放流程，开始解码和播放。
        /// </summary>
        public void Play()
        {
            _controller.StartDecode();
            mediaTask = Task.Run(PlaybackLoop, _allSource.Token);
        }

        /// <summary>
        /// 停止媒体播放流程，取消任务并释放资源。
        /// </summary>
        public void Stop()
        {
            mediaSource?.Cancel();
            mediaTask?.Wait();
            mediaSource?.Dispose();
            mediaTask?.Dispose();
            mediaTask = null;
            mediaSource = null;
            _controller.StopDecode();
        }

        /// <summary>
        /// 暂停媒体播放流程，仅取消播放任务，不释放解码资源。
        /// </summary>
        public void Pause()
        {
            mediaSource?.Cancel();
            mediaTask?.Wait();
            mediaTask?.Dispose();
            mediaTask = null;
            mediaSource?.Dispose();
            mediaSource = null;
        }

        /// <summary>
        /// 跳转到指定时间点（TimeSpan），重置解码和播放状态。
        /// </summary>
        /// <param name="timeSpan">目标时间点。</param>
        public void Seek(TimeSpan timeSpan)
        {
            Seek((long)timeSpan.TotalMicroseconds);
        }

        /// <summary>
        /// 跳转到指定时间点（微秒），重置解码和播放状态。
        /// </summary>
        /// <param name="position">目标时间点（微秒）。</param>
        public void Seek(long position)
        {
            if (position > Info.DurationTimeSpan.TotalMicroseconds)
            {
                position = (long)Info.DurationTimeSpan.TotalMicroseconds;
            }
            else if (position < 0)
            {
                position = 0;
            }

            CurrentTime = position;
            _controller.SeekDecoder(position);
            Clear();
            OnPlayback?.Invoke(CurrentTime);
        }

        /// <summary>
        /// 媒体播放主循环任务，负责音视频帧的调度与播放。
        /// </summary>
        private async Task PlaybackLoop()
        {
            mediaSource = mediaSource ?? CancellationTokenSource.CreateLinkedTokenSource(_allSource.Token);
            var token = mediaSource.Token;
            var allToken = _allSource.Token;
            int frameInterval = (int)(Info.Rate > 0 ? 1000.0 / Info.Rate : 25); // 毫秒
            int lastDelay = frameInterval;

            while (!token.IsCancellationRequested && !allToken.IsCancellationRequested)
            {
                bool hasAudio = _audioFrameQueue.Count > 0;
                bool hasVideo = _videoFrameQueue.Count > 0;
                if (!hasAudio && !hasVideo)
                {
                    await Task.Delay(lastDelay, token);
                    continue;
                }

                int audioDelay = 0;
                while (_audioFrameQueue.TryPeek(out var audioFrame))
                {
                    long timeDiff = audioFrame.Pts - CurrentTime;

                    if (timeDiff <= 0)
                    {
                        _audioFrameQueue.TryDequeue(out audioFrame);
                        OnAudioFrame?.Invoke(audioFrame);
                        _controller.OnAudioFrameRemoved();

                        CurrentTime = audioFrame.Pts;

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
                    long videoTimeDiff = videoFrame.Pts - CurrentTime;

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
                CurrentTime += waitTime;
                lastDelay = waitTime;
                waitTime = (int)(waitTime / RateSpeed);
                OnPlayback?.Invoke(CurrentTime);

                if (waitTime >= SkipWaitingTime)
                {
                    await Task.Delay(waitTime, token);
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