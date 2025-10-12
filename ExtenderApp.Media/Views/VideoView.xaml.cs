using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ExtenderApp.Media.ViewModels;
using ExtenderApp.Views;
using FFmpeg.AutoGen;
using Microsoft.Win32;

namespace ExtenderApp.Media
{
    /// <summary>
    /// VideoView.xaml 的交互逻辑
    /// </summary>
    public partial class VideoView : ExtenderAppView
    {
        public VideoView(VideoViewModel viewModle) : base(viewModle)
        {
            InitializeComponent();
        }

        //// FFmpeg相关指针
        //private IntPtr _formatContext;
        //private IntPtr _codecContext;
        //private IntPtr _swsContext;
        //private IntPtr _audioCodecContext;
        //private IntPtr _frame;
        //private IntPtr _rgbFrame;
        //private IntPtr _packet;
        //private IntPtr _audioFrame;
        //private IntPtr _audioPacket;

        //private bool _networkInitialized;

        //// 视频流和音频流索引
        //private int _videoStreamIndex = -1;
        //private int _audioStreamIndex = -1;

        //// 播放控制变量
        //private bool _isPlaying;
        //private bool _isPaused;
        //private bool _isDraggingSlider;
        //private CancellationTokenSource _cts;
        //private Task _playbackTask;
        //private byte[] _rgbBuffer;
        //private WriteableBitmap _bitmap;

        //// 视频信息
        //private long _videoDuration;
        //private Float64 _videoFrameRate;
        //private int _videoWidth;
        //private int _videoHeight;

        //public VideoView(VideoViewModel viewModle) : base(viewModle)
        //{
        //    InitializeComponent();

        //    //// 确保在窗口关闭时释放资源
        //    //Closing += (s, e) => Cleanup();
        //    //StartPlayback();
        //    //InitializeFFmpeg("D:\\迅雷下载\\国产网红.推特_一条肌肉狗_后入爆肏极品黑丝高跟骚母狗_1.mp4");
        //    //StartPlayback();
        //}

        //#region 按钮事件处理

        //private void OpenButton_Click(object sender, RoutedEventArgs e)
        //{
        //    // 先停止当前播放
        //    StopPlayback();

        //    // 打开文件选择对话框
        //    var openFileDialog = new OpenFileDialog
        //    {
        //        Filter = "视频文件|*.mp4;*.avi;*.mkv;*.mov;*.flv;*.wmv|所有文件|*.*"
        //    };

        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        string filePath = openFileDialog.FileName;
        //        if (InitializeFFmpeg(filePath))
        //        {
        //            //// 初始化成功，启用播放按钮
        //            //playButton.IsEnabled = true;
        //            //statusText.Text = $"已加载: {Path.GetFileName(filePath)}";
        //        }
        //    }
        //}

        //private void PlayButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (_isPaused)
        //    {
        //        // 从暂停状态恢复播放
        //        _isPaused = false;
        //    }
        //    else
        //    {
        //        // 开始新的播放
        //        StartPlayback();
        //    }

        //    UpdateButtonStates();
        //}

        //private void PauseButton_Click(object sender, RoutedEventArgs e)
        //{
        //    _isPaused = true;
        //    UpdateButtonStates();
        //}

        //private void StopButton_Click(object sender, RoutedEventArgs e)
        //{
        //    StopPlayback();
        //    UpdateButtonStates();
        //}

        //private void PositionSlider_DragStarted(object sender, DragStartedEventArgs e)
        //{
        //    _isDraggingSlider = true;
        //}

        //private void PositionSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        //{
        //    _isDraggingSlider = false;

        //    //if (_formatContext != IntPtr.Zero && _isPlaying)
        //    //{
        //    //    // 计算目标位置
        //    //    long targetPosition = (long)(_videoDuration * positionSlider.Value);

        //    //    //  seek到指定位置
        //    //    ffmpeg.av_seek_frame(_formatContext, -1, targetPosition, ffmpeg.AVSEEK_FLAG_BACKWARD);

        //    //    // 清除解码器缓存
        //    //    if (_codecContext != IntPtr.Zero)
        //    //    {
        //    //        ffmpeg.avcodec_flush_buffers(_codecContext);
        //    //    }
        //    //    if (_audioCodecContext != IntPtr.Zero)
        //    //    {
        //    //        ffmpeg.avcodec_flush_buffers(_audioCodecContext);
        //    //    }
        //    //}
        //}

        //private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Float64> e)
        //{
        //    // 这里可以添加音量控制逻辑
        //}

        //#endregion

        //#region FFmpeg初始化与清理

        ///// <summary>
        ///// 初始化FFmpeg并打开媒体文件
        ///// </summary>
        //private unsafe bool InitializeFFmpeg(string filePath)
        //{
        //    try
        //    {
        //        // 清理之前的资源
        //        Cleanup();

