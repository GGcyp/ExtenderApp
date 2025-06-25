using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    public class LinkerClientFactory
    {
        private readonly ILinkerFactory _linkerFactory;
        private readonly IServiceProvider _serviceProvider;

        public LinkerClientFactory(ILinkerFactory linkerFactory, IServiceProvider provider)
        {
            _linkerFactory = linkerFactory;
            _serviceProvider = provider;
        }

        public LinkClient<TLinker, LinkParser> Create<TLinker>()
            where TLinker : ILinker
        {
            return Create<TLinker, LinkParser>();
        }

        public LinkClient<TLinker, TLinkParser> Create<TLinker, TLinkParser>()
            where TLinker : ILinker
            where TLinkParser : LinkParser
        {
            var linker = _linkerFactory.CreateLinker<TLinker>();
            var parser = _serviceProvider.GetRequiredService<TLinkParser>();
            var result = new LinkClient<TLinker, TLinkParser>(linker, parser);
            return result;
        }
    }
}
