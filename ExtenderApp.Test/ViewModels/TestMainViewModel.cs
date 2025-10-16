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
        public class TestClass
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public TestMainViewModel(IServiceStore serviceStore, IBinaryParser binaryParser) : base(serviceStore)
        {
            var info = CreatTestExpectLocalFileInfo("text");
            binaryParser.Serialize(new TestClass { Name = "Test", Age = 18 }, out ByteBlock block);
            var obj = binaryParser.Deserialize<TestClass>(ref block);
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ServiceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}