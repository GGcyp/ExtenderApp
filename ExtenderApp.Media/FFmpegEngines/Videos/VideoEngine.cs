using System.Collections.Concurrent;
using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    public class VideoEngine
    {
        private readonly ConcurrentDictionary<string, VideoCodec> _videos;

        internal VideoEngine(string ffmpegPath)
        {
            _videos = new();
        }

        //public Video CreateVideo(string filePath, VideoOutSettings settings)
        //{
        //    VideoInfo info = VideoInfo.FromFile(filePath);

        //    //Video video = new Video(this, info, settings);

        //    return video;
        //}


        //private void InitVideo()
        //{
        //    try
        //    {
        //        var fc = formatContext = ffmpeg.avformat_alloc_context();
        //        int result = ffmpeg.avformat_open_input(&fc, Info.Uri, null, null);
        //        if (result < 0)
        //        {
        //            throw new ApplicationException($"无法打开视频文件: {Info.Uri}");
        //        }

        //        result = ffmpeg.avformat_find_stream_info(formatContext, null);
        //        if (result < 0)
        //        {
        //            throw new ApplicationException($"无法获取视频流信息: {Info.Uri}");
        //        }

        //        FindStreams();

        //        InitializeVideoDecoder();

        //        PrepareVideoFrames();

        //        OnStateChanged(FFmpegState.Stopped);
        //        Started?.Invoke(this, EventArgs.Empty);
        //    }
        //    catch (Exception ex)
        //    {
        //        Cleanup();
        //        throw new VideoException(this, "视频初始化失败", ex);
        //    }
        //}

        //private void FindStreams()
        //{
        //    for (int i = 0; i < formatContext->nb_streams; i++)
        //    {
        //        AVStream* stream = formatContext->streams[i];
        //        AVMediaType codecType = stream->codecpar->codec_type;

        //        if (codecType == AVMediaType.AVMEDIA_TYPE_VIDEO && VideoStreamIndex == -1)
        //        {
        //            VideoStreamIndex = i;
        //        }

        //        // 如果已经找到视频和音频流，提前退出
        //        if (VideoStreamIndex != -1)
        //            break;
        //    }

        //    if (VideoStreamIndex == -1)
        //    {
        //        throw new ApplicationException("未找到视频流");
        //    }
        //}

        ///// <summary>
        ///// 初始化视频解码器
        ///// </summary>
        //private bool InitializeVideoDecoder()
        //{
        //    AVStream* stream = formatContext->streams[VideoStreamIndex];
        //    AVCodecParameters* codecParameters = stream->codecpar;

        //    // 查找解码器
        //    AVCodec* codec = ffmpeg.avcodec_find_decoder(codecParameters->codec_id);
        //    if ((IntPtr)codec == IntPtr.Zero)
        //    {
        //        return false;
        //    }

        //    // 初始化解码器上下文
        //    codecContext = ffmpeg.avcodec_alloc_context3(codec);
        //    if (codecContextIntPtr == IntPtr.Zero)
        //    {
        //        return false;
        //    }

        //    // 从流参数复制到解码器上下文
        //    int result = ffmpeg.avcodec_parameters_to_context(codecContext, codecParameters);
        //    if (result < 0)
        //    {
        //        throw new ApplicationException("无法复制解码器参数到上下文");
        //    }

        //    // 打开解码器
        //    result = ffmpeg.avcodec_open2(codecContext, codec, null);
        //    if (result < 0)
        //    {
        //        throw new ApplicationException("无法打开解码器");
        //    }

        //    return true;
        //}

        //private void PrepareVideoFrames()
        //{
        //    // 分配帧和RGB帧
        //    frame = ffmpeg.av_frame_alloc();
        //    rgbFrame = ffmpeg.av_frame_alloc();

        //    // 计算RGB缓冲区大小
        //    int numBytes = ffmpeg.av_image_get_buffer_size(TargetPixelFormat,
        //        Info.Width,
        //        Info.Height,
        //        1);

        //    AVPixelFormat pixFmt = Settings.IsSourceFormatEqualTargetFormat ? codecContext->pix_fmt : TargetPixelFormat;

        //    // 分配RGB缓冲区
        //    rgbBuffer = ArrayPool<byte>.Shared.Rent(numBytes);
        //    // 声明临时变量
        //    byte_ptrArray4 data4;
        //    int_array4 linesize4;
        //    fixed (byte* ptr = rgbBuffer)
        //    {
        //        ffmpeg.av_image_fill_arrays(ref data4,
        //            ref linesize4,
        //            ptr,
        //            pixFmt,
        //            Info.Width,
        //            Info.Height,
        //            1);

        //        // 复制到rgbFrame->data和rgbFrame->linesize
        //        for (uint i = 0; i < 4; i++)
        //        {
        //            rgbFrame->data[i] = data4[i];
        //            rgbFrame->linesize[i] = linesize4[i];
        //        }
        //    }

        //    // 创建SWS上下文用于像素格式转换
        //    swsContext = ffmpeg.sws_getContext(
        //       Info.Width,
        //       Info.Height,
        //       codecContext->pix_fmt,
        //       Info.Width,
        //       Info.Height,
        //       pixFmt,
        //       ffmpeg.SWS_BILINEAR, null, null, null);
        //}
    }
}
