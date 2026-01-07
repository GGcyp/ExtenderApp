using System.Diagnostics;
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
        /// FFmpeg 引擎实例，用于执行底层的 FFmpeg 操作。
        /// </summary>
        private readonly FFmpegEngine _engine;

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
        public CancellationTokenSource? Source { get; private set; }

        /// <summary>
        /// 获取解码器集合，其中包含此控制器管理的所有解码器（例如，视频和音频）。
        /// </summary>
        public FFmpegDecoderCollection DecoderCollection { get; }

        public FFmpegDecodeModel DecodeMode { get; private set; }

        public FFmpegDecoderSettings Settings { get; }

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
        public FFmpegDecoderController(FFmpegEngine engine, FFmpegContext context, FFmpegDecoderCollection collection, FFmpegDecoderSettings settings)
        {
            _engine = engine;
            DecoderCollection = collection;
            _context = context;
            Settings = settings;
        }

        #region Operation

        /// <summary>
        /// 启动解码流程。 此方法会为数据包读取和每个解码器创建一个后台任务。如果解码已在运行，则会抛出 <see cref="InvalidOperationException"/>。
        /// </summary>
        /// <param name="decodeMode">解码模式，默认为正常模式。</param>
        /// <param name="threadingModel">解码线程模型，默认为自动选择。</param>
        /// <exception cref="InvalidOperationException">当解码任务已在运行时调用此方法。</exception>
        public void StartDecode(FFmpegDecodeModel decodeMode = FFmpegDecodeModel.Normal)
        {
            ThrowIfDisposed();
            if (processTasks != null)
            {
                throw new InvalidOperationException($"不能重复解析:{_context.Info.MediaUri}");
            }
            var token = _engine.FFmpegToken;
            if (token.IsCancellationRequested)
            {
                throw new InvalidOperationException($"控制器已被取消，不能解析:{_context.Info.MediaUri}");
            }
            Source = CancellationTokenSource.CreateLinkedTokenSource(token);
            DecodeMode = decodeMode;

            PrivateStartDecoding();
        }

        /// <summary>
        /// 异步停止当前解码流程。 此方法会取消所有正在运行的解码任务并等待它们完成。
        /// </summary>
        /// <returns>一个表示异步停止操作的任务。</returns>
        public async Task StopDecodeAsync()
        {
            ThrowIfDisposed();

            Source?.Cancel();
            await WaitForAllTasksComplete();
            Source?.Dispose();
            Source = null;
        }

        /// <summary>
        /// 异步跳转到媒体流的指定位置。 此方法会先停止当前解码，然后执行跳转操作，并清空所有解码器的内部缓存。
        /// </summary>
        /// <param name="position">目标跳转时间（毫秒）。</param>
        public void SeekDecoder(long position)
        {
            ThrowIfDisposed();

            _engine.Seek(_context.FormatContext, position, _context);
        }

        #endregion Operation

        #region ThreadDecodes

        private void PrivateStartDecoding()
        {
            int length = 1;
            int decoderIndex = 0;
            int threadIndex = 1;
            var token = Source!.Token;
            FFmpegDecoder decoder = default!;
            length = DecoderCollection.Count + 1;
            if (DecodeMode != FFmpegDecodeModel.Normal)
            {
                FFmpegMediaType targetType = Convert(DecodeMode);
                length = DecoderCollection.ContainsDecoder(targetType) ? length - 1 : length;
            }

            processTasks = new Task[length];
            processTasks[0] = Task.Run(DecodingLoop, token);

            while (threadIndex < length)
            {
                decoder = DecoderCollection[decoderIndex];
                if (DecodeMode != FFmpegDecodeModel.Normal &&
                    decoder.MediaType == Convert(DecodeMode))
                {
                    decoderIndex++;
                    continue;
                }
                processTasks[threadIndex] = decoder.DecodeLoopAsync(token);
                threadIndex++;
                decoderIndex++;
            }
        }

        /// <summary>
        /// 解码主循环任务，负责从媒体文件读取数据包并分发到相应的解码器队列。
        /// </summary>
        /// <param name="token">用于取消操作的取消令牌。</param>
        /// <returns>一个表示异步解码操作的任务。</returns>
        private void DecodingLoop()
        {
            var token = Source!.Token;
            while (!token.IsCancellationRequested)
            {
                var packetPtr = _engine.GetPacket();
                int result = ReadPacket(packetPtr);
                if (result == -1) break;
                else if (result == -2) continue;

                int index = _engine.GetPacketStreamIndex(packetPtr);
                var decoder = DecoderCollection[index];

                var packet = CreatePacket(packetPtr);

                try
                {
                    decoder.EnqueuePacket(packet, token);
                }
                catch
                {
                    _engine.Return(ref packetPtr);
                    throw;
                }
            }
        }

        #endregion ThreadDecodes

        #region Packets

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

        private FFmpegPacket CreatePacket(NativeIntPtr<AVPacket> ptr)
        {
            return new FFmpegPacket(Settings.GetCurrentGeneration(), ptr);
        }

        #endregion Packets

        #region Dispose

        /// <summary>
        /// 释放由 <see cref="FFmpegDecoderController"/> 占用的托管资源。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            Source?.Cancel();
            WaitForAllTasksComplete().GetAwaiter().GetResult();

            DecoderCollection.DisposeSafe();
            Source?.DisposeSafe();
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

        #endregion Uitls
    }
}