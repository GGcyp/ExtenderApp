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
        /// 在等待缓存空间时，状态检查的最大超时时间（毫秒）。
        /// </summary>
        private const int MaxTimeout = 10;

        /// <summary>
        /// FFmpeg 引擎实例，用于执行底层的 FFmpeg 操作。
        /// </summary>
        private readonly FFmpegEngine _engine;

        /// <summary>
        /// 解码状态控制器，用于在解码器缓存已满时阻塞读取线程。
        /// </summary>
        private readonly StateController<bool> _decodingController;

        /// <summary>
        /// 当前媒体文件的 FFmpeg 上下文。
        /// </summary>
        private FFmpegContext _context;

        /// <summary>
        /// 存储所有正在运行的解码任务的数组。
        /// </summary>
        private Task[]? processTasks;

        /// <summary>
        /// 用于控制当前解码会话（启动到停止/跳转）的取消令牌源。
        /// </summary>
        private CancellationTokenSource? source;

        /// <summary>
        /// 获取解码器集合，其中包含此控制器管理的所有解码器（例如，视频和音频）。
        /// </summary>
        public FFmpegDecoderCollection DecoderCollection { get; }

        /// <summary>
        /// 获取全局取消令牌源，用于终止控制器的整个生命周期。
        /// </summary>
        public CancellationTokenSource AllSource { get; }

        #region Events

        /// <summary>
        /// 当读取到文件末尾（EOF）时触发的事件。
        /// </summary>
        public event EventHandler? OnCompletedDecoded;

        #endregion Events

        /// <summary>
        /// 获取当前媒体流的基本信息（如时长、格式等）。
        /// </summary>
        public FFmpegInfo Info => _context.Info;

        /// <summary>
        /// 初始化 <see cref="FFmpegDecoderController"/> 类的新实例。
        /// </summary>
        /// <param name="engine">FFmpeg 引擎实例。</param>
        /// <param name="context">FFmpeg 上下文。</param>
        /// <param name="collection">解码器集合。</param>
        /// <param name="allSource">用于控制整个控制器生命周期的全局取消令牌源。</param>
        public FFmpegDecoderController(FFmpegEngine engine, FFmpegContext context, FFmpegDecoderCollection collection, CancellationTokenSource allSource)
        {
            _engine = engine;
            DecoderCollection = collection;
            _context = context;
            _decodingController = new(true, () => DecoderCollection.GetHasCacheSpace());
            AllSource = allSource;
        }

        /// <summary>
        /// 启动解码流程。
        /// 此方法会为数据包读取和每个解码器创建一个后台任务。如果解码已在运行，则会抛出 <see cref="InvalidOperationException"/>。
        /// </summary>
        /// <exception cref="InvalidOperationException">当解码任务已在运行时调用此方法。</exception>
        public void StartDecode()
        {
            ThrowIfDisposed();
            if (processTasks != null)
            {
                throw new InvalidOperationException($"不能重复解析:{_context.Info.MediaUri}");
            }
            source = CancellationTokenSource.CreateLinkedTokenSource(AllSource.Token);

            int length = DecoderCollection.Length + 1;
            processTasks = new Task[length];
            processTasks[0] = Task.Run(() => Decoding(source.Token), AllSource.Token);

            for (int i = 1; i < length; i++)
            {
                var decoder = DecoderCollection[i - 1];
                processTasks[i] = Task.Run(() => ProcessPacket(decoder, source.Token), AllSource.Token);
            }
        }

        /// <summary>
        /// 异步停止当前解码流程。
        /// 此方法会取消所有正在运行的解码任务并等待它们完成。
        /// </summary>
        /// <returns>一个表示异步停止操作的任务。</returns>
        public Task StopDecodeAsync()
        {
            ThrowIfDisposed();
            source?.Cancel();
            source?.Dispose();

            source = null;
            return WaitForAllTasksComplete();
        }

        /// <summary>
        /// 异步跳转到媒体流的指定位置。
        /// 此方法会先停止当前解码，然后执行跳转操作，并清空所有解码器的内部缓存。
        /// </summary>
        /// <param name="position">目标跳转时间（毫秒）。</param>
        public async Task SeekDecoderAsync(long position)
        {
            ThrowIfDisposed();

            await StopDecodeAsync();

            var collection = _context.ContextCollection;
            DecoderCollection.Flush();

            _engine.Seek(_context.FormatContext, position, _context);
        }

        /// <summary>
        /// 解码主循环任务，负责从媒体文件读取数据包并分发到相应的解码器队列。
        /// </summary>
        /// <param name="token">用于取消操作的取消令牌。</param>
        /// <returns>一个表示异步解码操作的任务。</returns>
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
                DecoderCollection[index].EnqueuePacket(packet);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 持续处理单个解码器的数据包队列的后台任务。
        /// </summary>
        /// <param name="decoder">要处理的解码器。</param>
        /// <param name="token">用于取消操作的取消令牌。</param>
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
        /// 异步等待所有正在运行的解码任务完成。
        /// </summary>
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

        /// <summary>
        /// 释放由 <see cref="FFmpegDecoderController"/> 占用的托管资源。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            AllSource.Cancel();
            AllSource.Dispose();
            source?.Cancel();
            source?.Dispose();

            DecoderCollection.Dispose();
            WaitForAllTasksComplete().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 释放由 <see cref="FFmpegDecoderController"/> 占用的非托管资源。
        /// </summary>
        protected override void DisposeUnmanagedResources()
        {
            _context.Dispose();
            DecoderCollection.Dispose();
        }
    }
}