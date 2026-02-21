using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Contracts;
using ExtenderApp.Test.Tests;
using ExtenderApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        private readonly ILZ4Compression lZ4Compression;
        private readonly IBinarySerialization binarySerialization;

        public TestMainViewModel(ILZ4Compression lZ4Compression, IBinarySerialization binarySerialization)
        {
            this.lZ4Compression = lZ4Compression;
            this.binarySerialization = binarySerialization;
        }

        public override void Inject(IServiceProvider serviceProvider)
        {
            base.Inject(serviceProvider);
            // 运行自包含的测试用例，检查序列化及内存回收/冻结相关行为
            //SerializationTests.RunAll(binarySerialization);
            //var factory = serviceProvider.GetRequiredService<ILinkerFactory<ITcpLinker>>();
            //Task.Run(() =>
            //{
            //    LinkerTests.RunAll(factory, binarySerialization);
            //});
            AwaitableEventArgsTests.RunAll();
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ProgramDirectory.ChekAndCreateFolder("test"), fileName);
        }
    }
}