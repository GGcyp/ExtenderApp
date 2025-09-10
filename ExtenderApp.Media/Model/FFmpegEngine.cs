using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExtenderApp.Models;
using FFmpeg.AutoGen;

namespace ExtenderApp.Media.Models
{
    public unsafe class FFmpegEngine : DataModel
    {
        // FFmpeg相关指针
        private IntPtr _formatContext;
        private IntPtr _codecContext;
        private IntPtr _swsContext;
        private IntPtr _audioCodecContext;
        private IntPtr _frame;
        private IntPtr _rgbFrame;
        private IntPtr _packet;
        private IntPtr _audioFrame;
        private IntPtr _audioPacket;

        private bool networkInitialized;

        public FFmpegEngine(string ffmpegLibPath)
        {
            if (!Directory.Exists(ffmpegLibPath))
            {
                throw new DirectoryNotFoundException($"FFmpeg库路径不存在: {ffmpegLibPath}");
            }
            ffmpeg.RootPath = ffmpegLibPath;

            // 初始化网络（用于播放网络流）
            ffmpeg.avformat_network_init();
        }

        ///// <summary>
        ///// 初始化FFmpeg并打开媒体文件
        ///// </summary>
        //private unsafe bool Init(string filePath)
        //{
        //    try
        //    {
        //        // 清理之前的资源
        //        Cleanup();

        //        // 初始化网络（用于播放网络流）
        //        ffmpeg.avformat_network_init();
        //        networkInitialized = true;
        //        // 打开媒体文件
        //        var formatContext = ffmpeg.avformat_alloc_context();
        //        int result = ffmpeg.avformat_open_input(&formatContext, filePath, null, null);
        //        if (result < 0)
        //        {
        //            ShowFFmpegError("无法打开媒体文件", result);
        //            return false;
        //        }
        //        _formatContext = (IntPtr)formatContext;

        //        // 获取流信息
        //        result = ffmpeg.avformat_find_stream_info(formatContext, null);
        //        if (result < 0)
        //        {
        //            ShowFFmpegError("无法获取流信息", result);
        //            return false;
        //        }

        //        // 获取视频流和音频流
        //        FindStreams();

        //        if (_videoStreamIndex == -1)
        //        {
        //            MessageBox.Show("未找到视频流");
        //            return false;
        //        }

        //        // 初始化视频解码器
        //        if (!InitializeVideoDecoder())
        //        {
        //            return false;
        //        }

        //        // 初始化音频解码器（可选）
        //        // InitializeAudioDecoder();

        //        // 准备视频帧处理
        //        PrepareVideoFrames();

        //        // 更新UI
        //        _videoDuration = formatContext->duration;
        //        //positionSlider.Maximum = 1.0;
        //        //positionSlider.Value = 0;

        //        UpdateTimeDisplay(0);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"初始化失败: {ex.Message}");
        //        Cleanup();
        //        return false;
        //    }
        //}

        //public void OpenFile(string filePath)
        //{
        //    //Init(filePath);
        //}

        ///// <summary>
        ///// 查找视频流和音频流
        ///// </summary>
        //private unsafe void FindStreams()
        //{
        //    AVFormatContext* formatContext = (AVFormatContext*)_formatContext;
        //    for (int i = 0; i < formatContext->nb_streams; i++)
        //    {
        //        AVStream* stream = formatContext->streams[i];
        //        AVMediaType codecType = stream->codecpar->codec_type;

        //        if (codecType == AVMediaType.AVMEDIA_TYPE_VIDEO && _videoStreamIndex == -1)
        //        {
        //            _videoStreamIndex = i;
        //        }
        //        else if (codecType == AVMediaType.AVMEDIA_TYPE_AUDIO && _audioStreamIndex == -1)
        //        {
        //            _audioStreamIndex = i;
        //        }

        //        // 如果已经找到视频和音频流，提前退出
        //        if (_videoStreamIndex != -1 && _audioStreamIndex != -1)
        //            break;
        //    }
        //}

        ///// <summary>
        ///// 初始化视频解码器
        ///// </summary>
        //private unsafe bool InitializeVideoDecoder()
        //{
        //    AVFormatContext* formatContext = (AVFormatContext*)_formatContext;
        //    AVStream* stream = formatContext->streams[_videoStreamIndex];
        //    AVCodecParameters* codecParameters = stream->codecpar;

        //    // 查找解码器
        //    AVCodec* codec = ffmpeg.avcodec_find_decoder(codecParameters->codec_id);
        //    if (codec == default)
        //    {
        //        MessageBox.Show("未找到合适的视频解码器");
        //        return false;
        //    }

        //    // 初始化解码器上下文
        //    AVCodecContext* codecContext = ffmpeg.avcodec_alloc_context3(codec);
        //    _codecContext = (IntPtr)codecContext;
        //    if (_codecContext == IntPtr.Zero)
        //    {
        //        MessageBox.Show("无法分配解码器上下文");
        //        return false;
        //    }

        //    // 从流参数复制到解码器上下文
        //    int result = ffmpeg.avcodec_parameters_to_context(codecContext, codecParameters);
        //    if (result < 0)
        //    {
        //        ShowFFmpegError("无法复制解码器参数", result);
        //        return false;
        //    }

