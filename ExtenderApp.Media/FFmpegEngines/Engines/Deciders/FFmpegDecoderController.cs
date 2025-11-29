using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines.Decoders;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 解码控制器。
    /// 管理解码流程的启动、停止、资源释放及解码状态同步，支持异步解码和取消操作。 适用于音视频流的多线程解码场景。
    /// </summary>
    public class FFmpegDecoderController : DisposableObject
    {
        /// <summary>
        /// 最大等待超时时间（毫秒），用于控制解码节奏。
        /// </summary>
        private const int MaxTimeout = 10;

        /// <summary>
        /// FFmpeg 引擎实例。
        /// </summary>
        private readonly FFmpegEngine _engine;

        /// <summary>
        /// 解码器集合，包含视频和音频解码器。
        /// </summary>
        private readonly FFmpegDecoderCollection _decoderCollection;

        /// <summary>
        /// 解码状态控制器，用于同步解码状态和缓存空间。
        /// </summary>
        private readonly StateController<bool> _decodingController;

        /// <summary>
        /// 当前解码上下文。
        /// </summary>
        private FFmpegContext _context;

        private Task[]? processTasks;

        /// <summary>
        /// 当前解码流程的取消令牌源。
        /// </summary>
        private CancellationTokenSource? source;

        /// <summary>
        /// 全局取消令牌源，用于控制解码流程的终止。
        /// </summary>
        public CancellationTokenSource AllSource { get; }

        #region Events

        /// <summary>
        /// 解码完成事件（如遇到 EOF 时触发）。
        /// </summary>
        public EventHandler? OnCompletedDecoded;

        #endregion Events

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
        /// <param name="allSource">全局取消令牌源。</param>
        public FFmpegDecoderController(FFmpegEngine engine, FFmpegContext context, FFmpegDecoderCollection collection, CancellationTokenSource allSource)
        {
            _engine = engine;
            _decoderCollection = collection;
            _context = context;
            _decodingController = new(true, () => _decoderCollection.All(d => d.CacheStateController.HasCacheSpace));
            AllSource = allSource;
        }

        /// <summary>
        /// 启动解码流程。 若已启动则抛出异常，自动创建取消令牌并异步执行解码任务。
        /// </summary>
        public void StartDecode()
        {
            ThrowIfDisposed();
            if (processTasks != null)
            {
                throw new InvalidOperationException($"不能重复解析:{_context.Info.MediaUri}");
            }
            source = CancellationTokenSource.CreateLinkedTokenSource(AllSource.Token);

            int length = _decoderCollection.Length + 1;
            processTasks = new Task[length];
            processTasks[0] = Task.Run(() => Decoding(source.Token), AllSource.Token);

            for (int i = 1; i < length; i++)
            {
                var decoder = _decoderCollection[i - 1];
                processTasks[i] = Task.Run(() => ProcessPacket(decoder, source.Token), AllSource.Token);
            }
        }

        /// <summary>
        /// 停止解码流程，取消所有解码相关任务并释放资源。
        /// 包括主解码任务、音频解码任务、视频解码任务的取消和等待，
        /// 以及相关取消令牌的释放。若所有任务均为空则直接返回。 任务取消异常会被忽略，确保资源安全释放。
        /// </summary>
        public Task StopDecodeAsync()
        {
            ThrowIfDisposed();
            source?.Cancel();
            source?.Dispose();

            source = null;
            return WaitForAllTasksComplete();
        }

        /// <summary>
        /// 跳转解码器到指定时间戳（毫秒）。 先停止当前解码流程，刷新解码器集合缓存，
        /// 调用 FFmpegEngine 的 Seek 方法将媒体流定位到目标时间点，
        /// 然后清空音视频数据包队列，实现解码器的时间跳转和状态重置。 适用于音视频播放进度跳转、快进/快退等场景。
        /// </summary>
        /// <param name="position">目标跳转时间（毫秒）。</param>
        public async Task SeekDecoderAsync(long position)
        {
            ThrowIfDisposed();

            await StopDecodeAsync();

            var collection = _context.ContextCollection;
            _decoderCollection.Flush();

            _engine.Seek(_context.FormatContext, position, _context);
        }

        /// <summary>
        /// 解码主循环任务。 持续读取数据包并分发到对应解码器，支持异步解码和取消。
        /// 解码完成或遇到错误时触发 OnCompletedDecoded 事件。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>已完成的任务。</returns>
        private Task Decoding(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !AllSource.IsCancellationRequested)
            {
                _decodingController.WaitForTargetState(MaxTimeout, token);
                if (token.IsCancellationRequested || AllSource.IsCancellationRequested)
                    return Task.CompletedTask;

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
                        _engine.ShowException("读取帧失败", result);
                    }
                    break;
                }
                else if (_engine.IsTryAgain(result))
                {
                    // 重试读取
                    continue;
                }

                int index = _engine.GetPacketStreamIndex(packet);
                _decoderCollection[index].EnqueuePacket(packet);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 通知视频解码器缓存已添加新帧。
        /// 用于更新视频解码器的缓存状态，唤醒等待的解码线程或调整解码节奏。 适用于帧解码完成后调用，确保缓存计数准确。
        /// </summary>
        public void OnVideoFrameAdded()
        {
            _decoderCollection.VideoDecoder?.CacheStateController.OnFrameAdded();
        }

        /// <summary>
        /// 通知音频解码器缓存已添加新帧。
        /// 用于更新音频解码器的缓存状态，唤醒等待的解码线程或调整解码节奏。 适用于帧解码完成后调用，确保缓存计数准确。
        /// </summary>
        public void OnAudioFrameAdded()
        {
            _decoderCollection.AudioDecoder?.CacheStateController.OnFrameAdded();
        }

        /// <summary>
        /// 通知视频解码器缓存已移除帧。
        /// 用于更新视频解码器的缓存状态，唤醒等待的解码线程或调整解码节奏。 适用于帧被消费或回收后调用，确保缓存计数准确。
        /// </summary>
        public void OnVideoFrameRemoved()
        {
            _decoderCollection.VideoDecoder?.CacheStateController.OnFrameRemoved();
        }

        /// <summary>
        /// 通知音频解码器缓存已移除帧。
        /// 用于更新音频解码器的缓存状态，唤醒等待的解码线程或调整解码节奏。 适用于帧被消费或回收后调用，确保缓存计数准确。
        /// </summary>
        public void OnAudioFrameRemoved()
        {
            _decoderCollection.AudioDecoder?.CacheStateController.OnFrameRemoved();
        }

        private async Task ProcessPacket(FFmpegDecoder decoder, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await decoder.ProcessPacket(token);
                }
                catch (TaskCanceledException)
                {
                    // 忽略任务取消异常
                }
            }
        }

        /// <summary>
        /// 可等待所有解码相关任务完成。
        /// </summary>
        /// <returns></returns>
        private async Task WaitForAllTasksComplete()
        {
            if (processTasks == null)
                return;
            try
            {
                await Task.WhenAll(processTasks);
            }
            catch (TaskCanceledException)
            {
                // 忽略任务取消异常
            }
            processTasks = null;
        }

        protected override void DisposeManagedResources()
        {
            AllSource.Cancel();
            AllSource.Dispose();
            source?.Cancel();
            source?.Dispose();

            _decoderCollection.Dispose();
            WaitForAllTasksComplete().GetAwaiter().GetResult();
        }

        protected override void DisposeUnmanagedResources()
        {
            _context.Dispose();
            _decoderCollection.Dispose();
        }
    }
}