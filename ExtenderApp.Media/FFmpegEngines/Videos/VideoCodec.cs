using System.Buffers;
using ExtenderApp.Abstract;
using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    public unsafe class VideoCodec : FFmpegCodecBase
    {
        protected NativeIntPtr<SwsContext> SwsContextPtr;
        protected NativeIntPtr<AVFrame> RgbFrameIntPtr;


        private byte[]? rgbBuffer;

        private Task? playbackTask;

        private int rgbBufferLength;
        private int width;
        private int height;

        public VideoOutSettings Settings { get; private set; }

        private readonly IFFmpegCodecScheduling<VideoFrame> _scheduling;

        public VideoCodec(NativeIntPtr<AVCodecContext> codecContext, CacheStateController controller, IFFmpegCodecScheduling<VideoFrame> scheduling, VideoOutSettings settings) : base(codecContext, controller)
        {
            Settings = settings;
            _scheduling = scheduling;
        }

        public override void Init()
        {
            base.Init();

            // 分配帧和RGB帧
            RgbFrameIntPtr = new(ffmpeg.av_frame_alloc());

            width = Context.Value->width;
            height = Context.Value->height;

            // 计算RGB缓冲区大小
            AVPixelFormat pixFmt = Settings.IsSourceFormatEqualTargetFormat ? Context.Value->pix_fmt : Settings.TargetPixelFormat;
            rgbBufferLength = ffmpeg.av_image_get_buffer_size(pixFmt,
                width,
                height,
                1);


            // 分配RGB缓冲区
            rgbBuffer = ArrayPool<byte>.Shared.Rent(rgbBufferLength);
            // 声明临时变量
            byte_ptrArray4 data4 = default;
            int_array4 linesize4 = default;
            fixed (byte* ptr = rgbBuffer)
            {
                ffmpeg.av_image_fill_arrays(ref data4,
                    ref linesize4,
                    ptr,
                    pixFmt,
                    width,
                    height,
                    1);

                // 复制到rgbFrame->data和rgbFrame->linesize
                for (uint i = 0; i < 4; i++)
                {
                    RgbFrameIntPtr.Value->data[i] = data4[i];
                    RgbFrameIntPtr.Value->linesize[i] = linesize4[i];
                }
            }

            // 创建SWS上下文用于像素格式转换
            SwsContextPtr = new(ffmpeg.sws_getContext(
               width,
               width,
               Context.Value->pix_fmt,
               width,
               height,
               pixFmt,
               ffmpeg.SWS_BILINEAR, null, null, null));
        }

        protected override unsafe long ProtectedProcess(AVFrame* frame)
        {
            int result = ffmpeg.avcodec_receive_frame(Context.Value, frame);
            if (result == ffmpeg.AVERROR(ffmpeg.EAGAIN) || result == ffmpeg.AVERROR_EOF)
            {
                return -1;
            }
            else if (result < 0)
            {
                throw new FFmpegCodecException(this, "从解码器接收帧失败");
            }

            // 转换像素格式并显示
            ffmpeg.sws_scale(SwsContextPtr.Value,
                frame->data,
                frame->linesize,
                0,
                height,
                RgbFrameIntPtr.Value->data,
                RgbFrameIntPtr.Value->linesize);

            //frame->pts
            long pts = frame->pts;
            var data = ArrayPool<byte>.Shared.Rent(rgbBufferLength);
            Array.Copy(rgbBuffer!, data, rgbBufferLength);

            VideoFrame videoFrame = new VideoFrame(rgbBuffer!,
                rgbBufferLength,
                pts,
                frame->width,
                frame->height,
                frame->linesize[0]);

            _scheduling.Schedule(videoFrame);
            return pts;
        }

        protected override void protectedCleanup()
        {
            // 释放FFmpeg资源
            if (!SwsContextPtr.IsEmpty)
            {
                ffmpeg.sws_freeContext(SwsContextPtr.Value);
            }

            if (!RgbFrameIntPtr.IsEmpty)
            {
                ffmpeg.av_frame_free(RgbFrameIntPtr.ValuePtr);
            }
            ArrayPool<byte>.Shared.Return(rgbBuffer!);
        }
    }
}
