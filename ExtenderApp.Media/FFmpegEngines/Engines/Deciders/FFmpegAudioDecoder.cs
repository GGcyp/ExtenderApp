using FFmpeg.AutoGen;
using NAudio.Wave;


namespace ExtenderApp.Media.FFmpegEngines
{
    public class FFmpegAudioDecoder : FFmpegDecoder
    {
        private NativeIntPtr<SwrContext> swrContext;
        private NativeIntPtr<AVFrame> pcmFrame;

        public FFmpegAudioDecoder(FFmpegEngine engine, FFmpegDecoderContext context, FFmpegInfo info, CancellationToken allToken, FFmpegDecoderSettings settings) : base(engine, context, info, allToken, settings, settings.AudioMaxCacheLength)
        {
            pcmFrame = engine.CreateFrame();
            engine.SettingsAudioFrame(pcmFrame, context, settings);
            swrContext = engine.CreateSwrContext();
            engine.SetSwrContextOptionsAndInit(swrContext, context, settings);
        }

        protected override void ProtectedDecoding(NativeIntPtr<AVFrame> frame, long framePts)
        {
            Engine.SwrConvert(swrContext, pcmFrame, frame);
            var buffer = Engine.CopyFrameToBuffer(pcmFrame, (long)Settings.ChannelLayout, out int length);
            long duration = Engine.GetFrameDuration(frame, Context);
            AudioFrame audioFrame = new AudioFrame(buffer, length, Settings.SampleRate, (int)Settings.ChannelLayout, 16, framePts, duration);
            Settings.OnAudioScheduling(audioFrame);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Engine.Free(ref swrContext);
            Engine.Free(ref pcmFrame);
        }
    }
}



