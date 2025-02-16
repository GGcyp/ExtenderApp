using ExtenderApp.Data;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using ExtenderApp.Common;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        public TestMainViewModel(ISplitterParser splitter, IBinaryParser binary, IServiceStore serviceStore) : base(serviceStore)
        {
            //var fileInfo = CreatTestExpectLocalFileInfo(string.Format("测试{0}", DateTime.Now.ToString()));
            //var info = new FileSplitterInfo(2048, 2, 0, 1024, FileExtensions.TextFileExtensions);
            //splitter.Creat(fileInfo, info);
            //var s = binary.GetCount(Guid.NewGuid());
            //byte[] bytes = binary.Serialize("ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss");
            //var info = CreatTestExpectLocalFileInfo("test");
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
        }


        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(_serviceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}
