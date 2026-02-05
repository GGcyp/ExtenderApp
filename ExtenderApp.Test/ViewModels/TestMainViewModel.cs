using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;
using ExtenderApp.ViewModels;

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
            //binarySerialization.Serialize(1111111111L, out ByteBuffer buffer);
            //var value3 = binarySerialization.Deserialize<long>(buffer);
            //LogDebug("value3:" + value3);
            //buffer.Dispose();
            Guid guid1 = new("sss");
            Guid guid2 = new("sss");
            LogDebug(guid1 == guid2);
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ProgramDirectory.ChekAndCreateFolder("test"), fileName);
        }
    }
}