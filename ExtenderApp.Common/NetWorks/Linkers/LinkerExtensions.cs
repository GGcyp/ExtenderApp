using System.Buffers;
using System.Net;
using System.Runtime.InteropServices;

using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks
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
        /// <remarks>
        /// - 当前实现注册 TCP 相关组件（调用
        ///   <c>services.AddTcpLinker()</c>）； <br/>
        /// - 可根据需要扩展为注册更多协议的链接器。
        /// </remarks>
        public static IServiceCollection AddLinker(this IServiceCollection services)
        {
            services.AddTcpLinker();
            services.AddUdpLinker();
            return services;
        }

        #region Send

        /// <summary>
        /// 同步发送一段只读内存数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="memory">要发送的数据窗口。</param>
        /// <returns>发送结果（包含已发送字节数和可能的底层错误）。</returns>
        /// <remarks>
        /// 这是对 <see
        /// cref="ILinker.Send(Memory{byte})"/> 的便捷包装。
        /// </remarks>
        public static SocketOperationResult Send(this ILinker linker, in ReadOnlyMemory<byte> memory)
        {
            return linker.Send(MemoryMarshal.AsMemory(memory));
        }

        /// <summary>
        /// 同步发送 <see cref="ByteBlock"/> 中尚未读取的数据，并按发送量推进其读取位置。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="block">
        /// 字节块；方法内部会根据发送量调用 <see cref="ByteBlock.ReadAdvance(int)"/>。
        /// </param>
        /// <returns>发送结果。</returns>
        public static SocketOperationResult Send(this ILinker linker, ref ByteBlock block)
        {
            var result = linker.Send(block.UnreadMemory);
            block.ReadAdvance(result.BytesTransferred);
            return result;
        }

        /// <summary>
        /// 同步发送 <see cref="ByteBuffer"/> 中尚未读取的数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="buffer">
        /// 只读缓冲视图；其 <see
        /// cref="ByteBuffer.UnreadSequence"/> 将被发送。
        /// </param>
        /// <returns>发送结果（累加所有分段的已发送字节数与最后一次结果的端点/标志）。</returns>
        /// <remarks>
        /// - TCP：内部处理“部分发送”，会在每段上循环直至耗尽。 <br/>
        /// - UDP：按分段逐帧发送；若需单帧发送，请先合并为单块再调用。
        /// </remarks>
        public static SocketOperationResult Send(this ILinker linker, ByteBuffer buffer)
        {
            return linker.Send(buffer.UnreadSequence);
        }

        /// <summary>
        /// 同步发送一个只读字节序列（可能由多段组成）。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="sequence">只读字节序列，将按顺序发送。</param>
        /// <returns>发送结果（累加所有分段的已发送字节数与最后一次结果的端点/标志）。</returns>
        /// <remarks>
        /// - TCP：处理“部分发送”，对每个分段进行补发直到耗尽。 <br/>
        /// - UDP：按分段逐帧发送；若需保持单报文，请先合并为单块。
        /// </remarks>
        public static SocketOperationResult Send(this ILinker linker, ReadOnlySequence<byte> sequence)
        {
            int total = 0;
            SocketOperationResult last = SocketOperationResult.Empty;

            foreach (var segment in sequence)
            {
                var remaining = segment;
                while (remaining.Length > 0)
                {
                    last = linker.Send(remaining);

                    if (last.SocketError != null)
                        return new SocketOperationResult(total, last.RemoteEndPoint, last.SocketError, default);

                    if (last.BytesTransferred <= 0)
                        return new SocketOperationResult(total, last.RemoteEndPoint, last.SocketError, default);

                    total += last.BytesTransferred;

                    if (last.BytesTransferred < remaining.Length)
                        remaining = remaining.Slice(last.BytesTransferred);
                    else
                        break;
                }
            }

            return new SocketOperationResult(total, last.RemoteEndPoint, null, last.ReceiveMessageFromPacketInfo);
        }

        /// <summary>
        /// 异步发送一段只读内存数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="memory">要发送的数据窗口。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果任务（包含已发送字节数和可能的底层错误）。</returns>
        /// <remarks>
        /// 这是对 <see
        /// cref="ILinker.SendAsync(Memory{byte},
        /// CancellationToken)"/> 的便捷包装。
        /// </remarks>
        public static ValueTask<SocketOperationResult> SendAsync(this ILinker linker, in ReadOnlyMemory<byte> memory, CancellationToken token = default)
        {
            return linker.SendAsync(MemoryMarshal.AsMemory(memory), token);
        }

        /// <summary>
        /// 异步发送一段只读跨度数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="span">要发送的数据跨度。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果任务。</returns>
        /// <remarks>
        /// 内部会创建临时 <see cref="ByteBlock"/> 承载数据，并在发送完成后释放。
        /// </remarks>
        public static ValueTask<SocketOperationResult> SendAsync(this ILinker linker, in ReadOnlySpan<byte> span, CancellationToken token = default)
        {
            ByteBlock block = new(span);
            var task = Task.Run(async () =>
            {
                var result = await linker.SendAsync(block, token).ConfigureAwait(false);
                block.Dispose();
                return result;
            });
            return new ValueTask<SocketOperationResult>(task);
        }

        /// <summary>
        /// 异步发送 <see cref="ByteBlock"/> 中尚未读取的数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="block">字节块（只读视图将被发送；不在本方法内推进读指针或释放）。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果任务。</returns>
        public static ValueTask<SocketOperationResult> SendAsync(this ILinker linker, ByteBlock block, CancellationToken token = default)
        {
            return linker.SendAsync(block.UnreadMemory, token);
        }

        /// <summary>
        /// 异步发送 <see cref="ByteBuffer"/> 中尚未读取的数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="buffer">
        /// 只读缓冲视图；其 <see
        /// cref="ByteBuffer.UnreadSequence"/> 将被发送。
        /// </param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果任务（累加所有分段的已发送字节数与最后一次结果的端点/标志）。</returns>
        /// <remarks>
        /// - TCP：内部处理“部分发送”，对每段循环直至耗尽。 <br/>
        /// - UDP：按分段逐帧发送；若需单帧发送，请先合并为单块。
        /// </remarks>
        public static ValueTask<SocketOperationResult> SendAsync(this ILinker linker, ByteBuffer buffer, CancellationToken token = default)
        {
            return linker.SendAsync(buffer.UnreadSequence, token);
        }

        /// <summary>
        /// 异步发送一个只读字节序列（可能由多段组成）。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="sequence">只读字节序列，将按顺序发送。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果任务（累加所有分段的已发送字节数与最后一次结果的端点/标志）。</returns>
        /// <remarks>
        /// - TCP：处理“部分发送”，对每个分段进行补发直到耗尽。 <br/>
        /// - UDP：按分段逐帧发送；若需保持单报文，请先合并为单块。
        /// </remarks>
        public static async ValueTask<SocketOperationResult> SendAsync(this ILinker linker, ReadOnlySequence<byte> sequence, CancellationToken token = default)
        {
            int total = 0;
            SocketOperationResult last = SocketOperationResult.Empty;

            foreach (var segment in sequence)
            {
                var remaining = segment;
                while (remaining.Length > 0)
                {
                    last = await linker.SendAsync(remaining, token).ConfigureAwait(false);

                    if (last.SocketError != null)
                        return new SocketOperationResult(total, last.RemoteEndPoint, last.SocketError, default);

                    if (last.BytesTransferred <= 0)
                        return new SocketOperationResult(total, last.RemoteEndPoint, last.SocketError, default);

                    total += last.BytesTransferred;

                    if (last.BytesTransferred < remaining.Length)
                        remaining = remaining.Slice(last.BytesTransferred);
                    else
                        break;
                }
            }

            return new SocketOperationResult(total, last.RemoteEndPoint, null, last.ReceiveMessageFromPacketInfo);
        }

        /// <summary>
        /// 异步发送一个 <see cref="ByteBuffer"/> 中的未读数据到指定 UDP 远端（SendTo 语义）。
        /// </summary>
        /// <typeparam name="TLinker">UDP 链接器类型。</typeparam>
        /// <param name="linker">目标 UDP 链接器。</param>
        /// <param name="buffer">源缓冲（ref struct，不在方法内释放；仅复制未读部分）。</param>
        /// <param name="endPoint">目标远端终结点。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>一次发送操作的结果。</returns>
        /// <exception cref="ArgumentNullException">Buffer 为空（IsEmpty）。</exception>
        /// <remarks>
        /// - 内部将把 <paramref name="buffer"/> 克隆为 <see cref="ByteBlock"/>，避免直接处理 ref struct；
        ///   发送结束后自动释放克隆。<br/>
        /// - 若需要避免额外复制，可先手动合并为 <see cref="ByteBlock"/> 调用另一个重载。<br/>
        /// - 仅发送未读部分（UnreadSequence）；不推进原缓冲读指针。
        /// </remarks>
        public static ValueTask<SocketOperationResult> SendToAsync<TLinker>(this TLinker linker, ByteBuffer buffer, EndPoint endPoint, CancellationToken token = default)
            where TLinker : IUdpLinker
        {
            if (buffer.IsEmpty)
                throw new ArgumentNullException(nameof(buffer));

            ByteBlock block = new(buffer);
            return PrivateSendToAsync(linker, block, endPoint, token);
        }

        /// <summary>
        /// 私有辅助：发送并释放临时 <see cref="ByteBlock"/>。
        /// </summary>
        /// <typeparam name="TLinker">UDP 链接器类型。</typeparam>
        /// <param name="linker">链接器实例。</param>
        /// <param name="block">待发送的临时块。</param>
        /// <param name="endPoint">目标远端。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果。</returns>
        /// <remarks>
        /// - 若取消或失败，仍确保 <paramref name="block"/> 被释放。<br/>
        /// - 此方法不做参数校验，由外层调用保证。
        /// </remarks>
        private static async ValueTask<SocketOperationResult> PrivateSendToAsync<TLinker>(this TLinker linker, ByteBlock block, EndPoint endPoint, CancellationToken token = default)
            where TLinker : IUdpLinker
        {
            var result = await linker.SendToAsync(block.UnreadMemory, endPoint, token);
            block.Dispose();
            return result;
        }

        /// <summary>
        /// 异步发送一个现有 <see cref="ByteBlock"/>（未读数据）到指定 UDP 远端。
        /// </summary>
        /// <typeparam name="TLinker">UDP 链接器类型。</typeparam>
        /// <param name="linker">目标链接器。</param>
        /// <param name="block">数据块（未在此方法内释放或推进读取位置）。</param>
        /// <param name="endPoint">目标远端终结点。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果。</returns>
        /// <exception cref="ArgumentNullException">block 为空（IsEmpty）。</exception>
        /// <remarks>
        /// - 仅读取 <see cref="ByteBlock.UnreadMemory"/>；不修改读指针。 <br/>
        /// - 若需要发送后自动释放，请调用上层封装或自行在外部 finally 里释放。
        /// </remarks>
        public static ValueTask<SocketOperationResult> SendToAsync<TLinker>(this TLinker linker, ByteBlock block, EndPoint endPoint, CancellationToken token = default)
            where TLinker : IUdpLinker
        {
            if (block.IsEmpty)
                throw new ArgumentNullException(nameof(block));

            return linker.SendToAsync(block.UnreadMemory, endPoint, token);
        }

        /// <summary>
        /// 异步发送一段只读内存数据到指定 UDP 远端（无需显式构造 ByteBlock）。
        /// </summary>
        /// <typeparam name="TLinker">UDP 链接器类型。</typeparam>
        /// <param name="linker">目标链接器。</param>
        /// <param name="memory">要发送的只读内存。</param>
        /// <param name="endPoint">目标远端终结点。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果。</returns>
        /// <remarks>
        /// - 内部使用 <see cref="MemoryMarshal.AsMemory{T}(ReadOnlyMemory{T})"/> 转换为可写视图后调用底层。<br/>
        /// - 对大数据或频繁调用场景，可考虑预分配缓冲以减少复制。
        /// </remarks>
        public static ValueTask<SocketOperationResult> SendToAsync<TLinker>(this TLinker linker, in ReadOnlyMemory<byte> memory, EndPoint endPoint, CancellationToken token = default)
            where TLinker : IUdpLinker
        {
            return linker.SendToAsync(MemoryMarshal.AsMemory(memory), endPoint, token);
        }

        #endregion Send

        #region Receive

        /// <summary>
        /// 同步接收数据到 ByteBuffer。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="buffer">
        /// 写入目标（ref struct，不能用于 async 方法）。
        /// </param>
        /// <param name="sizeHint">
        /// 期望接收的最小容量提示；内部将调用 <see
        /// cref="ByteBuffer.GetMemory(int)"/> 申请写缓冲。
        /// </param>
        /// <returns>
        /// 接收结果；TCP 下 BytesTransferred 为 0
        /// 通常表示对端优雅关闭；UDP 下可能存在截断标志（取决于实现）。
        /// </returns>
        /// <remarks>
        /// 由于 <see cref="ByteBuffer"/> 为 ref
        /// struct，无法在真正的异步方法中使用；因此提供同步 API。
        /// </remarks>
        public static SocketOperationResult Receive(this ILinker linker, ref ByteBuffer buffer, int sizeHint = 1024)
        {
            var memory = buffer.GetMemory(sizeHint);
            var result = linker.Receive(memory);
            buffer.WriteAdvance(result.BytesTransferred);
            return result;
        }

        #endregion Receive
    }
}