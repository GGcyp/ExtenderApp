using ExtenderApp.Data;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using ExtenderApp.Common;
using System.Security.Cryptography;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        private readonly ILinkerFactory _linkerFactory;

        public TestMainViewModel(IHashProvider hashProvider, IServiceStore serviceStore) : base(serviceStore)
        {
            //BinaryParserTest(parser);
            //SplitterParserTest(splitterParser, parser);
            HashTest(hashProvider);
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

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(_serviceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}
