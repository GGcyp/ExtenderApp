

using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    public struct FFmpegContext : IDisposable
    {
        private readonly FFmpegEngine _engine;

        public NativeIntPtr<AVFormatContext> FormatContext { get; }

        public NativeIntPtr<AVDictionary> Options { get; }

        public FFmpegInfo Info { get; }

        public FFmpegDecoderContext VideoContext { get; }

        public FFmpegDecoderContext AudioContext { get; }

        public FFmpegContext(FFmpegEngine engine, NativeIntPtr<AVFormatContext> formatContext, NativeIntPtr<AVDictionary> options, FFmpegInfo info, FFmpegDecoderContext videoContexts, FFmpegDecoderContext audioContext)
        {
            _engine = engine;
            FormatContext = formatContext;
            Options = options;
            Info = info;
            VideoContext = videoContexts;
            AudioContext = audioContext;
        }

        public void Dispose()
        {
            //_engine.FreeFormatContext(this);
        }
    }
}
