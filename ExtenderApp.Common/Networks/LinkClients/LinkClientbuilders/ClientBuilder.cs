using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    public class ClientBuilder<TILinker> where TILinker : ILinker
    {
        private readonly IServiceProvider _provider;
        public ILinkerFactory<TILinker>? LinkerFactory { get; set; }
        public IClientPluginManager? PluginManager { get; set; }
        public IClientFormatterManager? FormatterManager { get; set; }

        public ClientBuilder(IServiceProvider provider, ILinkerFactory<TILinker> factory) : this(provider)
        {
            LinkerFactory = factory;
        }

        public ClientBuilder(IServiceProvider provider)
        {
            _provider = provider;
        }

        public ClientBuilder<TILinker> SetFormatterManager(Action<FormatterManagerBuilder> action)
        {
            ClientFormatterManager manager = new();
            FormatterManager = manager;
            action?.Invoke(new FormatterManagerBuilder(_provider, FormatterManager));
            return this;
        }

        public IClient Build()
        {
            return Build(AddressFamily.InterNetwork);
        }

        public IClient Build(AddressFamily addressFamily)
        {
            if (LinkerFactory is null)
                throw new InvalidOperationException("LinkerFactory 未设置，无法创建 Linker 实例。");

            return Build(LinkerFactory.CreateLinker(addressFamily));
        }

        public IClient Build(Socket socket)
        {
            if (LinkerFactory is null)
                throw new InvalidOperationException("LinkerFactory 未设置，无法创建 Linker 实例。");

            return Build(LinkerFactory.CreateLinker(socket));
        }

        public IClient Build(TILinker linker)
        {
            ArgumentNullException.ThrowIfNull(linker, nameof(linker));

            var client = new LinkClient<TILinker>(linker);

            if (FormatterManager is not null)
                client.SetClientFormatterManager(FormatterManager);

            return client;
        }
    }
}