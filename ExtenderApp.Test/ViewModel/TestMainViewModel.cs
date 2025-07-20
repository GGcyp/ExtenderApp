using ExtenderApp.Data;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using ExtenderApp.Common;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using ExtenderApp.Common.Networks;
using System.Buffers.Binary;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        private readonly LinkerClientFactory _linkerFactory;

        public TestMainViewModel(ResourceLimiter limiter, ResourceLimiter limiter1, ITcpLinker linker, IListenerLinker<ITcpLinker> listenerLinker, IServiceStore serviceStore) : base(serviceStore)
        {
            //BinaryParserTest(parser);
            //SplitterParserTest(splitterParser, parser);
            //HashTest(hashProvider);
            //BinaryTest(sequencePool);
            //_linkerFactory = linkerClientFactory;
            //TcpLinkTest();
            limiter.OnStatsUpdated += r => Debug(r.ToString());
            listenerLinker.InitInterNetwork();
            listenerLinker.Bind(IPAddress.Loopback, 12345);
            listenerLinker.Listen(10);
            listenerLinker.BeginAccept(Linker_OnConnect);
            linker.Connect(IPAddress.Loopback, 12345);
            //linker.OnReceive += (s, i) => Debug($"{i}");
        }

        private void BinaryParserTest(IBinaryParser parser)
        {
            // 测试二进制解析器
            var fileInfo = CreatTestExpectLocalFileInfo("binaryTest");
            var result = parser.Read<byte[]>(fileInfo);
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            parser.Write(fileInfo, data);
            //Debug.WriteLine($"Parsed data: {BitConverter.ToString(result)}");
            result = parser.Read<byte[]>(fileInfo);

            data = new byte[] { 0x05, 0x06, 0x07, 0x08, 0x09 };
            parser.WriteAsync(fileInfo, data);
            parser.ReadAsync<byte[]>(fileInfo, b => Info(b[0].ToString()));
        }

        private void SplitterParserTest(ISplitterParser splitterParser, IBinaryParser binaryParser)
        {
            // 测试分割器解析器
            var tagretFileInfo = CreatTestExpectLocalFileInfo("binaryTest").CreatLocalFileInfo(FileExtensions.BinaryFileExtensions);
            var splitterInfo = splitterParser.CreateInfoForFile(tagretFileInfo, 10);

            var temp = splitterParser.Read(tagretFileInfo, 0, splitterInfo);
            var result = binaryParser.Deserialize<byte[]>(temp);
            //Debug.WriteLine($"Parsed Splitter Info: {result.FileName}, Size: {result.FileSize}, Chunks: {result.TotalChunks}");
        }

        private void HashTest(IHashProvider hashProvider)
        {
            // 测试哈希提供者
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var hash = hashProvider.ComputeHash<MD5>(data);
            Info(hash);
            var fileInfo = CreatTestExpectLocalFileInfo("binaryTest");
            hash = hashProvider.ComputeHash<MD5>(fileInfo.CreateReadWriteOperate(FileExtensions.BinaryFileExtensions));
            Info(hash);
            //Debug.WriteLine($"Computed Hash: {BitConverter.ToString(hash)}");
        }

        private void TcpLinkTest()
        {
            Task.Run(TcpListener);

            var client = _linkerFactory.Create<ITcpLinker, LinkParser>();
            //client.SetLinkParser(new ExtenderLinkParser(_binaryParser));
            ExtenderBinaryWriter writer = new ExtenderBinaryWriter(new SequencePool<byte>(), new byte[8]);
            var span = writer.GetSpan(8);
            BinaryPrimitives.WriteInt32BigEndian(span, 12345678);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(4), 556665554);
            writer.Advance(8);
            writer.Commit();

            client.OnConnectClient += (c) =>
            {
                Info("链接成功，开始发送数据");
                c.Send(new byte[] { 0x01, 0x02, 0x03, 0x04 });
                c.Send(new LinkerDto());
            };
            client.ConnectAsync(IPAddress.Loopback, 12345);
            client.SendAsync(new byte[] { 0x01, 0x02, 0x03, 0x04 });
            client.SendAsync(writer);
        }

        private void Linker_OnConnect(ILinker obj)
        {
            Info("链接成功，开始发送数据");
            byte[] bytes = new byte[Utility.MegabytesToBytes(1)];
            for (int i = 0; i < 1000; i++)
            {
                obj.SendAsync(bytes);
            }
        }

        private void BinaryTest(SequencePool<byte> sequencePool)
        {
            byte[] bytes = new byte[10];
            ExtenderBinaryWriter write = new ExtenderBinaryWriter(sequencePool, bytes);
            var span = write.GetSpan(4);
            BinaryPrimitives.WriteInt32BigEndian(span, 11223344);
            write.Advance(4);
            ExtenderBinaryReader reader = new ExtenderBinaryReader(bytes);
            int value = BinaryPrimitives.ReadInt32BigEndian(reader.UnreadSpan);
            Info(value);
        }

        private async void TcpListener()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 12345);
            listener.Start();
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                Info("收到请求");
                var stream = client.GetStream();
                var buffer = new byte[1024];
                var callback = new AsyncCallback(i =>
                {
                    try
                    {
                        var bytesRead = stream.EndRead(i);
                        if (bytesRead > 0)
                        {
                            var readBuffer = new byte[bytesRead];
                            var item = i.AsyncState as Tuple<byte[], NetworkStream, AsyncCallback>;

                            Array.Copy(item.Item1, readBuffer, bytesRead);
                            Info($"接收到数据: {BitConverter.ToString(readBuffer)}");
                            item.Item2.BeginRead(item.Item1, 0, 1024, item.Item3, item);
                        }
                    }
                    catch (Exception ex)
                    {
                        Info($"读取数据时发生错误: {ex.Message}");
                    }
                });

                stream.BeginRead(buffer, 0, 1024, callback, Tuple.Create(buffer, stream, callback));
            }

        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(_serviceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}