        //    // 打开解码器
        //    result = ffmpeg.avcodec_open2(codecContext, codec, null);
        //    if (result < 0)
        //    {
        //        ShowFFmpegError("无法打开解码器", result);
        //        return false;
        //    }

        //    // 获取视频信息
        //    _videoWidth = codecContext->width;
        //    _videoHeight = codecContext->height;

        //    // 计算帧率
        //    AVRational frameRate = stream->avg_frame_rate;
        //    if (frameRate.den != 0)
        //    {
        //        _videoFrameRate = (double)frameRate.num / frameRate.den;
        //    }
        //    else
        //    {
        //        _videoFrameRate = 25.0; // 默认帧率
        //    }

        //    return true;
        //}

        ///// <summary>
        ///// 准备视频帧处理
        ///// </summary>
        //private unsafe void PrepareVideoFrames()
        //{
        //    // 分配帧
        //    AVFrame* frame = ffmpeg.av_frame_alloc();
        //    AVFrame* rgbFrame = ffmpeg.av_frame_alloc();
        //    _frame = (IntPtr)frame;
        //    _rgbFrame = (IntPtr)rgbFrame;
        //    AVCodecContext* codecContext = (AVCodecContext*)_codecContext;

        //    // 计算RGB缓冲区大小
        //    int numBytes = ffmpeg.av_image_get_buffer_size(AVPixelFormat.AV_PIX_FMT_BGR24,
        //        _videoWidth, _videoHeight, 1);

        //    // 分配RGB缓冲区
        //    //_rgbBuffer = new byte[numBytes];
        //    //fixed (byte* ptr = _rgbBuffer)
        //    //{
        //    //    ffmpeg.av_image_fill_arrays(ref rgbFrame->data, ref rgbFrame->linesize,
        //    //        ptr, AVPixelFormat.AV_PIX_FMT_BGR24,
        //    //        _videoWidth, _videoHeight, 1);
        //    //}

        //    // 创建SWS上下文用于像素格式转换
        //    SwsContext* swsContext = ffmpeg.sws_getContext(
        //        _videoWidth, _videoHeight, codecContext->pix_fmt,
        //        _videoWidth, _videoHeight, AVPixelFormat.AV_PIX_FMT_BGR24,
        //        ffmpeg.SWS_BILINEAR, null, null, null);

        //    _swsContext = (IntPtr)swsContext;

        //    // 创建WPF位图用于显示
        //    _bitmap = new WriteableBitmap(
        //        _videoWidth, _videoHeight, 96, 96,
        //        PixelFormats.Bgr24, null);

        //    videoImage.Source = _bitmap;
        //}

        ///// <summary>
        ///// 清理FFmpeg资源
        ///// </summary>
        //private unsafe void Cleanup()
        //{
        //    // 停止播放
        //    StopPlayback();

        //    // 释放FFmpeg资源
        //    if (_swsContext != IntPtr.Zero)
        //    {
        //        SwsContext* swsContext = (SwsContext*)_swsContext;
        //        ffmpeg.sws_freeContext(swsContext);
        //        _swsContext = IntPtr.Zero;
        //    }

        //    if (_frame != IntPtr.Zero)
        //    {
        //        ffmpeg.av_frame_free((AVFrame**)_frame);
        //        _frame = IntPtr.Zero;
        //    }

        //    if (_rgbFrame != IntPtr.Zero)
        //    {
        //        ffmpeg.av_frame_free((AVFrame**)_rgbFrame);
        //        _rgbFrame = IntPtr.Zero;
        //    }

        //    if (_audioFrame != IntPtr.Zero)
        //    {
        //        ffmpeg.av_frame_free((AVFrame**)_audioFrame);
        //        _audioFrame = IntPtr.Zero;
        //    }

        //    if (_packet != IntPtr.Zero)
        //    {
        //        ffmpeg.av_packet_free((AVPacket**)_packet);
        //        _packet = IntPtr.Zero;
        //    }

        //    if (_audioPacket != IntPtr.Zero)
        //    {
        //        ffmpeg.av_packet_free((AVPacket**)_audioPacket);
        //        _audioPacket = IntPtr.Zero;
        //    }

        //    if (_codecContext != IntPtr.Zero)
        //    {
        //        //ffmpeg.avcodec_close(_codecContext);
        //        ffmpeg.av_free((void*)_codecContext);
        //        _codecContext = IntPtr.Zero;
        //    }

        //    if (_audioCodecContext != IntPtr.Zero)
        //    {
        //        //ffmpeg.avcodec_close(_audioCodecContext);
        //        ffmpeg.av_free((void*)_audioCodecContext);
        //        _audioCodecContext = IntPtr.Zero;
        //    }

        //    if (_formatContext != IntPtr.Zero)
        //    {
        //        ffmpeg.avformat_close_input((AVFormatContext**)_formatContext);
        //        _formatContext = IntPtr.Zero;
        //    }

        //    if (networkInitialized)
        //    {
        //        // 清理网络
        //        ffmpeg.avformat_network_deinit();
        //        networkInitialized = false;
        //    }

        //    // 重置变量
        //    _videoStreamIndex = -1;
        //    _audioStreamIndex = -1;
        //    _rgbBuffer = null;
        //    _bitmap = null;
        //    _videoDuration = 0;

        //    // 更新UI
        //    videoImage.Source = null;
        //    //timeText.Text = "00:00/00:00";
        //    //positionSlider.Value = 0;
        //}
    }
}
