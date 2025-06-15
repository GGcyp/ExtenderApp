using ExtenderApp.Data;
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

        public TestMainViewModel(IBinaryParser parser, ISplitterParser splitterParser, IServiceStore serviceStore) : base(serviceStore)
        {
            //BinaryParserTest(parser);
            SplitterParserTest(splitterParser, parser);
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

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(_serviceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}
