using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    public class LinkClientBuilder<TILinker> where TILinker : ILinker
    {
        private readonly IServiceProvider _provider;
        public ILinkerFactory<TILinker>? LinkerFactory { get; set; }
        public ILinkClientPluginManager? PluginManager { get; set; }
        public ILinkClientFormatterManager? FormatterManager { get; set; }

        public LinkClientBuilder(IServiceProvider provider, ILinkerFactory<TILinker> factory) : this(provider)
        {
            LinkerFactory = factory;
        }

        public LinkClientBuilder(IServiceProvider provider)
        {
            _provider = provider;
        }

        public LinkClientBuilder<TILinker> SetFormatterManager(Action<FormatterManagerBuilder> action)
        {
            LinkClientFormatterManager manager = new();
            FormatterManager = manager;
            action?.Invoke(new FormatterManagerBuilder(_provider, FormatterManager));
            return this;
        }

        public ILinkClient Build()
        {
            return Build(AddressFamily.InterNetwork);
        }

        public ILinkClient Build(AddressFamily addressFamily)
        {
            if (LinkerFactory is null)
                throw new InvalidOperationException("LinkerFactory 未设置，无法创建 Linker 实例。");

            return Build(LinkerFactory.CreateLinker(addressFamily));
        }

        public ILinkClient Build(Socket socket)
        {
            if (LinkerFactory is null)
                throw new InvalidOperationException("LinkerFactory 未设置，无法创建 Linker 实例。");

            return Build(LinkerFactory.CreateLinker(socket));
        }

        public ILinkClient Build(TILinker linker)
        {
            ArgumentNullException.ThrowIfNull(linker, nameof(linker));

            var client = new LinkClient<TILinker>(linker);

            if (FormatterManager is not null)
                client.SetClientFormatterManager(FormatterManager);

            return client;
        }
    }
}