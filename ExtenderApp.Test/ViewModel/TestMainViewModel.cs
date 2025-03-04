using ExtenderApp.Data;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using ExtenderApp.Common;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using ExtenderApp.Data.File;
using ExtenderApp.Common.NetWorks;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        private readonly IBinaryParser _binaryParser;
        private readonly SequencePool<byte> _sequencePool;

        public TestMainViewModel(TcpLinker operate, IBinaryParser binaryParser, SequencePool<byte> sequencePool, IServiceStore serviceStore) : base(serviceStore)
        {
            //var fileInfo = CreatTestExpectLocalFileInfo(string.Format("测试{0}", DateTime.Now.ToString()));
            //var info = new FileSplitterInfo(2048, 2, 0, 1024, FileExtensions.TextFileExtensions);
            //splitter.Creat(fileInfo, info);
            //var s = binary.GetCount(Guid.NewGuid());
            //byte[] bytes = binary.Serialize("ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss");
            //var totalMegabytes = Utility.MegabytesToBytes(20);
            //var chunkSize = Utility.MegabytesToBytes(1);

            //uint temp = (uint)(totalMegabytes % chunkSize > 0 ? 1 : 0);
            //uint count = (uint)(totalMegabytes / chunkSize) + temp;
            //splitter.Creat(info, new SplitterInfo(totalMegabytes, count, 0, (int)chunkSize, FileExtensions.TextFileExtensions));
            //for (uint i = 0; i < count; i++)
            //{
            //    splitter.WriteAsync(info, bytes, i);
            //}
            //var result = splitter.Read<string>(info);
            //for (uint i = 1024 * 512; i < 1024 * 1024; i++)
            //{
            //    splitter.Write(info, bytes, i);
            //}
            //result = splitter.Read<string>(info);
            //var info = CreatTestExpectLocalFileInfo("test");
            //binary.Write(info, 50);
            //var temp = binary.Read<int>(info);
            //binary.Write(info, "sssssss");
            //var temp1 = binary.Read<string>(info);
            //binary.Write(info, string.Empty);
            //temp1 = binary.Read<string>(info);
            //binary.Write(info, new byte[5000]);
            //var temp2 = binary.Read<byte[]>(info);
            _binaryParser = binaryParser;
            _sequencePool = sequencePool;
            Task.Run(Listener);
            operate.Start();
            operate.Connect("127.0.0.1", 5520);
            byte[] b = new byte[Utility.KilobytesToBytes(12)];
            //for (int i = 0; i < 10; i++)
            //{
            //    operate.Send(b);
            //    //operate.Send("b");
            //}
            Send(operate);
            //operate.Heartbeat.ChangeSendHearbeatInterval(20);
            //operate.Close();
        }

        private async void Send(TcpLinker tcpLinker)
        {
            await Task.Delay(1000);
            tcpLinker.Send("s");
            tcpLinker.Send("发送完成，关闭链接");
            tcpLinker.Heartbeat.ReceiveHeartbeatEvent += Heartbeat_ReceiveHeartbeatEvent;
            tcpLinker.Heartbeat.SendHeartbeat();
        }

        private async void Listener()
        {
            //TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5520));
            //listener.Start();
            //var client = listener.AcceptTcpClient();

            TcpListenerLinker tcpListener = new TcpListenerLinker(_binaryParser, _sequencePool);
            tcpListener.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5520));
            tcpListener.Listen();
            TcpLinker link = tcpListener.Accept();
            link.Start();
            Debug("收到");
            //var stream = client.GetStream();
            //byte[] bytes = new byte[1024];
            //await stream.ReadAsync(bytes);
            //var temp = _binaryParser.Deserialize<NetworkPacket>(bytes);
            //var name = _binaryParser.Deserialize<string>(temp.Bytes);
            //Debug(temp.TypeCode.ToString() + name);
            link.Register<byte[]>(Networ);
            link.Register<string>(Networ);
            link.Heartbeat.ChangeSendHearbeatInterval(0);
            link.Heartbeat.ReceiveHeartbeatEvent += Heartbeat_ReceiveHeartbeatEvent;
        }

        private void Heartbeat_ReceiveHeartbeatEvent(Common.NetWorks.HearbeatResult obj)
        {
            Debug(obj.HeartbeatType.ToString());
        }

        private void Networ(byte[] bytes)
        {
            //Debug(s);
            Debug("收到");
        }

        private void Networ(string s)
        {
            Debug(s);
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(_serviceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}
