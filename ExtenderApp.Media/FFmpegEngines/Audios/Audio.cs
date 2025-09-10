using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using ExtenderApp.Common;
using FFmpeg.AutoGen;
using Microsoft.VisualBasic;

namespace ExtenderApp.Media.FFmpegEngines
{
    public unsafe class Audio : DisposableObject
    {
        //#region FFmpeg核心指针（类比Video类）
        //private readonly AVFormatContext* _formatContext; // 从VideoEngine注入，共享媒体上下文
        //private IntPtr _audioCodecContextIntPtr;
        //private IntPtr _audioFrameIntPtr;
        //private IntPtr _audioPacketIntPtr;
        //private IntPtr _swrContextIntPtr; // 音频重采样上下文（处理不同采样率/格式）

        //// 强类型访问
        //private AVCodecContext* AudioCodecContext => (AVCodecContext*)_audioCodecContextIntPtr;
        //private AVFrame* AudioFrame => (AVFrame*)_audioFrameIntPtr;
        //private AVPacket* AudioPacket => (AVPacket*)_audioPacketIntPtr;
        //private SwrContext* SwrContext => (SwrContext*)_swrContextIntPtr;
        //#endregion

        //#region 音频核心参数
        //public int AudioStreamIndex { get; } // 音频流索引（从VideoEngine注入）
        //public AudioState State { get; private set; } = AudioState.Uninitialized;
        //public AudioInfo Info { get; } // 音频信息（采样率、声道数、采样格式等）

        //// 音频输出目标格式（统一为PCM 16位，方便播放）
        //private const AVSampleFormat TargetSampleFormat = AVSampleFormat.AV_SAMPLE_FMT_S16;
        //private const int TargetSampleRate = 44100; // 标准采样率
        //private const int TargetChannels = 2; // 立体声
        //#endregion

        //#region 音频播放组件（NAudio）
        //private WaveOutEvent _waveOut; // 音频输出设备
        //private CircularBuffer<byte> _pcmBuffer; // PCM数据缓冲区（避免播放卡顿）
        //private bool _isWaveOutReady;
        //#endregion

        //#region 同步相关（关键：暴露当前音频PTS供视频同步）
        //private long _currentPtsMs; // 当前播放的音频PTS（毫秒）
        //public long CurrentPtsMs => Volatile.Read(ref _currentPtsMs); // 线程安全读取
        //#endregion

        //#region 事件（类比Video类）
        //public event EventHandler? Started;
        //public event EventHandler<AudioState>? StateChanged;
        //#endregion

        //private CancellationTokenSource? _cts;
        //private Task? _decodeTask;

        //// 构造函数：从VideoEngine注入共享的媒体上下文和音频流信息
        //internal Audio(AVFormatContext* formatContext, int audioStreamIndex, AudioInfo audioInfo)
        //{
        //    _formatContext = formatContext;
        //    AudioStreamIndex = audioStreamIndex;
        //    Info = audioInfo;
        //    _pcmBuffer = new CircularBuffer<byte>(1024 * 1024); // 1MB缓冲区
        //}

        //#region 1. 初始化音频（解码+播放设备）
        //public Task InitAsync()
        //{
        //    if (State != AudioState.Uninitialized)
        //        return Task.CompletedTask;

        //    _cts = new CancellationTokenSource();
        //    return Task.Run(InitAudio, _cts.Token);
        //}

        //private void InitAudio()
        //{
        //    try
        //    {
        //        // 1.1 查找音频解码器
        //        AVStream* audioStream = _formatContext->streams[AudioStreamIndex];
        //        AVCodecParameters* codecPar = audioStream->codecpar;
        //        AVCodec* audioCodec = ffmpeg.avcodec_find_decoder(codecPar->codec_id);
        //        if (audioCodec == null)
        //            throw new ApplicationException("未找到音频解码器");

        //        // 1.2 初始化解码器上下文
        //        _audioCodecContextIntPtr = (IntPtr)ffmpeg.avcodec_alloc_context3(audioCodec);
        //        int result = ffmpeg.avcodec_parameters_to_context(AudioCodecContext, codecPar);
        //        if (result < 0)
        //            throw new ApplicationException("复制音频解码器参数失败");

        //        // 1.3 打开解码器
        //        result = ffmpeg.avcodec_open2(AudioCodecContext, audioCodec, null);
        //        if (result < 0)
        //            throw new ApplicationException("打开音频解码器失败");

        //        // 1.4 初始化音频重采样（将解码后的音频转为目标格式：PCM 16位/44100Hz/立体声）
        //        InitSwrContext(audioStream);

        //        // 1.5 初始化NAudio播放设备
        //        InitWaveOut();

        //        // 1.6 初始化音频帧和数据包
        //        _audioFrameIntPtr = (IntPtr)ffmpeg.av_frame_alloc();
        //        _audioPacketIntPtr = (IntPtr)ffmpeg.av_packet_alloc();

