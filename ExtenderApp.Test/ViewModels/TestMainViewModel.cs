using ExtenderApp.Data;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using System.Net;
using System.Text;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        public TestMainViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            var info = CreatTestExpectLocalFileInfo("text");
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ServiceStore.PathService.CreateFolderPathForAppRootFolder("test"), fileName);
        }
    }
}