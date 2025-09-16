using System.Collections.Concurrent;
using ExtenderApp.Common;
using ExtenderApp.Common.DataBuffers;

namespace ExtenderApp.Media.FFmpegEngines
{
    public class MediaPlayer : DisposableObject
    {
        private FFmpegDecoderSettings _settings;
        private readonly ConcurrentQueue<VideoFrame> _videoFrameQueue;
        private readonly ConcurrentQueue<AudioFrame> _audioFrameQueue;
        private readonly FFmpegDecoderController _controller;
        private readonly CancellationTokenSource _allSource;
        public long CurrentPts { get; private set; }

        private Task? mediaTask;
        private CancellationTokenSource? mediaSource;

        public FFmpegInfo Info => _controller.Info;

        #region Events

        public event Action<VideoFrame>? OnVideoFrame;
        public event Action<AudioFrame>? OnAudioFrame;

        #endregion

        public MediaPlayer(FFmpegDecoderController collection, CancellationTokenSource allSource, FFmpegDecoderSettings settings, int maxFrameCount, int minframeCount)
        {
            _videoFrameQueue = new();
            _audioFrameQueue = new();
            _controller = collection;
            _allSource = allSource;
            _settings = settings;
            settings.VideoScheduling += VideoSchedule;
            settings.AudioScheduling += AudioSchedule;
        }

        private void AudioSchedule(AudioFrame obj)
        {
            if (obj.IsEmpty)
                throw new Exception("传入音频帧不能为空");
            _audioFrameQueue.Enqueue(obj);
            _controller.OnAudioFrameAdded();
        }

        public void Play()
        {
            _controller.StartDecode();
            mediaTask = Task.Run(PlaybackLoop, _allSource.Token);
        }

        private async Task PlaybackLoop()
        {
            mediaSource = mediaSource ?? CancellationTokenSource.CreateLinkedTokenSource(_allSource.Token);
            var token = mediaSource.Token;
            var allToken = _allSource.Token;
            int frameInterval = (int)(Info.FrameRate > 0 ? 1000.0 / Info.FrameRate : 20); // 毫秒
            while (!token.IsCancellationRequested && !allToken.IsCancellationRequested)
            {
                if (_videoFrameQueue.Count == 0 || !_videoFrameQueue.TryDequeue(out var videoFrame))
                {
                    await Task.Delay(frameInterval, token);
                    continue;
                }

                OnVideoFrame?.Invoke(videoFrame);
                videoFrame.Dispose();
                CurrentPts = videoFrame.Pts;

                while (_audioFrameQueue.Count > 0)
                {
                    if (!_audioFrameQueue.TryPeek(out var audioFrame))
                    {
                        break;
                    }

                    if (audioFrame.Pts > CurrentPts)
                    {
                        break;
                    }
                    _audioFrameQueue.TryDequeue(out audioFrame);
                    OnAudioFrame?.Invoke(audioFrame);
                    _controller.OnAudioFrameRemoved();
                    audioFrame.Dispose();
                }

                _controller.OnVideoFrameRemoved();
                await Task.Delay(frameInterval, token);
            }
        }

        public void VideoSchedule(VideoFrame item)
        {
            if (item.IsEmpty)
                throw new Exception("传入视频帧不能为空");

            _videoFrameQueue.Enqueue(item);
            _controller.OnVideoFrameAdded();
        }

        protected override void Dispose(bool disposing)
        {
            //ffmpeg.avformat_close_input(formatContextIntPtr.ValuePtr);
        }
    }
}