        //        State = AudioState.Stopped;
        //        OnStateChanged(AudioState.Stopped);
        //        Started?.Invoke(this, EventArgs.Empty);
        //    }
        //    catch (Exception ex)
        //    {
        //        Cleanup();
        //        throw new AudioException(this, "音频初始化失败", ex);
        //    }
        //}

        //// 初始化音频重采样上下文（处理不同采样率/格式的转换）
        //private void InitSwrContext(AVStream* audioStream)
        //{
        //    _swrContextIntPtr = (IntPtr)ffmpeg.swr_alloc_set_opts(
        //        null,
        //        ffmpeg.av_get_default_channel_layout(TargetChannels), // 目标声道布局
        //        TargetSampleFormat, // 目标采样格式
        //        TargetSampleRate, // 目标采样率
        //        ffmpeg.av_get_default_channel_layout(AudioCodecContext->channels), // 原声道布局
        //        AudioCodecContext->sample_fmt, // 原采样格式
        //        AudioCodecContext->sample_rate, // 原采样率
        //        0,
        //        null
        //    );

        //    if (SwrContext == null || ffmpeg.swr_init(SwrContext) < 0)
        //        throw new ApplicationException("初始化音频重采样上下文失败");
        //}

        //// 初始化NAudio播放设备
        //private void InitWaveOut()
        //{
        //    _waveOut = new WaveOutEvent();
        //    // 定义PCM格式（与目标格式一致）
        //    var waveFormat = WaveFormat.CreateCustomFormat(
        //        WaveFormatEncoding.Pcm,
        //        TargetSampleRate,
        //        TargetChannels,
        //        TargetSampleRate * TargetChannels * 2, // 比特率：44100 * 2 * 2（16位=2字节）
        //        TargetChannels * 2, // 块对齐
        //        16 // 位深度
        //    );

        //    // 自定义PCM数据提供器（从缓冲区读取数据给WaveOut）
        //    var pcmProvider = new BufferedWaveProvider(waveFormat)
        //    {
        //        BufferDuration = TimeSpan.FromSeconds(1) // 1秒缓冲区，避免卡顿
        //    };

        //    // 绑定播放设备
        //    _waveOut.Init(pcmProvider);
        //    _isWaveOutReady = true;
        //}
        //#endregion

        //#region 2. 音频解码与播放（接收VideoEngine分发的数据包）
        //// 从VideoEngine接收音频数据包并解码（需加锁，避免多线程冲突）
        //internal void ProcessAudioPacket(AVPacket* packet, CancellationToken cancellationToken)
        //{
        //    if (State != AudioState.Playing || cancellationToken.IsCancellationRequested)
        //        return;

        //    try
        //    {
        //        // 2.1 发送数据包到解码器
        //        int result = ffmpeg.avcodec_send_packet(AudioCodecContext, packet);
        //        if (result < 0)
        //            throw new AudioException(this, "发送音频包到解码器失败");

        //        // 2.2 循环接收解码后的音频帧
        //        while (result >= 0 && !cancellationToken.IsCancellationRequested)
        //        {
        //            result = ffmpeg.avcodec_receive_frame(AudioCodecContext, AudioFrame);
        //            if (result == ffmpeg.AVERROR(ffmpeg.EAGAIN) || result == ffmpeg.AVERROR_EOF)
        //                break; // 暂时无数据或结束，退出循环
        //            if (result < 0)
        //                throw new AudioException(this, "接收音频帧失败");

        //            // 2.3 转换PTS为毫秒（关键：用于同步）
        //            UpdateCurrentPts(AudioFrame, _formatContext->streams[AudioStreamIndex]);

        //            // 2.4 重采样音频帧为目标PCM格式
        //            var pcmData = ResampleAudioFrame(AudioFrame);

        //            // 2.5 将PCM数据写入缓冲区，NAudio自动播放
        //            if (pcmData != null && _isWaveOutReady)
        //            {
        //                ((BufferedWaveProvider)_waveOut.OutputWaveFormat).AddSamples(pcmData, 0, pcmData.Length);
        //                if (_waveOut.PlaybackState != PlaybackState.Playing)
        //                    _waveOut.Play();
        //            }

        //            // 2.6 释放帧引用
        //            ffmpeg.av_frame_unref(AudioFrame);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new AudioException(this, "处理音频帧失败", ex);
        //    }
        //}

        //// 重采样音频帧为目标PCM格式（16位/44100Hz/立体声）
        //private byte[]? ResampleAudioFrame(AVFrame* frame)
        //{
        //    if (SwrContext == null)
        //        return null;

        //    // 计算重采样后的样本数
        //    int outSamples = ffmpeg.swr_get_out_samples(SwrContext, frame->nb_samples);
        //    int bufferSize = ffmpeg.av_samples_get_buffer_size(
        //        null,
        //        TargetChannels,
        //        outSamples,
        //        TargetSampleFormat,
        //        1
        //    );

