using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// LinkClientFactory 类用于创建 LinkClient 实例。
    /// </summary>
    public class LinkClientFactory
    {
        /// <summary>
        /// 私有成员 _linkerFactory，用于创建 ILinker 实例。
        /// </summary>
        private readonly ILinkerFactory _linkerFactory;

        /// <summary>
        /// 私有成员 _serviceProvider，用于提供服务。
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 构造函数，初始化 LinkerClientFactory 实例。
        /// </summary>
        /// <param name="linkerFactory">ILinkerFactory 实例，用于创建 ILinker 实例。</param>
        /// <param name="provider">IServiceProvider 实例，用于提供服务。</param>
        public LinkClientFactory(ILinkerFactory linkerFactory, IServiceProvider provider)
        {
            _linkerFactory = linkerFactory;
            _serviceProvider = provider;
        }

        /// <summary>
        /// 创建 LinkClient 实例，使用默认的 LinkParser。
        /// </summary>
        /// <typeparam name="TLinker">ILinker 的实现类型。</typeparam>
        /// <returns>LinkClient<TLinker, LinkParser> 实例。</returns>
        public LinkClient<TLinker, LinkParser> Create<TLinker>()
            where TLinker : ILinker
        {
            return Create<TLinker, LinkParser>();
        }

        /// <summary>
        /// 创建 LinkClient 实例。
        /// </summary>
        /// <typeparam name="TLinker">ILinker 的实现类型。</typeparam>
        /// <typeparam name="TLinkParser">LinkParser 的实现类型。</typeparam>
        /// <returns>LinkClient<TLinker, TLinkParser> 实例。</returns>
        public LinkClient<TLinker, TLinkParser> Create<TLinker, TLinkParser>()
            where TLinker : ILinker
            where TLinkParser : LinkParser
        {
            //var linker = _linkerFactory.CreateLinker<TLinker>();
            //var parser = _serviceProvider.GetRequiredService<TLinkParser>();
            //var result = new LinkClient<TLinker, TLinkParser>(linker, parser);
            //return result;
            return default;
        }

        public LinkClient<TLinker, TLinkParser> Create<TLinker, TLinkParser>(TLinker linker)
            where TLinker : ILinker
            where TLinkParser : LinkParser
        {
            //if (linker == null)
            //{
            //    throw new ArgumentNullException(nameof(linker), "链接不能为空");
            //}
            //var parser = _serviceProvider.GetRequiredService<TLinkParser>();
            //var result = new LinkClient<TLinker, TLinkParser>(linker, parser);
            //return result;
            return default;
        }
    }
}
