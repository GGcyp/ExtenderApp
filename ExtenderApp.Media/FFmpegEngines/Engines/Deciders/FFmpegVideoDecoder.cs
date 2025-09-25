using System.Buffers;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    public class FFmpegVideoDecoder : FFmpegDecoder
    {
        private NativeIntPtr<SwsContext> swsContext;
        private NativeIntPtr<AVFrame> rgbFrame;

        private NativeIntPtr<byte> rgbBuffer;

        private int rgbBufferLength;

        public FFmpegVideoDecoder(FFmpegEngine engine, FFmpegDecoderContext context, FFmpegInfo info, CancellationToken allToken, FFmpegDecoderSettings settings) : base(engine, context, info, allToken, settings, settings.VideoMaxCacheLength)
        {
            //分配帧和RGB帧
            rgbFrame = engine.CreateFrame();
            rgbBufferLength = engine.GetBufferSizeForImage(settings.PixelFormat, info.Width, info.Height);
            rgbBuffer = engine.CreateRGBBuffer(ref rgbFrame, rgbBufferLength, settings.PixelFormat, info.Width, info.Height);
            swsContext = engine.CreateSwsContext(info.Width, info.Height, info.PixelFormat, info.Width, info.Height, settings.PixelFormat);
        }

        protected override void ProtectedDecoding(NativeIntPtr<AVFrame> frame, long framePts)
        {
            Engine.Scale(swsContext, frame, rgbFrame, Info);
            var buffer = ArrayPool<byte>.Shared.Rent(rgbBufferLength);
            Marshal.Copy(rgbBuffer, buffer, 0, rgbBufferLength);

            VideoFrame videoFrame = new(buffer, framePts, Info.Width, Info.Height, GetStride());
            Settings.OnVideoScheduling(videoFrame);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Engine.Free(ref swsContext);
            Engine.ReturnFrame(ref rgbFrame);
            Marshal.FreeHGlobal(rgbBuffer);
        }
    }
}
