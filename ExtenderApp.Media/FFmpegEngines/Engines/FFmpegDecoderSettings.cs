using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 解码器参数设置类。
    /// 用于配置视频/音频解码相关参数，并提供帧调度事件。
    /// </summary>
    public class FFmpegDecoderSettings
    {
        /// <summary>
        /// 视频像素格式，默认 BGR24。
        /// </summary>
        public AVPixelFormat PixelFormat { get; } = AVPixelFormat.AV_PIX_FMT_BGR24;

        /// <summary>
        /// 视频帧最大缓存数量。
        /// </summary>
        public int VideoMaxCacheLength { get; } = 10;
        /// <summary>
        /// 音频帧最大缓存数量。
        /// </summary>
        public int AudioMaxCacheLength { get; } = 10;

        /// <summary>
        /// 音频通道布局（如立体声=2）。
        /// </summary>
        public ulong ChannelLayout { get; } = 2;

        /// <summary>
        /// 音频采样格式（如 S16）。
        /// </summary>
        public int SampleFormat { get; } = (int)AVSampleFormat.AV_SAMPLE_FMT_S16;

        /// <summary>
        /// 音频采样率（Hz），默认 44100。
        /// </summary>
        public int SampleRate { get; } = 44100;

        /// <summary>
        /// 视频帧调度事件，解码后触发。
        /// </summary>
        public event Action<VideoFrame>? VideoScheduling;
        /// <summary>
        /// 触发视频帧调度事件。
        /// </summary>
        /// <param name="videoFrame">待调度的视频帧。</param>
        public void OnVideoScheduling(VideoFrame videoFrame)
            => VideoScheduling?.Invoke(videoFrame);

        /// <summary>
        /// 音频帧调度事件，解码后触发。
        /// </summary>
        public event Action<AudioFrame>? AudioScheduling;
        /// <summary>
        /// 触发音频帧调度事件。
        /// </summary>
        /// <param name="audioFrame">待调度的音频帧。</param>
        public void OnAudioScheduling(AudioFrame audioFrame)
            => AudioScheduling?.Invoke(audioFrame);
    }
}