        //    if (bufferSize <= 0)
        //        return null;

        //    // 分配PCM缓冲区
        //    byte[] pcmBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        //    fixed (byte* pcmPtr = pcmBuffer)
        //    {
        //        // 重采样
        //        int result = ffmpeg.swr_convert(
        //            SwrContext,
        //            &pcmPtr, // 输出PCM数据
        //            outSamples,
        //            (const byte**)frame->data, // 输入音频帧数据
        //            frame->nb_samples
        //        );

        //        if (result < 0)
        //        {
        //            ArrayPool<byte>.Shared.Return(pcmBuffer);
        //            return null;
        //        }
        //    }

        //    // 截取有效数据（避免缓冲区多余空间）
        //    byte[] resultData = new byte[bufferSize];
        //    Array.Copy(pcmBuffer, resultData, bufferSize);
        //    ArrayPool<byte>.Shared.Return(pcmBuffer);
        //    return resultData;
        //}

        //// 更新当前音频PTS（转换为毫秒）
        //private void UpdateCurrentPts(AVFrame* frame, AVStream* audioStream)
        //{
        //    if (frame->pts == ffmpeg.AV_NOPTS_VALUE)
        //        return;

        //    // PTS转换公式：pts * 时间基（av_q2d将AVRational转为double） * 1000（转为毫秒）
        //    double ptsSec = frame->pts * av_q2d(audioStream->time_base);
        //    Volatile.Write(ref _currentPtsMs, (long)(ptsSec * 1000));
        //}
        //#endregion

        //#region 3. 音频控制（播放/暂停/停止，类比Video类）
        //public void StartPlayback()
        //{
        //    if (State == AudioState.Playing || !_isWaveOutReady)
        //        return;

        //    State = AudioState.Playing;
        //    OnStateChanged(AudioState.Playing);
        //    _waveOut.Play();
        //}

        //public void PausePlayback()
        //{
        //    if (State != AudioState.Playing)
        //        return;

        //    State = AudioState.Paused;
        //    OnStateChanged(AudioState.Paused);
        //    _waveOut.Pause();
        //}

        //public void StopPlayback()
        //{
        //    if (State == AudioState.Stopped)
        //        return;

        //    State = AudioState.Stopped;
        //    OnStateChanged(AudioState.Stopped);
        //    _waveOut.Stop();

        //    // 重置PTS和缓冲区
        //    Volatile.Write(ref _currentPtsMs, 0);
        //    ((BufferedWaveProvider)_waveOut.OutputWaveFormat).ClearBuffer();

        //    // 重置解码器（用于重新播放）
        //    if (AudioCodecContext != null)
        //        ffmpeg.avcodec_flush_buffers(AudioCodecContext);
        //}
        //#endregion

        //#region 辅助方法
        //private void OnStateChanged(AudioState newState)
        //{
        //    State = newState;
        //    StateChanged?.Invoke(this, newState);
        //}
        //#endregion

        //#region 资源释放
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        _cts?.Cancel();
        //        _cts?.Dispose();
        //        _decodeTask?.Wait();
        //        _waveOut?.Dispose();
        //    }

        //    // 释放FFmpeg资源
        //    if (_swrContextIntPtr != IntPtr.Zero)
        //    {
        //        ffmpeg.swr_free(&SwrContext);
        //        _swrContextIntPtr = IntPtr.Zero;
        //    }

        //    if (_audioFrameIntPtr != IntPtr.Zero)
        //    {
        //        ffmpeg.av_frame_free(&AudioFrame);
        //        _audioFrameIntPtr = IntPtr.Zero;
        //    }

        //    if (_audioPacketIntPtr != IntPtr.Zero)
        //    {
        //        ffmpeg.av_packet_free(&AudioPacket);
        //        _audioPacketIntPtr = IntPtr.Zero;
        //    }

        //    if (_audioCodecContextIntPtr != IntPtr.Zero)
        //    {
        //        ffmpeg.avcodec_free_context(&AudioCodecContext);
        //        _audioCodecContextIntPtr = IntPtr.Zero;
        //    }

        //    base.Dispose(disposing);
        //}
        //#endregion
    }

    // 音频状态（类比VideoState）
    public enum AudioState
    {
        Uninitialized,
        Playing,
        Paused,
        Stopped,
        Completed
    }

    // 音频信息（存储采样率、声道数等）
    public class AudioInfo
    {
        public string Uri { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public AVSampleFormat SampleFormat { get; set; }
        public double DurationMs { get; set; } // 音频总时长（毫秒）
    }

    // 音频异常（类比VideoException）
    public class AudioException : Exception
    {
        public Audio Audio { get; }
        public AudioException(Audio audio, string message, Exception innerException)
            : base(message, innerException)
        {
            Audio = audio;
        }
    }
}
