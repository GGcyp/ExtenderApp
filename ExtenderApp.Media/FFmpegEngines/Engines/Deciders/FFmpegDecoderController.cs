using System.Runtime.CompilerServices;
using ExtenderApp.Common;
using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines.Decoders;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 解码控制器。 管理解码流程的启动、停止、资源释放及解码状态同步，支持异步解码和取消操作。 适用于音视频流的多线程解码场景。
    /// </summary>
    public class FFmpegDecoderController : DisposableObject
    {
        /// <summary>
        /// 在等待缓存空间时，状态检查的最大超时时间（毫秒）。
        /// </summary>
        private const int MaxTimeout = 5;

        /// <summary>
        /// 默认启用多线程解码的高度阈值（像素）。
        /// </summary>
        private const int DefaultMultiThreadingHeight = 1080;

        /// <summary>
        /// 默认启用多线程解码的宽度阈值（像素）。
        /// </summary>
        private const int DefaultMultiThreadingWidth = 1920;

        /// <summary>
        /// 启用多线程解码的总像素阈值 (1920 * 1080)。
        /// </summary>
        private const int MultiThreadingPixelThreshold = DefaultMultiThreadingHeight * DefaultMultiThreadingWidth;

        /// <summary>
        /// 启用多线程解码的帧率阈值。
        /// </summary>
        private const double MultiThreadingFrameRateThreshold = 50.0;

        /// <summary>
        /// FFmpeg 引擎实例，用于执行底层的 FFmpeg 操作。
        /// </summary>
        private readonly FFmpegEngine _engine;

        private readonly object _seekLock;

        /// <summary>
        /// 解码状态控制器，用于在解码器缓存已满时阻塞读取线程。
        /// </summary>
        private readonly CacheStateController<bool> _cacheStateController;

        private readonly ManualResetEventSlim _seekResetEvent;
        private CountdownEvent _pauseAckEvent;

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

        public FFmpegDecodeThreadingModel DecodeThreadingModel { get; private set; }

        public FFmpegDecodeModel DecodeMode { get; private set; }

        #region Events

        /// <summary>
        /// 当读取到文件末尾（EOF）时触发的事件。
        /// </summary>
        public event Action<FFmpegDecoderController>? OnCompletedDecoded;

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
            _cacheStateController = new(true, () => DecoderCollection.GetHasPacketCacheSpace());
            AllSource = allSource;
            _seekResetEvent = new(true);
            _pauseAckEvent = new(1);
            _seekLock = new();
        }

        #region Operation

        /// <summary>
        /// 启动解码流程。 此方法会为数据包读取和每个解码器创建一个后台任务。如果解码已在运行，则会抛出 <see cref="InvalidOperationException"/>。
        /// </summary>
        /// <param name="decodeMode">解码模式，默认为正常模式。</param>
        /// <param name="threadingModel">解码线程模型，默认为自动选择。</param>
        /// <exception cref="InvalidOperationException">当解码任务已在运行时调用此方法。</exception>
        public void StartDecode(FFmpegDecodeModel decodeMode = FFmpegDecodeModel.Normal, FFmpegDecodeThreadingModel threadingModel = FFmpegDecodeThreadingModel.Auto)
        {
            ThrowIfDisposed();
            if (processTasks != null)
            {
                throw new InvalidOperationException($"不能重复解析:{_context.Info.MediaUri}");
            }
            if (AllSource.Token.IsCancellationRequested)
            {
                throw new InvalidOperationException($"控制器已被取消，不能解析:{_context.Info.MediaUri}");
            }
            source = CancellationTokenSource.CreateLinkedTokenSource(AllSource.Token);
            this.DecodeMode = decodeMode;

            PrivateStartDecoding(threadingModel);
        }

        /// <summary>
        /// 异步停止当前解码流程。 此方法会取消所有正在运行的解码任务并等待它们完成。
        /// </summary>
        /// <returns>一个表示异步停止操作的任务。</returns>
        public async Task StopDecodeAsync()
        {
            ThrowIfDisposed();

            source?.Cancel();
            await WaitForAllTasksComplete();
            source?.Dispose();
            source = null;
        }

        /// <summary>
        /// 异步跳转到媒体流的指定位置。 此方法会先停止当前解码，然后执行跳转操作，并清空所有解码器的内部缓存。
        /// </summary>
        /// <param name="position">目标跳转时间（毫秒）。</param>
        public void SeekDecoder(long position)
        {
            ThrowIfDisposed();

            lock (_seekLock)
            {
                PrivateSeekDecoder(position);
            }
        }

        private void PrivateSeekDecoder(long position)
        {
            // 必须确保有任务在运行，否则不需要同步
            int threadCount = processTasks?.Length ?? 0;
            if (threadCount == 0)
            {
                _engine.Seek(_context.FormatContext, position, _context);
                return;
            }

            try
            {
                _pauseAckEvent.Reset(threadCount);
                _seekResetEvent.Reset();

                if (source == null || source.IsCancellationRequested)
                    return;
                DecoderCollection.ReleaseAllWait();
                _cacheStateController.ReleaseWait();

                if (source == null || source.IsCancellationRequested)
                    return;
                _pauseAckEvent.Wait(source?.Token ?? CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                if (source != null && !source.IsCancellationRequested)
                {
                    _engine.Seek(_context.FormatContext, position, _context);
                    _cacheStateController.Reset();
                    DecoderCollection.FlushAll();
                    _seekResetEvent.Set();
                }
            }
        }

        #endregion Operation

        #region ThreadDecodes

        private void PrivateStartDecoding(FFmpegDecodeThreadingModel threadingModel)
        {
            int length = 1;
            int decoderIndex = 0;
            int threadIndex = 1;
            var token = source!.Token;
            FFmpegDecoder decoder = default!;
            switch (threadingModel)
            {
                case FFmpegDecodeThreadingModel.Single:
                    processTasks = new Task[length];
                    processTasks[0] = Task.Run(SingleThreadDecoding, token);
                    break;

                case FFmpegDecodeThreadingModel.Hybrid:
                    if (DecodeMode != FFmpegDecodeModel.Normal ||
                        DecoderCollection.GetDecoder(FFmpegMediaType.AUDIO) == null ||
                        DecoderCollection.GetDecoder(FFmpegMediaType.VIDEO) == null)
                    {
                        PrivateStartDecoding(FFmpegDecodeThreadingModel.Single);
                        return;
                    }

                    length = 2;
                    processTasks = new Task[length];
                    processTasks[0] = Task.Run(HybridDecodingLoop, token);

                    decoder = DecoderCollection.GetDecoder(FFmpegMediaType.VIDEO)!;
                    processTasks[1] = Task.Run(() => ProcessPacket(decoder, token), token);
                    break;

                case FFmpegDecodeThreadingModel.Multi:
                    length = DecoderCollection.Length + 1;
                    if (DecodeMode != FFmpegDecodeModel.Normal)
                    {
                        FFmpegMediaType targetType = Convert(DecodeMode);
                        length = DecoderCollection.ContainsDecoder(targetType) ? length - 1 : length;
                    }

                    processTasks = new Task[length];
                    processTasks[0] = Task.Run(MultithreadingDecoding, token);

                    while (threadIndex < length)
                    {
                        decoder = DecoderCollection[decoderIndex];
                        if (DecodeMode != FFmpegDecodeModel.Normal &&
                            decoder.MediaType == Convert(DecodeMode))
                        {
                            decoderIndex++;
                            continue;
                        }
                        processTasks[threadIndex] = Task.Run(() => ProcessPacket(decoder, token), token);
                        threadIndex++;
                        decoderIndex++;
                    }
                    break;

                case FFmpegDecodeThreadingModel.Auto:
                default:
                    threadingModel = GetThreadingModel();
                    PrivateStartDecoding(threadingModel);
                    return;
            }
            DecodeThreadingModel = threadingModel;
        }

        /// <summary>
        /// 检查是否需要暂停，并处理确认逻辑。
        /// </summary>
        /// <returns>如果未取消，则返回 true；否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WaitPause(CancellationToken token)
        {
            if (_seekResetEvent.IsSet)
            {
                return true;
            }

            _pauseAckEvent.Signal();
            try
            {
                _seekResetEvent.Wait(token);
            }
            catch (OperationCanceledException)
            {
                // 忽略取消异常，由调用方处理
                return false;
            }
            return true;
        }

        /// <summary>
        /// 单线程解码主循环，负责从媒体文件读取数据包并分发到相应的解码器队列。
        /// </summary>
        private void SingleThreadDecoding()
        {
            var token = source!.Token;

            while (!token.IsCancellationRequested)
            {
                if (!WaitPause(token))
                    continue;

                FFmpegDecoder decoder;
                if (!DecoderCollection.GetHasPacketCacheSpace())
                {
                    for (int i = 0; i < DecoderCollection.Length; i++)
                    {
                        decoder = DecoderCollection[i];
                        decoder.ProcessPacket(token);
                    }
                    token.WaitHandle.WaitOne(MaxTimeout);
                    continue;
                }

                ReadPackets(token);
            }
        }

        private void HybridDecodingLoop()
        {
            var token = source!.Token;
            var audioDecoder = DecoderCollection.GetDecoder(FFmpegMediaType.AUDIO);

            while (!token.IsCancellationRequested)
            {
                if (!WaitPause(token))
                    continue;

                audioDecoder?.ProcessPacket(token);

                if (!_cacheStateController.WaitForTargetState(token, MaxTimeout))
                    continue;

                ReadPackets(token);
            }
        }

        /// <summary>
        /// 解码主循环任务，负责从媒体文件读取数据包并分发到相应的解码器队列。
        /// </summary>
        /// <param name="token">用于取消操作的取消令牌。</param>
        /// <returns>一个表示异步解码操作的任务。</returns>
        private void MultithreadingDecoding()
        {
            var token = source!.Token;

            while (!token.IsCancellationRequested)
            {
                if (!WaitPause(token) ||
                    !_cacheStateController.WaitForTargetState(token, MaxTimeout))
                    continue;

                ReadPackets(token);
            }
        }

        #endregion ThreadDecodes

        #region Packets

        private void ReadPackets(CancellationToken token)
        {
            bool hasPacketCacheSpace = true;
            while (hasPacketCacheSpace)
            {
                var packet = _engine.GetPacket();
                int result = ReadPacket(packet);
                if (result == -1) break;
                if (result == -2) continue;

                int index = _engine.GetPacketStreamIndex(packet);
                var decoder = DecoderCollection[index];

                decoder.EnqueuePacket(packet);
                hasPacketCacheSpace = DecoderCollection.GetHasPacketCacheSpace();
            }
        }

        /// <summary>
        /// 读取单个数据包的方法。 如果读取成功，返回 0；如果读取到文件末尾，返回 -1；如果需要重试读取，返回 -2。
        /// </summary>
        /// <param name="packet">需要读取的包</param>
        /// <returns>返回结果</returns>
        private int ReadPacket(NativeIntPtr<AVPacket> packet)
        {
            int result = _engine.ReadPacket(_context.FormatContext, ref packet);
            if (result < 0)
            {
                // 读取完毕，退出循环
                if (result == ffmpeg.AVERROR_EOF)
                {
                    OnCompletedDecoded?.Invoke(this);
                }
                else
                {
                    _engine.ShowException("读取帧失败", result);
                }
                _engine.Return(ref packet);
                return -1;
            }
            else if (_engine.IsTryAgain(result))
            {
                // 重试读取
                _engine.Return(ref packet);
                return -2;
            }
            return 0;
        }

        /// <summary>
        /// 持续处理单个解码器的数据包队列的后台任务。
        /// </summary>
        /// <param name="decoder">要处理的解码器。</param>
        /// <param name="token">用于取消操作的取消令牌。</param>
        private void ProcessPacket(FFmpegDecoder decoder, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!WaitPause(token)) continue;
                decoder.WaitProcessPacket(token);
            }
        }

        #endregion Packets

        #region Dispose

        /// <summary>
        /// 释放由 <see cref="FFmpegDecoderController"/> 占用的托管资源。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            AllSource.Cancel();
            source?.Cancel();
            WaitForAllTasksComplete().GetAwaiter().GetResult();

            DecoderCollection.DisposeSafe();
            AllSource.DisposeSafe();
            source?.DisposeSafe();
            _seekResetEvent.DisposeSafe();
            _pauseAckEvent.DisposeSafe();
        }

        /// <summary>
        /// 释放由 <see cref="FFmpegDecoderController"/> 占用的非托管资源。
        /// </summary>
        protected override void DisposeUnmanagedResources()
        {
            _context.Dispose();
            DecoderCollection.Dispose();
        }

        #endregion Dispose

        #region Uitls

        /// <summary>
        /// 异步等待所有正在运行的解码任务完成。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task WaitForAllTasksComplete()
        {
            if (processTasks == null)
                return;
            try
            {
                await Task.WhenAll(processTasks).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // 忽略任务取消异常
            }
            finally
            {
                DecoderCollection.FlushAll();
                processTasks = null;
            }
        }

        /// <summary>
        /// 将解码模式转换为对应的媒体类型。
        /// </summary>
        /// <param name="mode">解码模式。</param>
        /// <returns>对应的媒体类型。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private FFmpegMediaType Convert(FFmpegDecodeModel mode)
        {
            return mode switch
            {
                FFmpegDecodeModel.AudioOnly => FFmpegMediaType.AUDIO,
                FFmpegDecodeModel.VideoOnly => FFmpegMediaType.VIDEO,
                _ => FFmpegMediaType.UNKNOWN,
            };
        }

        /// <summary>
        /// 根据视频信息决定使用的解码线程模型。
        /// </summary>
        /// <returns>适用的解码线程模型。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private FFmpegDecodeThreadingModel GetThreadingModel()
        {
            // 如果没有视频流，或者视频规格很低，使用单线程
            if (!DecoderCollection.ContainsDecoder(FFmpegMediaType.VIDEO)
                || (Info.Width * Info.Height < 1280 * 720))
            {
                return FFmpegDecodeThreadingModel.Single;
            }

            // 对于高规格视频，使用完全多线程
            if (ShouldUseMultithreading())
            {
                return FFmpegDecodeThreadingModel.Multi;
            }

            // 其他情况（如720p, 普通1080p）使用混合模式，这是对低性能PC的优化
            return FFmpegDecodeThreadingModel.Hybrid;
        }

        /// <summary>
        /// 根据视频信息决定是否应使用多线程解码。
        /// </summary>
        /// <returns>如果应使用多线程，则为 true；否则为 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ShouldUseMultithreading()
        {
            // 检查总像素数
            if (Info.Width * Info.Height >= MultiThreadingPixelThreshold)
            {
                return true;
            }

            // 检查帧率
            if (Info.Rate >= MultiThreadingFrameRateThreshold)
            {
                return true;
            }

            // 检查是否为计算密集型编码
            return Info.VideoCodecName.Contains("hevc", StringComparison.OrdinalIgnoreCase) ||
                   Info.VideoCodecName.Contains("av1", StringComparison.OrdinalIgnoreCase);
        }

        #endregion Uitls
    }
}