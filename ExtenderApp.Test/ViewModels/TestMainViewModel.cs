using ExtenderApp.Data;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using System.Net;
using System.Text;
using ExtenderApp.Common.Networks;
using ExtenderApp.Common.Pipelines;
using ExtenderApp.Common.Networks.Middlewares;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        private readonly IBinaryFormatter<string> _string;
        private readonly IBinaryFormatter<LinkHeader> _linkHeaderFormatter;

        public TestMainViewModel(IServiceStore serviceStore, ITcpLinker tcpLinker, ITcpListenerLinker tcpListenerLinker, IBinaryFormatter<string> formatter,IBinaryFormatter<LinkHeader> formatter1, IByteBufferFactory factory) : base(serviceStore)
        {
            var info = CreatTestExpectLocalFileInfo("text");
            _string = formatter;
            _linkHeaderFormatter = formatter1;

            IPEndPoint loop = new IPEndPoint(IPAddress.Loopback, 9090);
            tcpListenerLinker.Bind(loop);
            tcpListenerLinker.OnAccept += TcpListenerLinker_OnAccept;
            tcpListenerLinker.Listen(10);

            tcpLinker.ConnectAsync(loop);
            LinkClient<ITcpLinker> client = new(tcpLinker);
            PipelineBuilder<LinkerClientContext, LinkerClientContext> builder = new();
            FormatterMiddleware<string> middleware = new(formatter, factory);
            builder.Use(middleware);
            client.SetClientPipeline(builder);
            client.SendAsync("saadasdad").GetAwaiter().GetResult();
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
            LinkHeader header = _linkHeaderFormatter.Deserialize(ref buffer);
            string s = _string.Deserialize(ref buffer);
            Info(s);
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ServiceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}