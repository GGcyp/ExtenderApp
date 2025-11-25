using System.Collections.Concurrent;
using ExtenderApp.Data;
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
        /// 全局取消令牌源，用于控制解码流程的终止。
        /// </summary>
        private readonly CancellationTokenSource _allSource;

        /// <summary>
        /// 解码状态控制器，用于同步解码状态和缓存空间。
        /// </summary>
        private readonly StateController<bool> _decodingController;

        /// <summary>
        /// 当前解码上下文。
        /// </summary>
        private FFmpegContext _context;

        /// <summary>
        /// 解码处理任务。
        /// </summary>
        private Task? processTask;

        /// <summary>
        /// 音频解码任务。
        /// </summary>
        private Task? audioTask;

        /// <summary>
        /// 视频解码任务。
        /// </summary>
        private Task? videoTask;

        /// <summary>
        /// 当前解码流程的取消令牌源。
        /// </summary>
        private CancellationTokenSource? source;

        /// <summary>
        /// 音频数据包队列。 用于缓存待解码的音频
        /// AVPacket，支持多线程安全入队和出队。 解码主循环将音频包分发到该队列，由音频解码器异步处理。
        /// </summary>
        private readonly ConcurrentQueue<NativeIntPtr<AVPacket>> _audioQueue;

        /// <summary>
        /// 视频数据包队列。 用于缓存待解码的视频
        /// AVPacket，支持多线程安全入队和出队。 解码主循环将视频包分发到该队列，由视频解码器异步处理。
        /// </summary>
        private readonly ConcurrentQueue<NativeIntPtr<AVPacket>> _videoQueue;

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
        /// <param name="source">全局取消令牌源。</param>
        public FFmpegDecoderController(FFmpegEngine engine, FFmpegContext context, FFmpegDecoderCollection collection, CancellationTokenSource source)
        {
            _audioQueue = new();
            _videoQueue = new();

            _engine = engine;
            _allSource = source;
            _decoderCollection = collection;
            _context = context;
            _decodingController = new(false, () => _videoQueue.Count > 1 && _audioQueue.Count > 1);
        }

        /// <summary>
        /// 启动解码流程。 若已启动则抛出异常，自动创建取消令牌并异步执行解码任务。
        /// </summary>
        public void StartDecode()
        {
            ThrowIfDisposed();
            if (processTask != null)
            {
                throw new InvalidOperationException($"不能重复解析:{_context.Info.MediaUri}");
            }
            source = source ?? CancellationTokenSource.CreateLinkedTokenSource(_allSource.Token);
            processTask = Task.Run(() => Decoding(source.Token), _allSource.Token);
            videoTask = Task.Run(() => ProcessPacket(_videoQueue, _decoderCollection.VideoDecoder, source.Token), _allSource.Token);
            audioTask = Task.Run(() => ProcessPacket(_audioQueue, _decoderCollection.AudioDecoder, source.Token), _allSource.Token);
        }

        /// <summary>
        /// 停止解码流程，取消所有解码相关任务并释放资源。
        /// 包括主解码任务、音频解码任务、视频解码任务的取消和等待，
        /// 以及相关取消令牌的释放。若所有任务均为空则直接返回。
        /// 任务取消异常会被忽略，确保资源安全释放。
        /// </summary>
        public async Task StopDecodeAsync()
        {
            ThrowIfDisposed();
            source?.Cancel();
            source?.Dispose();

            Task? ptask = processTask;
            Task? atask = audioTask;
            Task? vtask = videoTask;
            processTask = null;
            audioTask = null;
            videoTask = null;
            source = null;

            if (ptask == null && atask == null && vtask == null)
                return;

            try
            {
                var tasks = new List<Task>();
                if (ptask != null) tasks.Add(ptask);
                if (atask != null) tasks.Add(atask);
                if (vtask != null) tasks.Add(vtask);
                if (tasks.Count > 0)
                    await Task.WhenAll(tasks);

                ptask?.Dispose();
                atask?.Dispose();
                vtask?.Dispose();
            }
            catch (Exception ex)
            {
                // 忽略任务取消引发的异常
            }

            //Task.Run(async () =>
            //{
            //    if (ptask != null)
            //        await ptask;
            //    if (atask != null)
            //        await atask;
            //    if (vtask != null)
            //        await vtask;

            //    ptask?.Dispose();
            //    atask?.Dispose();
            //    vtask?.Dispose();
            //});
        }

        /// <summary>
        /// 跳转解码器到指定时间戳（毫秒）。
        /// 先停止当前解码流程，刷新解码器集合缓存，
        /// 调用 FFmpegEngine 的 Seek 方法将媒体流定位到目标时间点，
        /// 然后清空音视频数据包队列，实现解码器的时间跳转和状态重置。
        /// 适用于音视频播放进度跳转、快进/快退等场景。
        /// </summary>
        /// <param name="position">目标跳转时间（毫秒）。</param>
        public async Task SeekDecoderAsync(long position)
        {
            ThrowIfDisposed();
            await Task.Run(async () =>
            {
                await StopDecodeAsync();

                var collection = _context.ContextCollection;
                _decoderCollection.Flush();

                _engine.Seek(_context.FormatContext, position, _context);
                Clear();
            });
        }

        /// <summary>
        /// 解码主循环任务。 持续读取数据包并分发到对应解码器，支持异步解码和取消。
        /// 解码完成或遇到错误时触发 OnCompletedDecoded 事件。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>已完成的任务。</returns>
        private Task Decoding(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !_allSource.IsCancellationRequested)
            {
                if (!_decodingController.WaitForTargetState(token, MaxTimeout))
                {
                    continue;
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
                var queue = GetQueue(index);
                queue?.Enqueue(packet);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 根据流索引获取对应的解码器（视频或音频）。
        /// </summary>
        /// <param name="streamIndex">流索引。</param>
        /// <returns>对应的 FFmpegDecoder 实例。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 流索引无效时抛出异常。
        /// </exception>
        private ConcurrentQueue<NativeIntPtr<AVPacket>>? GetQueue(int streamIndex)
        {
            if (_decoderCollection.VideoDecoder?.StreamIndex == streamIndex)
            {
                return _videoQueue;
            }
            else if (_decoderCollection.AudioDecoder?.StreamIndex == streamIndex)
            {
                return _audioQueue;
            }
            return null;
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

        /// <summary>
        /// 解码器数据包处理任务。 持续从指定队列中取出 AVPacket
        /// 并交由解码器进行解码， 解码完成后回收数据包资源。支持异步等待和取消。 适用于音视频解码的多线程处理场景。
        /// </summary>
        /// <param name="queue">待处理的数据包队列（音频或视频）。</param>
        /// <param name="decoder">对应的解码器实例。</param>
        private async Task ProcessPacket(ConcurrentQueue<NativeIntPtr<AVPacket>> queue, FFmpegDecoder decoder, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!queue.TryDequeue(out var packet))
                {
                    try
                    {
                        await Task.Delay(MaxTimeout, token);
                    }
                    catch (TaskCanceledException ex)
                    {
                        break;
                    }
                    continue;
                }
                decoder.Decoding(packet, token);
                _engine.ReturnPacket(ref packet);
            }
        }

        /// <summary>
        /// 清除音视频数据包队列，释放所有未处理的 AVPacket 资源。
        /// </summary>
        private void Clear()
        {
            while (_audioQueue.TryDequeue(out var packet))
            {
                _engine.ReturnPacket(ref packet);
            }
            while (_videoQueue.TryDequeue(out var packet))
            {
                _engine.ReturnPacket(ref packet);
            }
        }

        /// <summary>
        /// 释放解码控制器相关资源。 包括取消任务、释放上下文和等待解码任务完成。
        /// </summary>
        /// <param name="disposing">
        /// 指示是否由 Dispose 方法调用。
        /// </param>
        protected override void Dispose(bool disposing)
        {
            _allSource.Cancel();
            _allSource.Dispose();
            source?.Cancel();
            source?.Dispose();
            processTask?.Wait();
            audioTask?.Wait();
            videoTask?.Wait();
            Clear();
            processTask?.Dispose();
            audioTask?.Dispose();
            videoTask?.Dispose();
            processTask = null;
            audioTask = null;
            videoTask = null;
            source = null;
            _context.Dispose();
            _decoderCollection.Dispose();
        }
    }
}