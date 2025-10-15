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
            //binaryParser.Serialize("5646464", out ByteBuffer buffer);
            //Info(buffer.Remaining);
            //var data = binaryParser.Deserialize<string>(ref buffer);
            binaryParser.Serialize(new int[1000], out ByteBlock block, CompressionType.Lz4Block);
            binaryParser.Serialize(new int[1000], out ByteBlock bblock, CompressionType.Lz4BlockArray);
            var data = binaryParser.Deserialize<int[]>(ref bblock);
            data = binaryParser.Deserialize<int[]>(ref block);
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ServiceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}