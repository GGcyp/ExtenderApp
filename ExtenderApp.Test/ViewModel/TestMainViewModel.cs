using ExtenderApp.Data;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        public TestMainViewModel(ISplitterParser splitter, IBinaryParser binary, IServiceStore serviceStore) : base(serviceStore)
        {
            //var fileInfo = CreatTestExpectLocalFileInfo(string.Format("测试{0}", DateTime.Now.ToString()));
            //var info = new FileSplitterInfo(2048, 2, 0, 1024, FileExtensions.TextFileExtensions);
            //splitter.Creat(fileInfo, info);
            var s = binary.GetCount(Guid.NewGuid());
        }


        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(_serviceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}
