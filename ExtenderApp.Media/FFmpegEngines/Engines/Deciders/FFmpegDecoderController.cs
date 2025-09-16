using ExtenderApp.Common;
using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 解码控制器。
    /// 管理解码流程的启动、停止、资源释放及解码状态同步，支持异步解码和取消操作。
    /// 适用于音视频流的多线程解码场景。
    /// </summary>
    public class FFmpegDecoderController : DisposableObject
    {
        /// <summary>
        /// FFmpeg 引擎实例。
        /// </summary>
        private readonly FFmpegEngine _engine;

        /// <summary>
        /// 当前解码上下文。
        /// </summary>
        private readonly FFmpegContext _context;

        /// <summary>
        /// 解码器集合，包含视频和音频解码器。
        /// </summary>
        private readonly FFmpegDecoderCollection _decoderCollection;

        /// <summary>
        /// 全局取消令牌源，用于控制解码流程的终止。
        /// </summary>
        private readonly CancellationTokenSource _allSource;

        /// <summary>
        /// 解码状态控制器，用于同步解码状态和缓存空间。
        /// </summary>
        private readonly StateController<bool> _decodingController;

        /// <summary>
        /// 解码处理任务。
        /// </summary>
        private Task? processTask;

        /// <summary>
        /// 当前解码流程的取消令牌源。
        /// </summary>
        private CancellationTokenSource? source;

        #region Events

        /// <summary>
        /// 解码完成事件（如遇到 EOF 时触发）。
        /// </summary>
        public EventHandler? OnCompletedDecoded;

        #endregion

        /// <summary>
        /// 包含媒体流的基本信息（如时长、格式等）。
        /// </summary>
        public FFmpegInfo Info => _context.Info;

        /// <summary>
        /// 初始化 FFmpegDecoderController 实例。
        /// </summary>
        /// <param name="engine">FFmpeg 引擎实例。</param>
        /// <param name="context">解码上下文。</param>
        /// <param name="collection">解码器集合。</param>
        /// <param name="source">全局取消令牌源。</param>
        public FFmpegDecoderController(FFmpegEngine engine, FFmpegContext context, FFmpegDecoderCollection collection, CancellationTokenSource source)
        {
            _engine = engine;
            _allSource = source;
            _decoderCollection = collection;
            _context = context;
            _decodingController = new(true, () => _decoderCollection.VideoDecoder.CacheStateController.HasCacheSpace);
        }

        /// <summary>
        /// 启动解码流程。
        /// 若已启动则抛出异常，自动创建取消令牌并异步执行解码任务。
        /// </summary>
        public void StartDecode()
        {
            if (processTask != null)
            {
                throw new InvalidOperationException($"不能重复解析:{_context.Info.Uri}");
            }
            source = source ?? CancellationTokenSource.CreateLinkedTokenSource(_allSource.Token);
            processTask = Task.Run(() => Decoding(source.Token), _allSource.Token);
        }

        /// <summary>
        /// 停止解码流程，取消任务并释放相关资源。
        /// </summary>
        public void Stop()
        {
            source?.Cancel();
            source?.Dispose();
            processTask?.Dispose();
            processTask = null;
            source = null;
        }

        /// <summary>
        /// 解码主循环任务。
        /// 持续读取数据包并分发到对应解码器，支持异步解码和取消。
        /// 解码完成或遇到错误时触发 OnCompletedDecoded 事件。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>已完成的任务。</returns>
        private Task Decoding(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !_allSource.IsCancellationRequested)
            {
                if (!_decodingController.WaitForTargetState(token))
                {
                    break;
                }

                NativeIntPtr<AVPacket> packet = _engine.GetPacket();
                int result = _engine.ReadPacket(_context.FormatContext, ref packet);
                if (result < 0)
                {
                    // 读取完毕，退出循环
                    if (result == ffmpeg.AVERROR_EOF)
                    {
                        OnCompletedDecoded?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        _engine.ShowError("读取帧失败", result);
                    }
                    break;
                }
                else if (_engine.IsTryAgain(result))
                {
                    // 重试读取
                    continue;
                }

                int index = _engine.GetPacketStreamIndex(packet);
                var decoder = GetDecoder(index);
                decoder?.Decoding(packet);
                _engine.ReturnPacket(ref packet);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 根据流索引获取对应的解码器（视频或音频）。
        /// </summary>
        /// <param name="streamIndex">流索引。</param>
        /// <returns>对应的 FFmpegDecoder 实例。</returns>
        /// <exception cref="ArgumentOutOfRangeException">流索引无效时抛出异常。</exception>
        public FFmpegDecoder? GetDecoder(int streamIndex)
        {
            if (_decoderCollection.VideoDecoder?.StreamIndex == streamIndex)
            {
                return _decoderCollection.VideoDecoder;
            }
            else if (_decoderCollection.AudioDecoder?.StreamIndex == streamIndex)
            {
                return _decoderCollection.AudioDecoder;
            }
            return null;
            //throw new ArgumentOutOfRangeException(nameof(streamIndex), $"无法识别的流索引:{streamIndex}");
        }

        public void OnVideoFrameAdded()
        {
            _decoderCollection.VideoDecoder?.CacheStateController.OnFrameAdded();
        }

        public void OnAudioFrameAdded()
        {
            _decoderCollection.AudioDecoder?.CacheStateController.OnFrameAdded();
        }

        public void OnVideoFrameRemoved()
        {
            _decoderCollection.VideoDecoder?.CacheStateController.OnFrameRemoved();
        }

        public void OnAudioFrameRemoved()
        {
            _decoderCollection.AudioDecoder?.CacheStateController.OnFrameRemoved();
        }

        /// <summary>
        /// 释放解码控制器相关资源。
        /// 包括取消任务、释放上下文和等待解码任务完成。
        /// </summary>
        /// <param name="disposing">指示是否由 Dispose 方法调用。</param>
        protected override void Dispose(bool disposing)
        {
            source?.Cancel();
            source?.Dispose();
            processTask?.Wait();
            processTask?.Dispose();
            _context.Dispose();
        }
    }
}