        //        // 初始化网络（用于播放网络流）
        //        ffmpeg.avformat_network_init();
        //        _networkInitialized = true;
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
        //        _videoFrameRate = (Float64)frameRate.num / frameRate.den;
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
        //    _rgbBuffer = new byte[numBytes];
        //    // 声明临时变量
        //    byte_ptrArray4 data4;
        //    int_array4 linesize4;
        //    fixed (byte* ptr = _rgbBuffer)
        //    {
        //        ffmpeg.av_image_fill_arrays(ref data4, ref linesize4,
        //            ptr, AVPixelFormat.AV_PIX_FMT_BGR24,
        //            _videoWidth, _videoHeight, 1);

        //        // 复制到rgbFrame->data和rgbFrame->linesize
        //        for (uint i = 0; i < 4; i++)
        //        {
        //            rgbFrame->data[i] = data4[i];
        //            rgbFrame->linesize[i] = linesize4[i];
        //        }
        //    }

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

        //    if (_networkInitialized)
        //    {
        //        // 清理网络
        //        ffmpeg.avformat_network_deinit();
        //        _networkInitialized = false;
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

        //#endregion

        //#region 播放控制

        ///// <summary>
        ///// 开始播放
        ///// </summary>
        //private unsafe void StartPlayback()
        //{
        //    if (_isPlaying || _formatContext == IntPtr.Zero)
        //        return;

        //    _isPlaying = true;
        //    _isPaused = false;
        //    _cts = new CancellationTokenSource();

        //    // 创建数据包
        //    _packet = (IntPtr)ffmpeg.av_packet_alloc();

        //    // 启动播放任务
        //    _playbackTask = Task.Run(() => PlaybackLoop(_cts.Token), _cts.Token);
        //}

        ///// <summary>
        ///// 播放循环
        ///// </summary>
        //private unsafe void PlaybackLoop(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        while (!cancellationToken.IsCancellationRequested && _isPlaying)
        //        {
        //            // 如果处于暂停状态，短暂休眠
        //            if (_isPaused)
        //            {
        //                Thread.Sleep(100);
        //                continue;
        //            }

        //            // 读取数据包
        //            int result = ffmpeg.av_read_frame((AVFormatContext*)_formatContext, (AVPacket*)_packet);
        //            if (result < 0)
        //            {
        //                // 读取完毕，退出循环
        //                if (result == ffmpeg.AVERROR_EOF)
        //                {
        //                    //Dispatcher.Invoke(() =>
        //                    //{
        //                    //    statusText.Text = "播放完成";
        //                    //});
        //                }
        //                else
        //                {
        //                    ShowFFmpegError("读取帧失败", result);
        //                }
        //                break;
        //            }

        //            // 处理视频帧
        //            AVPacket* packet = (AVPacket*)_packet;
        //            if (packet->stream_index == _videoStreamIndex)
        //            {
        //                ProcessVideoPacket(cancellationToken);
        //            }
        //            // 处理音频帧（可选）
        //            else if (packet->stream_index == _audioStreamIndex && _audioCodecContext != IntPtr.Zero)
        //            {
        //                // ProcessAudioPacket(cancellationToken);
        //            }

        //            // 释放数据包
        //            ffmpeg.av_packet_unref(packet);

        //            // 更新进度（如果不是用户正在拖动滑块）
        //            AVFormatContext* formatContext = (AVFormatContext*)_formatContext;
        //            if (!_isDraggingSlider && _formatContext != IntPtr.Zero)
        //            {
        //                long currentTime = formatContext->streams[_videoStreamIndex]->index;
        //                Float64 position = (Float64)currentTime / _videoDuration;

        //                Dispatcher.Invoke(() =>
        //                {
        //                    //positionSlider.Value = position;
        //                    UpdateTimeDisplay(currentTime);
        //                });
        //            }

        //            // 控制播放速度
        //            Thread.Sleep((int)(1000 / _videoFrameRate));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Dispatcher.Invoke(() =>
        //        //{
        //        //    MessageBox.Show($"播放错误: {ex.Message}");
        //        //    statusText.Text = $"错误: {ex.Message}";
        //        //});
        //    }
        //    finally
        //    {
        //        if (!cancellationToken.IsCancellationRequested)
        //        {
        //            Dispatcher.Invoke(StopPlayback);
        //        }
        //    }
        //}

        ///// <summary>
        ///// 处理视频数据包
        ///// </summary>
        //private unsafe void ProcessVideoPacket(CancellationToken cancellationToken)
        //{
        //    // 发送数据包到解码器
        //    AVCodecContext* codecContext = (AVCodecContext*)_codecContext;
        //    AVPacket* packet = (AVPacket*)_packet;
        //    int result = ffmpeg.avcodec_send_packet(codecContext, packet);
        //    if (result < 0)
        //    {
        //        ShowFFmpegError("发送数据包失败", result);
        //        return;
        //    }

