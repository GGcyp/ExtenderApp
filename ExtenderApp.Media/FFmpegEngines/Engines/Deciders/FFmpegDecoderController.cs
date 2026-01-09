using System.Runtime.CompilerServices;
using ExtenderApp.Common;
using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines.Decoders;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 解码控制器。
    /// 管理解码流程的启动、停止、跳转、资源释放及解码状态同步，支持异步解码和取消操作。
    /// 适用于音视频流的多线程解码场景。
    /// </summary>
    public class FFmpegDecoderController : DisposableObject
    {
        /// <summary>
        /// 控制器运行上下文，封装了引擎、媒体上下文、全局取消令牌及代际（generation）管理等状态。
        /// </summary>
        private FFmpegDecoderControllerContext _controllerContext;

        /// <summary>
        /// FFmpeg 引擎实例，用于执行底层 FFmpeg 操作（读包、Seek、Flush、对象池等）。
        /// </summary>
        private FFmpegEngine Engine => _controllerContext.Engine;

        /// <summary>
        /// 当前媒体的 FFmpeg 上下文（格式上下文、媒体信息等）。
        /// </summary>
        private FFmpegContext Context => _controllerContext.Context;

        /// <summary>
        /// 当前解码会话创建的后台任务集合。
        /// <para>索引 0 通常为读包分发循环，其余为各解码器的解码循环。</para>
        /// </summary>
        private Task[]? processTasks;

        /// <summary>
        /// 目标跳转位置（毫秒）。
        /// <para>该值会在解码循环中读取并用于 <see cref="SeekPrivate"/>。</para>
        /// </summary>
        private long seekTagetPosition;

        /// <summary>
        /// 当前解码会话（从 Start 到 Stop/Seek）的取消令牌源。
        /// </summary>
        private CancellationTokenSource? source;

        /// <summary>
        /// 获取解码器集合，包含该控制器管理的所有解码器（例如视频、音频）。
        /// </summary>
        public FFmpegDecoderCollection DecoderCollection { get; }

        /// <summary>
        /// 解码控制器设置。
        /// </summary>
        public FFmpegDecoderSettings Settings { get; }

        /// <summary>
        /// 获取控制器的全局取消令牌（来自上下文的 AllSource）。
        /// </summary>
        public CancellationToken Token => _controllerContext.AllSource.Token;

        #region Events

        /// <summary>
        /// 当读取到文件末尾（EOF）时触发。
        /// <para>注意：在读包线程触发，若订阅方涉及 UI 更新需要自行切换线程。</para>
        /// </summary>
        public event Action<FFmpegDecoderController>? OnCompletedDecoded;

        #endregion Events

        /// <summary>
        /// 获取当前媒体流的基本信息（如时长、格式、URI 等）。
        /// </summary>
        public FFmpegInfo Info => Context.Info;

        /// <summary>
        /// 初始化 <see cref="FFmpegDecoderController"/> 的新实例。
        /// </summary>
        /// <param name="collection">解码器集合。</param>
        /// <param name="controllerContext">控制器上下文。</param>
        /// <param name="settings">解码设置。</param>
        public FFmpegDecoderController(FFmpegDecoderCollection collection, FFmpegDecoderControllerContext controllerContext, FFmpegDecoderSettings settings)
        {
            DecoderCollection = collection;
            Settings = settings;
            _controllerContext = controllerContext;
        }

        #region Operation

        /// <summary>
        /// 启动解码流程。
        /// <para>该方法会创建读包分发任务以及每个解码器的解码任务。</para>
        /// </summary>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        /// <exception cref="InvalidOperationException">重复启动，或控制器已被取消。</exception>
        public void StartDecode()
        {
            ThrowIfDisposed();
            if (processTasks != null)
            {
                throw new InvalidOperationException($"不能重复解析:{Context.Info.MediaUri}");
            }

            var token = _controllerContext.AllSource.Token;
            if (token.IsCancellationRequested)
            {
                throw new InvalidOperationException($"控制器已被取消，不能解析:{Context.Info.MediaUri}");
            }

            source = CancellationTokenSource.CreateLinkedTokenSource(token);
            StartDecodingPrivate();
        }

        /// <summary>
        /// 异步停止当前解码流程。
        /// <para>会取消当前会话并等待所有后台任务退出，然后清理相关资源。</para>
        /// </summary>
        /// <returns>表示异步停止操作的任务。</returns>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        public async Task StopDecodeAsync()
        {
            ThrowIfDisposed();

            source?.Cancel();
            await WaitForAllTasksComplete().ConfigureAwait(false);

            source?.Dispose();
            source = null;
        }

        /// <summary>
        /// 请求跳转到媒体流的指定位置（毫秒）。
        /// <para>通过代际（generation）机制通知解码循环执行 Seek。</para>
        /// <para>若当前未在解码（<see cref="processTasks"/> 为 null），则立即 Seek。</para>
        /// </summary>
        /// <param name="position">目标时间（毫秒）。</param>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        public void SeekDecoder(long position)
        {
            ThrowIfDisposed();

            Interlocked.Exchange(ref seekTagetPosition, position);
            _controllerContext.IncrementGeneration();

            if (processTasks == null)
            {
                SeekPrivate();
            }
        }

        /// <summary>
        /// 创建并启动读包循环与各解码器解码循环任务。
        /// </summary>
        private void StartDecodingPrivate()
        {
            int length = DecoderCollection.Count + 1;

            int decoderIndex = 0;
            int threadIndex = 1;

            var token = source!.Token;

            processTasks = new Task[length];
            processTasks[0] = Task.Run(DecodingLoop, token);

            while (threadIndex < length)
            {
                var decoder = DecoderCollection[decoderIndex];
                processTasks[threadIndex] = decoder.DecodeLoopAsync(token);

                threadIndex++;
                decoderIndex++;
            }
        }

        /// <summary>
        /// 读包/分发主循环。
        /// <para>负责从媒体源读取 <see cref="AVPacket"/>，并按流索引分发到对应解码器队列。</para>
        /// <para>当检测到代际变化时执行 <see cref="SeekPrivate"/> 并继续读取。</para>
        /// </summary>
        private void DecodingLoop()
        {
            var token = source!.Token;

            var localGeneration = _controllerContext.GetCurrentGeneration();
            var currentGeneration = localGeneration;

            while (!token.IsCancellationRequested)
            {
                currentGeneration = _controllerContext.GetCurrentGeneration();
                if (localGeneration != currentGeneration)
                {
                    SeekPrivate();
                    localGeneration = currentGeneration;
                    continue;
                }

                var packetPtr = Engine.GetPacket();
                int result = ReadPacket(packetPtr);
                if (result == -1)
                {
                    break;
                }
                else if (result == -2)
                {
                    continue;
                }

                int index = Engine.GetPacketStreamIndex(packetPtr);
                var decoder = DecoderCollection[index];

                FFmpegPacket packet = new(currentGeneration, packetPtr);

                try
                {
                    decoder.EnqueuePacket(packet, token);
                }
                catch
                {
                    Engine.Return(packetPtr);
                    throw;
                }
            }
        }

        /// <summary>
        /// 执行 Seek 并刷新 FFmpeg 内部缓冲。
        /// </summary>
        private void SeekPrivate()
        {
            var context = Context.FormatContext;
            var seekTagetPosition = Interlocked.Read(ref this.seekTagetPosition);

            Engine.Seek(context, seekTagetPosition, Context);
            Engine.Flush(ref context);
        }

        #endregion Operation

        #region Packets

        /// <summary>
        /// 读取单个数据包。
        /// </summary>
        /// <param name="packet">待填充的包指针（通常来自引擎对象池）。</param>
        /// <returns>
        /// 0：读取成功。
        /// -1：读取失败或 EOF（应退出循环）。
        /// -2：需要重试（例如 EAGAIN，应继续循环）。
        /// </returns>
        private int ReadPacket(NativeIntPtr<AVPacket> packet)
        {
            int result = Engine.ReadPacket(Context.FormatContext, ref packet);
            if (result < 0)
            {
                if (result == ffmpeg.AVERROR_EOF)
                {
                    OnCompletedDecoded?.Invoke(this);
                }
                else
                {
                    Engine.ShowException("读取帧失败", result);
                }

                Engine.Return(packet);
                return -1;
            }
            else if (Engine.IsTryAgain(result))
            {
                Engine.Return(packet);
                return -2;
            }

            return 0;
        }

        #endregion Packets

        #region Dispose

        /// <summary>
        /// 释放托管资源。
        /// <para>会主动取消会话并等待后台任务退出。</para>
        /// </summary>
        protected override void DisposeManagedResources()
        {
            source?.Cancel();
            _controllerContext.DisposeSafe();
            WaitForAllTasksComplete().GetAwaiter().GetResult();

            source?.DisposeSafe();
        }

        /// <summary>
        /// 释放非托管资源。
        /// </summary>
        protected override void DisposeUnmanagedResources()
        {
            DecoderCollection.DisposeSafe();
        }

        #endregion Dispose

        #region Uitls

        /// <summary>
        /// 获取当前代际（generation）。
        /// <para>用于判断是否发生了 Seek/重置等需要同步的状态变化。</para>
        /// </summary>
        /// <returns>当前代际值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCurrentGeneration()
        {
            return _controllerContext.GetCurrentGeneration();
        }

        /// <summary>
        /// 异步等待所有解码相关任务完成。
        /// <para>无论任务是否取消，最终都会 Flush 解码器并清空任务引用。</para>
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
                // 忽略任务取消异常。
            }
            finally
            {
                DecoderCollection.FlushAll();
                processTasks = null;
            }
        }

        #endregion Uitls
    }
}