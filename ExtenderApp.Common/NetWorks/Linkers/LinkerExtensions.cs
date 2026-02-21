using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Common.Networks;
using ExtenderApp.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 提供与链接（ILinker）及其依赖注册相关的扩展方法。
    /// </summary>
    public static class LinkerExtensions
    {
        /// <summary>
        /// 向服务集合中添加通用链接相关服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>原服务集合（便于链式调用）。</returns>
        public static IServiceCollection AddLinker(this IServiceCollection services)
        {
            services.AddTcpLinker();
            services.AddUdpLinker();
            return services;
        }

        /// <summary>
        /// 添加指定类型的链接器及其工厂到服务集合中。
        /// </summary>
        /// <typeparam name="TLinker">指定类型连接器</typeparam>
        /// <typeparam name="TLinkerFactory">指定类型连接器工厂</typeparam>
        /// <param name="services">服务集合。</param>
        /// <returns>原服务集合（便于链式调用）。</returns>
        public static IServiceCollection AddLinker<TLinker, TLinkerFactory>(this IServiceCollection services)
            where TLinker : class, ILinker
            where TLinkerFactory : class, ILinkerFactory<TLinker>
        {
            services.AddSingleton<ILinkerFactory<TLinker>, TLinkerFactory>();
            services.AddTransient(provider =>
            {
                return provider.GetRequiredService<ILinkerFactory<TLinker>>().CreateLinker();
            });
            return services;
        }

        /// <summary>
        /// 创建并返回当前链接器实例的一个强类型副本。
        /// </summary>
        /// <typeparam name="T">链接器的具体类型，必须实现 ILinker。</typeparam>
        /// <param name="linker">要克隆的链接器实例。</param>
        /// <returns>返回类型为 <typeparamref name="T"/> 的新链接器实例。</returns>
        public static T Clone<T>(this T linker) where T : ILinker
        {
            return (T)linker.Clone();
        }

        #region Send

        /// <summary>
        /// 同步发送抽象缓冲区的数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="buffer">待发送的缓冲区。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>发送结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<LinkOperationValue> Send(this ILinker linker, AbstractBuffer<byte> buffer, LinkFlags flags = LinkFlags.None)
        {
            if (buffer is SequenceBuffer<byte> sequenceBuffer)
                return SendPrivate(linker, sequenceBuffer, flags);
            else if (buffer is MemoryBlock<byte> memoryBlock)
                return SendPrivate(linker, memoryBlock, flags);
            else
                throw new ArgumentException("不支持的缓冲区类型。", nameof(buffer));
        }

        /// <summary>
        /// 同步发送 <see cref="MemoryBlock{T}"/> 中已提交的数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="memoryBlock">内存块。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>发送结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<LinkOperationValue> Send(this ILinker linker, MemoryBlock<byte> memoryBlock, LinkFlags flags = LinkFlags.None)
        {
            ArgumentNullException.ThrowIfNull(linker, nameof(linker));
            ArgumentNullException.ThrowIfNull(memoryBlock, nameof(memoryBlock));

            return SendPrivate(linker, memoryBlock, flags);
        }

        /// <summary>
        /// 同步发送 <see cref="SequenceBuffer{T}"/> 中的所有分段数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="sequenceBuffer">序列缓冲区。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>发送结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<LinkOperationValue> Send(this ILinker linker, SequenceBuffer<byte> sequenceBuffer, LinkFlags flags = LinkFlags.None)
        {
            ArgumentNullException.ThrowIfNull(linker, nameof(linker));
            ArgumentNullException.ThrowIfNull(sequenceBuffer, nameof(sequenceBuffer));

            return SendPrivate(linker, sequenceBuffer, flags);
        }

        /// <summary>
        /// 同步发送 <see cref="MemoryBlock{T}"/> 的已提交数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="memoryBlock">内存块。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>发送结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Result<LinkOperationValue> SendPrivate(ILinker linker, MemoryBlock<byte> memoryBlock, LinkFlags flags)
        {
            if (memoryBlock.Committed == 0)
                return Result.Failure<LinkOperationValue>("当前 MemoryBlock 中没有数据可发送。");
            return linker.Send(memoryBlock.CommittedMemory, flags);
        }

        /// <summary>
        /// 同步发送 <see cref="SequenceBuffer{T}"/> 的所有分段数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="sequenceBuffer">序列缓冲区。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>发送结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Result<LinkOperationValue> SendPrivate(ILinker linker, SequenceBuffer<byte> sequenceBuffer, LinkFlags flags)
        {
            var segments = sequenceBuffer.ToArraySegments();
            if (segments == null)
                return Result.Failure<LinkOperationValue>("当前 SequenceBuffer 中没有数据可发送。");
            return linker.Send(segments, flags);
        }

        #endregion Send

        #region SendAsync

        /// <summary>
        /// 异步发送抽象缓冲区的数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="buffer">待发送的缓冲区。</param>
        /// <param name="flags">发送标志。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<Result<LinkOperationValue>> SendAsync(this ILinker linker, AbstractBuffer<byte> buffer, LinkFlags flags = LinkFlags.None, CancellationToken token = default)
        {
            if (buffer is SequenceBuffer<byte> sequenceBuffer)
                return SendAsyncPrivate(linker, sequenceBuffer, flags, token);
            else if (buffer is MemoryBlock<byte> memoryBlock)
                return SendAsyncPrivate(linker, memoryBlock, flags, token);
            else
                throw new ArgumentException("不支持的缓冲区类型。", nameof(buffer));
        }

        /// <summary>
        /// 异步发送 <see cref="MemoryBlock{T}"/> 中已提交的数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="memoryBlock">内存块。</param>
        /// <param name="flags">发送标志。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<Result<LinkOperationValue>> SendAsync(this ILinker linker, MemoryBlock<byte> memoryBlock, LinkFlags flags = LinkFlags.None, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(linker, nameof(linker));
            ArgumentNullException.ThrowIfNull(memoryBlock, nameof(memoryBlock));

            return SendAsyncPrivate(linker, memoryBlock, flags, token);
        }

        /// <summary>
        /// 异步发送 <see cref="SequenceBuffer{T}"/> 中的所有分段数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="sequenceBuffer">序列缓冲区。</param>
        /// <param name="flags">发送标志。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<Result<LinkOperationValue>> SendAsync(this ILinker linker, SequenceBuffer<byte> sequenceBuffer, LinkFlags flags = LinkFlags.None, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(linker, nameof(linker));
            ArgumentNullException.ThrowIfNull(sequenceBuffer, nameof(sequenceBuffer));

            return SendAsyncPrivate(linker, sequenceBuffer, flags, token);
        }

        /// <summary>
        /// 异步发送 <see cref="MemoryBlock{T}"/> 的已提交数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="memoryBlock">内存块。</param>
        /// <param name="flags">发送标志。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ValueTask<Result<LinkOperationValue>> SendAsyncPrivate(this ILinker linker, MemoryBlock<byte> memoryBlock, LinkFlags flags, CancellationToken token)
        {
            if (memoryBlock.Committed == 0)
                return Result.Failure<LinkOperationValue>("当前 MemoryBlock 中没有数据可发送。");

            return linker.SendAsync(memoryBlock.CommittedMemory, flags, token);
        }

        /// <summary>
        /// 异步发送 <see cref="SequenceBuffer{T}"/> 的所有分段数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="sequenceBuffer">序列缓冲区。</param>
        /// <param name="flags">发送标志。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ValueTask<Result<LinkOperationValue>> SendAsyncPrivate(this ILinker linker, SequenceBuffer<byte> sequenceBuffer, LinkFlags flags, CancellationToken token)
        {
            var segments = sequenceBuffer.ToArraySegments();
            if (segments == null)
                return Result.Failure<LinkOperationValue>("当前 SequenceBuffer 中没有数据可发送。");

            return linker.SendAsync(segments, flags, token);
        }

        #endregion SendAsync

        #region Receive

        /// <summary>
        /// 同步接收数据到抽象缓冲区。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="buffer">用于接收的缓冲区。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>接收结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<LinkOperationValue> Receive(this ILinker linker, AbstractBuffer<byte> buffer, LinkFlags flags = LinkFlags.None)
        {
            if (buffer is SequenceBuffer<byte> sequenceBuffer)
                return ReceivePrivate(linker, sequenceBuffer, flags);
            else if (buffer is MemoryBlock<byte> memoryBlock)
                return ReceivePrivate(linker, memoryBlock, flags);
            else
                throw new ArgumentException("不支持的缓冲区类型。", nameof(buffer));
        }

        /// <summary>
        /// 同步接收数据到 <see cref="MemoryBlock{T}"/> 的可用空间。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="memoryBlock">内存块。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>接收结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<LinkOperationValue> Receive(this ILinker linker, MemoryBlock<byte> memoryBlock, LinkFlags flags = LinkFlags.None)
        {
            ArgumentNullException.ThrowIfNull(linker, nameof(linker));
            ArgumentNullException.ThrowIfNull(memoryBlock, nameof(memoryBlock));

            return ReceivePrivate(linker, memoryBlock, flags);
        }

        /// <summary>
        /// 同步接收数据到 <see cref="SequenceBuffer{T}"/> 的可用空间。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="sequenceBuffer">序列缓冲区。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>接收结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<LinkOperationValue> Receive(this ILinker linker, SequenceBuffer<byte> sequenceBuffer, LinkFlags flags = LinkFlags.None)
        {
            ArgumentNullException.ThrowIfNull(linker, nameof(linker));
            ArgumentNullException.ThrowIfNull(sequenceBuffer, nameof(sequenceBuffer));

            return ReceivePrivate(linker, sequenceBuffer, flags);
        }

        /// <summary>
        /// 同步接收数据到 <see cref="MemoryBlock{T}"/> 的可用空间。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="memoryBlock">内存块。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>接收结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Result<LinkOperationValue> ReceivePrivate(ILinker linker, MemoryBlock<byte> memoryBlock, LinkFlags flags)
        {
            var writableSpan = memoryBlock.GetAvailableSpan();
            if (writableSpan.IsEmpty || writableSpan.Length == 0)
                return ReceiveFailureResult(nameof(MemoryBlock<byte>));
            return linker.Receive(writableSpan, flags);
        }

        /// <summary>
        /// 同步接收数据到 <see cref="SequenceBuffer{T}"/> 的可用空间。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="sequenceBuffer">序列缓冲区。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>接收结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Result<LinkOperationValue> ReceivePrivate(ILinker linker, SequenceBuffer<byte> sequenceBuffer, LinkFlags flags)
        {
            var writableSequenceBuffer = sequenceBuffer.AvailableSlice();
            if (writableSequenceBuffer.Available == 0)
            {
                writableSequenceBuffer.TryRelease();
                return ReceiveFailureResult(nameof(SequenceBuffer<byte>));
            }
            var writableList = writableSequenceBuffer.ToArraySegments();
            if (writableList == null)
            {
                writableSequenceBuffer.TryRelease();
                return ReceiveFailureResult(nameof(SequenceBuffer<byte>));
            }
            writableSequenceBuffer.Freeze();
            var result = linker.Receive(writableList, flags);
            writableSequenceBuffer.TryRelease();
            return result;
        }

        #endregion Receive

        #region ReceiveAsync

        /// <summary>
        /// 异步接收数据到抽象缓冲区。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="buffer">用于接收的缓冲区。</param>
        /// <param name="flags">接收标志。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>接收结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<Result<LinkOperationValue>> ReceiveAsync(this ILinker linker, AbstractBuffer<byte> buffer, LinkFlags flags = LinkFlags.None, CancellationToken token = default)
        {
            if (buffer is SequenceBuffer<byte> sequenceBuffer)
                return ReceiveAsyncPrivate(linker, sequenceBuffer, flags, token);
            else if (buffer is MemoryBlock<byte> memoryBlock)
                return ReceiveAsyncPrivate(linker, memoryBlock, flags, token);
            else
                throw new ArgumentException("不支持的缓冲区类型。", nameof(buffer));
        }

        /// <summary>
        /// 异步接收数据到 <see cref="MemoryBlock{T}"/> 的可用空间。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="memoryBlock">内存块。</param>
        /// <param name="flags">接收标志。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>接收结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<Result<LinkOperationValue>> ReceiveAsync(this ILinker linker, MemoryBlock<byte> memoryBlock, LinkFlags flags = LinkFlags.None, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(linker, nameof(linker));
            ArgumentNullException.ThrowIfNull(memoryBlock, nameof(memoryBlock));

            return ReceiveAsyncPrivate(linker, memoryBlock, flags, token);
        }

        /// <summary>
        /// 异步接收数据到 <see cref="SequenceBuffer{T}"/> 的可用空间。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="sequenceBuffer">序列缓冲区。</param>
        /// <param name="flags">接收标志。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>接收结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<Result<LinkOperationValue>> ReceiveAsync(this ILinker linker, SequenceBuffer<byte> sequenceBuffer, LinkFlags flags = LinkFlags.None, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(linker, nameof(linker));
            ArgumentNullException.ThrowIfNull(sequenceBuffer, nameof(sequenceBuffer));

            return ReceiveAsyncPrivate(linker, sequenceBuffer, flags, token);
        }

        /// <summary>
        /// 异步接收数据到 <see cref="MemoryBlock{T}"/> 的可用空间。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="memoryBlock">内存块。</param>
        /// <param name="flags">接收标志。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>接收结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ValueTask<Result<LinkOperationValue>> ReceiveAsyncPrivate(ILinker linker, MemoryBlock<byte> memoryBlock, LinkFlags flags, CancellationToken token)
        {
            var writableMemory = memoryBlock.GetAvailableMemory();
            if (writableMemory.IsEmpty || writableMemory.Length == 0)
                return ReceiveFailureResult(nameof(MemoryBlock<byte>));

            return linker.ReceiveAsync(writableMemory, flags);
        }

        /// <summary>
        /// 异步接收数据到 <see cref="SequenceBuffer{T}"/> 的可用空间。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="sequenceBuffer">序列缓冲区。</param>
        /// <param name="flags">接收标志。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>接收结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ValueTask<Result<LinkOperationValue>> ReceiveAsyncPrivate(ILinker linker, SequenceBuffer<byte> sequenceBuffer, LinkFlags flags, CancellationToken token)
        {
            var writableSequenceBuffer = sequenceBuffer.AvailableSlice();
            if (writableSequenceBuffer.Available == 0)
            {
                writableSequenceBuffer.TryRelease();
                return ReceiveFailureResult(nameof(SequenceBuffer<byte>));
            }
            var writableList = writableSequenceBuffer.ToArraySegments();
            if (writableList == null)
            {
                writableSequenceBuffer.TryRelease();
                return ReceiveFailureResult(nameof(SequenceBuffer<byte>));
            }
            writableSequenceBuffer.Freeze();
            var result = linker.ReceiveAsync(writableList, flags, token);
            writableSequenceBuffer.TryRelease();
            return result;
        }

        #endregion ReceiveAsync

        /// <summary>
        /// 构造接收失败的结果信息。
        /// </summary>
        /// <param name="bufferName">缓冲区类型名称。</param>
        /// <returns>失败结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Result<LinkOperationValue> ReceiveFailureResult(string bufferName)
        {
            return Result.Failure<LinkOperationValue>($"当前 {bufferName} 中没有可用空间来接收数据。");
        }
    }
}