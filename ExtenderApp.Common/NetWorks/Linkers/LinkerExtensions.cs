using System.Runtime.InteropServices;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 提供与链接（ILinker）及其依赖注册相关的扩展方法。
    /// </summary>
    internal static class LinkerExtensions
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
            return services;
        }

        /// <summary>
        /// 以约定的生命周期将链接器与其工厂注册到 DI 容器。
        /// </summary>
        /// <typeparam name="TILinker">
        /// 对外暴露的链接接口类型，必须实现 <see cref="ILinker"/>。
        /// </typeparam>
        /// <typeparam name="TLinker">
        /// 链接器具体实现类型，需实现 <typeparamref name="TILinker"/>。
        /// </typeparam>
        /// <typeparam name="TLinkerFactory">
        /// 链接器工厂类型，需实现 <see cref="ILinkerFactory{T}"/>。
        /// </typeparam>
        /// <typeparam name="TListenerLinkerFactory">
        /// 监听器链接器工厂类型，需实现 <see cref="IListenerLinkerFactory{T}"/>。
        /// </typeparam>
        /// <param name="services">服务集合。</param>
        /// <returns>原服务集合（便于链式调用）。</returns>
        /// <remarks>
        /// 生命周期约定：
        /// - 工厂： <c>Singleton</c>； <br/>
        /// - ILinker 实例：
        ///   <c>Transient</c>（每次解析创建新实例）； <br/>
        /// - IListenerLinker 实例： <c>Transient</c>（每次解析创建新实例）。
        /// </remarks>
        public static IServiceCollection AddILinker<TILinker, TLinker, TLinkerFactory>(this IServiceCollection services)
            where TILinker : ILinker
            where TLinker : TILinker
            where TLinkerFactory : class, ILinkerFactory<TILinker>
        {
            services.AddSingleton<ILinkerFactory<TILinker>, TLinkerFactory>();
            services.AddSingleton<IListenerLinkerFactory<TILinker>, ListenerLinkerFactory<TILinker>>();

            // 每次解析 TILinker 时通过工厂创建
            services.AddTransient<TILinker>(provider =>
            {
                return provider.GetRequiredService<ILinkerFactory<TILinker>>().CreateLinker();
            });

            // 每次解析 IListenerLinker<TILinker> 时通过工厂创建
            services.AddTransient<IListenerLinker<TILinker>>(provider =>
            {
                return provider.GetRequiredService<IListenerLinkerFactory<TILinker>>().CreateListenerLinker();
            });
            return services;
        }

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
        public static ValueTask<SocketOperationResult> SendAsync(this ILinker linker, ByteBlock block, CancellationToken token)
        {
            return linker.SendAsync(MemoryMarshal.AsMemory(block.UnreadMemory), token);
        }

        /// <summary>
        /// 异步发送 <see cref="ByteBuffer"/> 中尚未读取的数据。
        /// </summary>
        /// <param name="linker">目标链接。</param>
        /// <param name="buffer">
        /// 字节缓冲（方法内部会创建临时 <see cref="ByteBlock"/>，发送后释放）。
        /// </param>
        /// <param name="token">取消令牌。</param>
        /// <returns>发送结果任务。</returns>
        public static ValueTask<SocketOperationResult> SendAsync(this ILinker linker, ByteBuffer buffer, CancellationToken token)
        {
            ByteBlock block = new(buffer);
            var task = Task.Run(async () =>
            {
                var result = await linker.SendAsync(block, token).ConfigureAwait(false);
                block.Dispose();
                return result;
            });
            return new ValueTask<SocketOperationResult>(task);
        }
    }
}