        //    // 接收解码后的帧
        //    AVFrame* frame = (AVFrame*)_frame;
        //    SwsContext* swsContext = (SwsContext*)_swsContext;
        //    AVFrame* rgbFrame = (AVFrame*)_rgbFrame;
        //    while (result >= 0 && !cancellationToken.IsCancellationRequested)
        //    {
        //        result = ffmpeg.avcodec_receive_frame(codecContext, frame);
        //        if (result == ffmpeg.AVERROR(ffmpeg.EAGAIN) || result == ffmpeg.AVERROR_EOF)
        //        {
        //            break;
        //        }
        //        else if (result < 0)
        //        {
        //            ShowFFmpegError("接收帧失败", result);
        //            return;
        //        }

        //        // 转换像素格式并显示
        //        ffmpeg.sws_scale(swsContext,
        //            frame->data, frame->linesize, 0, _videoHeight,
        //            rgbFrame->data, rgbFrame->linesize);

        //        // 在UI线程更新图像
        //        Dispatcher.Invoke(() =>
        //        {
        //            _bitmap.WritePixels(
        //                new Int32Rect(0, 0, _videoWidth, _videoHeight),
        //                _rgbBuffer, _videoWidth * 3, 0);
        //        });
        //    }
        //}

        ///// <summary>
        ///// 停止播放
        ///// </summary>
        //private unsafe void StopPlayback()
        //{
        //    if (!_isPlaying)
        //        return;

        //    _isPlaying = false;
        //    _isPaused = false;

        //    if (_cts != null)
        //    {
        //        _cts.Cancel();
        //        _cts.Dispose();
        //        _cts = null;
        //    }

        //    // 等待播放任务结束
        //    if (_playbackTask != null)
        //    {
        //        try
        //        {
        //            _playbackTask.Wait();
        //        }
        //        catch (AggregateException)
        //        {
        //            // 忽略取消异常
        //        }
        //        _playbackTask = null;
        //    }

        //    // 重置播放位置到开始
        //    if (_formatContext != IntPtr.Zero)
        //    {
        //        AVFormatContext* formatContext = (AVFormatContext*)_formatContext;
        //        ffmpeg.av_seek_frame(formatContext, _videoStreamIndex, 0, ffmpeg.AVSEEK_FLAG_BACKWARD);

        //        AVCodecContext* codecContext = (AVCodecContext*)_codecContext;
        //        ffmpeg.avcodec_flush_buffers(codecContext);
        //    }

        //    //// 更新UI
        //    //Dispatcher.Invoke(() =>
        //    //{
        //    //    positionSlider.Value = 0;
        //    //    UpdateTimeDisplay(0);
        //    //    statusText.Text = "已停止";
        //    //});
        //}

        //#endregion

        //#region UI辅助方法

        ///// <summary>
        ///// 更新按钮状态
        ///// </summary>
        //private void UpdateButtonStates()
        //{
        //    //playButton.IsEnabled = !_isPlaying || _isPaused;
        //    //pauseButton.IsEnabled = _isPlaying && !_isPaused;
        //    //stopButton.IsEnabled = _isPlaying;

        //    //statusText.Text = _isPlaying
        //    //    ? (_isPaused ? "已暂停" : "播放中")
        //    //    : "已停止";
        //}

        ///// <summary>
        ///// 更新时间显示
        ///// </summary>
        //private void UpdateTimeDisplay(long currentTime)
        //{
        //    //// 转换为秒
        //    //Float64 currentSeconds = (Float64)currentTime / ffmpeg.AV_TIME_BASE;
        //    //Float64 totalSeconds = (Float64)_videoDuration / ffmpeg.AV_TIME_BASE;

        //    //// 格式化时间
        //    //string currentTimeStr = TimeSpan.FromSeconds(currentSeconds).ToString(@"mm\:ss");
        //    //string totalTimeStr = TimeSpan.FromSeconds(totalSeconds).ToString(@"mm\:ss");

        //    //timeText.Text = $"{currentTimeStr}/{totalTimeStr}";
        //}

        ///// <summary>
        ///// 显示FFmpeg错误信息
        ///// </summary>
        //private unsafe void ShowFFmpegError(string message, int errorCode)
        //{
        //    //IntPtr buffer = Marshal.AllocHGlobal(1024);
        //    ulong errorBufferLength = 1024;
        //    byte[] errorBuffer = new byte[errorBufferLength];
        //    fixed (byte* errorBufferPtr = errorBuffer)
        //    {
        //        ffmpeg.av_strerror(errorCode, errorBufferPtr, errorBufferLength);

        //        IntPtr ptr = (IntPtr)errorBufferPtr;
        //        string errorMessage = Marshal.PtrToStringAnsi(ptr);
        //        Marshal.FreeHGlobal(ptr);

        //        Dispatcher.Invoke(() =>
        //        {
        //            MessageBox.Show($"{message}: {errorMessage} (错误代码: {errorCode})");
        //            //statusText.Text = $"错误: {errorMessage}";
        //        });
        //    }

        //}

        //#endregion
    }
}
