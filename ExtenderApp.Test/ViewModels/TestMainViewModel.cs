using ExtenderApp.Data;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using ExtenderApp.Common;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using ExtenderApp.Common.Networks;
using System.Buffers.Binary;
using System.Text;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        public TestMainViewModel(IServiceStore serviceStore, IBinaryParser binaryParser) : base(serviceStore)
        {
            var info = CreatTestExpectLocalFileInfo("text");
            string test = "sad阿斯顿";
            var tempb = Encoding.UTF8.GetBytes(test);
            var objs = Encoding.UTF8.GetString(tempb);
            var bytes = binaryParser.Serialize(test);
            bool eq = bytes.AsSpan(1).SequenceEqual(tempb);
            var obj = binaryParser.Deserialize<string>(bytes);
            binaryParser.Write(info, test);
            var tempS = binaryParser.Read<string>(info);
            binaryParser.Write(info, 123132);
            var tempI = binaryParser.Read<int>(info);
            binaryParser.Write(info, 123132L);
            var tempL = binaryParser.Read<long>(info);
            binaryParser.Write(info, 123.132f);
            var tempF = binaryParser.Read<float>(info);
            binaryParser.Write(info, DateTime.Now);
            var tempDT = binaryParser.Read<DateTime>(info);
        }
        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ServiceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}
