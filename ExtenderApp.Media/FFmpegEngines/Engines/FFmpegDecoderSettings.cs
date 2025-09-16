using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    public class FFmpegDecoderSettings
    {
        public AVPixelFormat PixelFormat { get; } = AVPixelFormat.AV_PIX_FMT_BGR24;

        public int MaxCacheLength { get; } = 10;

        public ulong ChannelLayout { get; } = 2;

        public int SampleFormat { get; } = (int)AVSampleFormat.AV_SAMPLE_FMT_S16;

        public int SampleRate { get; } = 44100;


        public event Action<VideoFrame>? VideoScheduling;
        public void OnVideoScheduling(VideoFrame videoFrame)
            => VideoScheduling?.Invoke(videoFrame);

        public event Action<AudioFrame>? AudioScheduling;
        public void OnAudioScheduling(AudioFrame audioFrame)
            => AudioScheduling?.Invoke(audioFrame);
    }
}
