using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace ExtenderApp.Media.FFmpegEngines
{
    public class FFmpegAudioDecoder : FFmpegDecoder
    {
        private NativeIntPtr<SwrContext> swrContext;
        private NativeIntPtr<AVFrame> pcmFrame;

        public FFmpegAudioDecoder(FFmpegEngine engine, FFmpegDecoderContext context, FFmpegInfo info, CancellationToken allToken, FFmpegDecoderSettings settings) : base(engine, context, info, allToken, settings)
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
            AudioFrame audioFrame = new AudioFrame(buffer, length, Settings.SampleRate, 16, (int)Settings.ChannelLayout, framePts);
            Settings.OnAudioScheduling(audioFrame);
        }
    }
}



