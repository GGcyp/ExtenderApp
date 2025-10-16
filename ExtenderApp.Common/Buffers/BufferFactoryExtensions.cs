using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Common.Error;
using AppHost.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Buffers
{
    /// <summary>
    /// 为 <see cref="IByteBufferFactory"/> 提供依赖注入注册与便捷创建方法的扩展。
    /// </summary>
    public static class BufferFactoryExtensions
    {
        /// <summary>
        /// 注册 <see cref="IByteBufferFactory"/> 的默认实现 <c>ByteBufferFactory</c> 到容器中（单例）。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>传入的 <see cref="IServiceCollection"/>，用于链式调用。</returns>
        /// <remarks>
        /// - 以单例方式注册工厂，便于在不同组件中复用缓冲池策略。<br/>
        /// - 如需自定义实现，可在调用本方法前或后替换绑定。
        /// </remarks>
        public static IServiceCollection AddBufferFactory(this IServiceCollection services)
        {
            services.AddSingleton<IByteBufferFactory, ByteBufferFactory>();
            return services;
        }

        /// <summary>
        /// 使用工厂创建一个 <see cref="ByteBuffer"/>，并将给定的字节数据写入其中。
        /// </summary>
        /// <param name="factory">缓冲工厂实例。</param>
        /// <param name="span">要写入的只读字节跨度。</param>
        /// <returns>写入完成的 <see cref="ByteBuffer"/> 实例。</returns>
        /// <remarks>
        /// - 返回的 <see cref="ByteBuffer"/> 为 ref struct，非线程安全，使用完毕如为池化可写缓冲应调用 <c>Dispose()</c> 归还。<br/>
        /// - <paramref name="span"/> 的内容会被追加到新建缓冲中，缓冲的长度将等于 <c>span.DefaultLength</c>。
        /// </remarks>
        public static ByteBuffer Create(this IByteBufferFactory factory, in ReadOnlySpan<byte> span)
        {
            factory.ArgumentNull(nameof(factory));
            var buffer = factory.Create();
            buffer.Write(span);
            return buffer;
        }
    }
}
