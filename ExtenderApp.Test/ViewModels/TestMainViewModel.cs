using ExtenderApp.Data;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using System.Net;
using System.Text;
using ExtenderApp.Common.Networks;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        private readonly IBinaryFormatter<string> _string;

        public TestMainViewModel(IServiceStore serviceStore, ITcpLinker tcpLinker, ITcpListenerLinker tcpListenerLinker, IBinaryFormatter<string> formatter, IByteBufferFactory factory, IServiceProvider provider) : base(serviceStore)
        {
            var info = CreatTestExpectLocalFileInfo("text");
            _string = formatter;

            IPEndPoint loop = new IPEndPoint(IPAddress.Loopback, 9090);
            tcpListenerLinker.Bind(loop);
            tcpListenerLinker.OnAccept += TcpListenerLinker_OnAccept;
            tcpListenerLinker.Listen(10);

            tcpLinker.ConnectAsync(loop);
            ClientBuilder<ITcpLinker> builder = new ClientBuilder<ITcpLinker>(provider);
            builder.SetFormatterManager(b =>
            {
                b.AddBinaryFormatter<string>();
            });
            LinkClient<ITcpLinker> client = builder.Build(tcpLinker) as LinkClient<ITcpLinker>;
            client.SendAsync("saadasdad");
        }

        private void TcpListenerLinker_OnAccept(object? sender, ITcpLinker e)
        {
            Task.Run(async () =>
            {
                byte[] bytes = new byte[1024];
                while (true)
                {
                    await e.ReceiveAsync(bytes);
                    Formatter(bytes);
                }
            });
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