using System.Buffers;
using System.Net;
using System.Runtime.InteropServices;
using ExtenderApp.Abstract;
using ExtenderApp.Contracts;
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
        /// 同步发送一段只读内存数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="memory">要发送的数据窗口。</param>
        /// <returns>发送结果（包含已发送字节数和可能的底层错误）。</returns>
        /// <remarks>这是对 <see cref="ILinker.Send(Memory{byte})"/> 的便捷包装。</remarks>
        public static Result<SocketOperationValue> Send(this ILinker linker, in ReadOnlyMemory<byte> memory)
        {
            return linker.Send(MemoryMarshal.AsMemory(memory));
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
        public static Result<SocketOperationValue> Send(this ILinker linker, ReadOnlySequence<byte> sequence)
        {
            int total = 0;
            var value = SocketOperationValue.Empty;

            SequencePosition position = sequence.Start;
            while (sequence.TryGet(ref position, out var segment))
            {
                var remaining = segment;
                while (remaining.Length > 0)
                {
                    var result = linker.Send(remaining);
                    if (!result)
                        return result;
                    value = result.Value;
                    if (value.BytesTransferred <= 0)
                        return Result.FromException<SocketOperationValue>(result.Exception!);
                    total += value.BytesTransferred;
                    if (value.BytesTransferred < remaining.Length)
                        remaining = remaining.Slice(value.BytesTransferred);
                    else
                        break;
                }
            }
            return Result.Success(new SocketOperationValue(total, value.RemoteEndPoint, value.ReceiveMessageFromPacketInfo));
        }

        /// <summary>
        /// 异步发送一段只读内存数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="memory">要发送的数据窗口。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果任务（包含已发送字节数和可能的底层错误）。</returns>
        /// <remarks>这是对 <see cref="ILinker.SendAsync(Memory{byte}, CancellationToken)"/> 的便捷包装。</remarks>
        public static ValueTask<Result<SocketOperationValue>> SendAsync(this ILinker linker, in ReadOnlyMemory<byte> memory, CancellationToken token = default)
        {
            return linker.SendAsync(MemoryMarshal.AsMemory(memory), token);
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
        public static async ValueTask<Result<SocketOperationValue>> SendAsync(this ILinker linker, ReadOnlySequence<byte> sequence, CancellationToken token = default)
        {
            int total = 0;
            var value = SocketOperationValue.Empty;

            SequencePosition position = sequence.Start;
            while (sequence.TryGet(ref position, out var segment))
            {
                var remaining = segment;
                while (remaining.Length > 0)
                {
                    var result = await linker.SendAsync(remaining, token).ConfigureAwait(false);

                    if (!result)
                        return result;

                    value = result.Value;
                    if (value.BytesTransferred <= 0)
                        return Result.FromException<SocketOperationValue>(result.Exception!);

                    total += value.BytesTransferred;

                    if (value.BytesTransferred < remaining.Length)
                        remaining = remaining.Slice(value.BytesTransferred);
                    else
                        break;
                }
            }

            return Result.Success(new SocketOperationValue(total, value.RemoteEndPoint, value.ReceiveMessageFromPacketInfo));
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
        /// - 内部使用 <see cref="MemoryMarshal.AsMemory{T}(ReadOnlyMemory{T})"/> 转换为可写视图后调用底层。 <br/>
        /// - 对大数据或频繁调用场景，可考虑预分配缓冲以减少复制。
        /// </remarks>
        public static ValueTask<Result<SocketOperationValue>> SendToAsync<TLinker>(this TLinker linker, in ReadOnlyMemory<byte> memory, EndPoint endPoint, CancellationToken token = default)
            where TLinker : IUdpLinker
        {
            return linker.SendToAsync(MemoryMarshal.AsMemory(memory), endPoint, token);
        }

        #endregion Send

        #region Receive


        #endregion Receive
    }
}