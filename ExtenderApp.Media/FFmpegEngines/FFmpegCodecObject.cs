using ExtenderApp.Common;
using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// FFmpeg基础类，封装了FFmpeg相关的指针和上下文管理。
    /// </summary>
    public abstract unsafe class FFmpegCodecBase : DisposableObject
    {
        private const int MaxCacheWaitTime = 50;

        private readonly CacheStateController _controller;

        /// <summary>
        /// FFmpeg解码器上下文指针，管理流的解码信息。
        /// </summary>
        protected FFmpegContext Context { get; }

        private NativeIntPtr<AVFrame> frame;

        /// <summary>
        /// 当前处理任务的取消令牌源。
        /// </summary>
        private CancellationTokenSource? source;

        /// <summary>
        /// 当前的处理任务。
        /// </summary>
        private Task? processTask;

        private long currentPts;

        public FFmpegCodecBase(FFmpegContext context, CacheStateController controller)
        {
            Context = context;
            _controller = controller;
            currentPts = 0;
        }

        public virtual void Init()
        {
            frame = new(ffmpeg.av_frame_alloc());
        }

        public Task ProcessAsync(AVPacket* packet)
        {
            source = source ?? new CancellationTokenSource();
            processTask = Task.Run(() => Process(packet, source.Token), source.Token);
            return processTask;
        }

        public void Process(AVPacket* packet, CancellationToken token)
        {
            // 发送数据包到解码器
            int result = ffmpeg.avcodec_send_packet(Context.VideoContext.CodecContext, packet);
            if (result < 0)
            {
                throw new FFmpegCodecException(this, "发送包到解码器失败");
            }

            while (result >= 0 && !token.IsCancellationRequested && currentPts >= 0)
            {
                if (!_controller.WaitForCacheSpace(token, MaxCacheWaitTime))
                {
                    break;
                }

                result = ffmpeg.avcodec_receive_frame(Context.VideoContext.CodecContext, frame);
                if (result == ffmpeg.AVERROR(ffmpeg.EAGAIN) || result == ffmpeg.AVERROR_EOF)
                {
                    break;
                }
                else if (result < 0)
                {
                    throw new FFmpegCodecException(this, "从解码器接收帧失败");
                }

                currentPts = ProtectedProcess(frame);
            }
        }

        protected abstract long ProtectedProcess(AVFrame* frame);

        public void Cleanup()
        {
            //停止当前解析任务
            source?.Cancel();
            if (!frame.IsEmpty)
            {
                ffmpeg.av_frame_free(frame);
            }

            if (processTask != null && !processTask.IsCompleted)
            {
                try
                {
                    processTask.Wait();
                    processTask.Dispose();
                    processTask = null;
                }
                catch (AggregateException ex)
                {
                    throw new FFmpegCodecException(this, "等待处理任务完成时发生错误", ex);
                }
            }
            source?.Dispose();
            source = null;
            ffmpeg.avcodec_flush_buffers(Context.VideoContext.CodecContext);
        }

        protected abstract void protectedCleanup();

        protected override void Dispose(bool disposing)
        {
            if (!Context.IsEmpty)
            {
                ffmpeg.av_free(Context.Value);
            }
            Cleanup();
        }
    }
}
