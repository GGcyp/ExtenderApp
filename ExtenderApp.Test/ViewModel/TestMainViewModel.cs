﻿using ExtenderApp.Data;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using ExtenderApp.Common;
using System.Net;
using ExtenderApp.Common.Networks;
using System.Diagnostics;
using System.Buffers;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        private readonly ILinkerFactory _linkerFactory;

        public TestMainViewModel(TcpLinker tcpLinker, ILinkerFactory linkerFactory, IServiceStore serviceStore) : base(serviceStore)
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
            //_binaryParser = binaryParser;
            //_sequencePool = sequencePool;
            _linkerFactory = linkerFactory;
            Task.Run(Listener);
            tcpLinker.Start();
            tcpLinker.Connect("127.0.0.1", 5520);
            //byte[] b = new byte[Utility.KilobytesToBytes(12)];
            //for (int i = 0; i < 10; i++)
            //{
            //    operate.Send(b);
            //    //operate.Send("b");
            //}

            //tcpLinker.Set(new LinkerDto() { NeedHeartbeat = true });

            //_sb = new StringBuilder();
            //tcpLinker.Recorder.OnFlowRecorder += o =>
            //{
            //    //_sb.AppendLine();
            //    //_sb.Append("每秒发送");
            //    //_sb.Append(Utility.BytesToMegabytes(o.SendBytesPerSecond).ToString());
            //    //_sb.AppendLine();
            //    //_sb.Append("总发送");
            //    //_sb.Append(Utility.BytesToMegabytes(o.SendByteCount).ToString());
            //    //_sb.AppendLine();
            //    //_sb.Append(Utility.BytesToMegabytes(o.ReceiveByteCount).ToString());
            //    //_sb.AppendLine();
            //    //_sb.Append(Utility.BytesToMegabytes(o.ReceiveBytesPerSecond).ToString());
            //    //_sb.AppendLine();

            //    //Debug(_sb.ToString());
            //    //_sb.Clear();
            //};
            //Task.Run(() => { TestSend(tcpLinker); });

            //operate.Heartbeat.ChangeSendHearbeatInterval(20);
            //operate.Close();

            //Delegate @delegate = () => { Debug("最开始"); };
            //var action = (@delegate as Action);
            //action += () => { Debug("第二个"); };
            //(@delegate as Action).Invoke();

            //Debug("重新");
            //action.Invoke();
        }

        private async void TestSend(TcpLinker tcpLinker)
        {
            await Task.Delay(1000);
            //tcpLinker.Send("s");
            //tcpLinker.Send("发送完成，关闭链接");

            //tcpLinker.Set(new LinkerDto() { NeedHeartbeat = true });
            //tcpLinker.Heartbeat.ReceiveHeartbeatEvent += Heartbeat_ReceiveHeartbeatEvent;
            //tcpLinker.Heartbeat.SendHeartbeat();
            //tcpLinker.Heartbeat.ChangeSendHearbeatInterval(1000);

            //byte[] bytes = new byte[Utility.MegabytesToBytes(10)];

            //Task.Run(async () =>
            //{
            //    for (int i = 0; i < 1000; i++)
            //    {
            //        for (int j = 0; j < 100000; j++)
            //        {
            //            tcpLinker.Send("ssssssssssssssssssssssssssss");
            //        }
            //        //await Task.Delay(10000);
            //    }
            //});
            var send = new byte[Utility.MegabytesToBytes(1)];
            //StringBuilder Builder = new StringBuilder();
            //tcpLinker.Recorder.OnFlowRecorder += o =>
            //{
            //    Builder.AppendLine();
            //    Builder.Append("每秒发送");
            //    Builder.Append(Utility.BytesToMegabytes(o.SendBytesPerSecond).ToString());
            //    Builder.AppendLine();
            //    Builder.Append("总发送");
            //    Builder.Append(Utility.BytesToMegabytes(o.SendByteCount).ToString());
            //    Builder.AppendLine();
            //    Debug(Builder.ToString());
            //    Builder.Clear();
            //};
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < 1000; i++)
            {
                tcpLinker.Send(send);
            }
            stopwatch.Stop();
            Debug("使用了：" + stopwatch.ElapsedMilliseconds.ToString());
        }

        private async void Listener()
        {
            //TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5520));
            //listener.Start();
            //var client = listener.AcceptTcpClient();

            TcpListenerLinker tcpListener = new TcpListenerLinker(_linkerFactory);
            tcpListener.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5520));
            tcpListener.Listen();
            TcpLinker link = (TcpLinker)tcpListener.Accept();
            link.Start();
            //var stream = client.GetStream();
            //byte[] bytes = new byte[1024];
            //await stream.ReadAsync(bytes);
            //var temp = _binaryParser.Deserialize<NetworkPacket>(bytes);
            //var name = _binaryParser.Deserialize<string>(temp.Bytes);
            //Debug(temp.TypeCode.ToString() + name);
            //link.Register<byte[]>(Networ);
            link.Register<string>(Networ);
            link.Recorder.OnFlowRecorder += o =>
            {
                //Builder.AppendLine();
                //Builder.Append("每秒接受");
                //Builder.Append(Utility.BytesToMegabytes(o.ReceiveBytesPerSecond).ToString());
                //Builder.AppendLine();
                //Builder.Append("总接受");
                //Builder.Append(Utility.BytesToMegabytes(o.ReceiveByteCount).ToString());
                //Builder.AppendLine();

                //Debug(Builder.ToString());
                //Builder.Clear();
            };
            link.OnReceive += Networ;
            //link.Register<LinkerDto>(i => Debug("接受"));
            //link.Heartbeat.ChangeSendHearbeatInterval(0);
            //link.Heartbeat.ReceiveHeartbeatEvent += Heartbeat_ReceiveHeartbeatEvent;
        }

        private void Heartbeat_ReceiveHeartbeatEvent(Common.Networks.HearbeatResult obj)
        {
            Debug(obj.HeartbeatType.ToString());
        }

        private void Networ(byte[] bytes)
        {
            //Debug(s);
            //Debug("收到" + bytes.Length.ToString());
        }

        private void Networ(string s)
        {
            //Debug(s);
        }

        private void Networ(ReadOnlyMemory<byte> bytes)
        {
            Debug("接受");
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(_serviceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}
