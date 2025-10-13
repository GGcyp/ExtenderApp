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
        public TestMainViewModel(IServiceStore serviceStore, IBinaryParser binaryParser) : base(serviceStore)
        {
            var info = CreatTestExpectLocalFileInfo("text");
            binaryParser.Write(info, 123132);
            var tempI = binaryParser.Read<int>(info);
            binaryParser.Write(info, "sssss");
            var tempS = binaryParser.Read<string>(info);
        }
        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ServiceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}
