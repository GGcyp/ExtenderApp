using ExtenderApp.Data;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using System.Net;
using System.Text;
using ExtenderApp.Common.Networks;
using AppHost.Extensions.DependencyInjection;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        private readonly IBinaryFormatter<string> _string;
        private readonly LinkClientBuilder<ITcpLinkClient> _builder;

        public TestMainViewModel(IServiceStore serviceStore, ITcpLinker tcpLinker, ITcpListenerLinker tcpListenerLinker, IBinaryFormatter<string> formatter, IServiceProvider provider) : base(serviceStore)
        {
            var info = CreatTestExpectLocalFileInfo("text");
            _string = formatter;

            IPEndPoint loop = new IPEndPoint(IPAddress.Loopback, 9090);
            tcpListenerLinker.Bind(loop);
            tcpListenerLinker.OnAccept += TcpListenerLinker_OnAccept;
            tcpListenerLinker.Listen(10);

            _builder = new(provider, provider.GetRequiredService<ILinkClientFactory<ITcpLinkClient>>());
            _builder.SetFormatterManager(b =>
            {
                //b.AddBinaryFormatter<string>(s => Info(s));
                b.AddBinaryFormatter<int[]>(a => Info(a.Length));
            });
            ITcpLinkClient client = _builder.Build(tcpLinker);
            client.Connect(loop);
            Thread.Sleep(1000);
            int[] ints = new int[1000000];
            for (int i = 0; i < 5; i++)
            {
                //client.SendAsync("Hello World!");
                client.SendAsync(ints);
            }
        }

        private void TcpListenerLinker_OnAccept(object? sender, ITcpLinker e)
        {
            var client = _builder.Build(e);
        }

        private void Formatter(byte[] bytes)
        {
            ByteBuffer buffer = new(bytes);
            string s = _string.Deserialize(ref buffer);
            Info(s);
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ServiceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}