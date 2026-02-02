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
            //Random rnd = new();
            //byte[] bytes = new byte[1024];
            ////rnd.NextBytes(bytes);
            //TestCompression(bytes);
            //LogDebug("第一次");
            //rnd.NextBytes(bytes);
            //TestCompression(bytes);
            //LogDebug("第二次");
            binarySerialization.Serialize(123, out ByteBuffer buffer);
            var value = binarySerialization.Deserialize<int>(buffer);
            buffer.Dispose();
            binarySerialization.Serialize("Hello, World!", out buffer);
            var str = binarySerialization.Deserialize<string>(buffer);
            buffer.Dispose();
            binarySerialization.Serialize(new byte[1024], out buffer);
            var data = binarySerialization.Deserialize<byte[]>(buffer);
        }

        private void TestCompression(byte[] bytes)
        {
            //lZ4Compression.TryCompress(bytes.AsMemory(), out ByteBlock block);
            //LogDebug("压缩后长度: " + block.Length);
            //lZ4Compression.TryDecompress(block.UnreadMemory, out block);
            //LogDebug($"与元数据对比结果{bytes.AsSpan().SequenceEqual(block)}");

            //lZ4Compression.TryCompress(new ByteBuffer(bytes), out var buffer);
            //LogDebug("压缩后长度: " + buffer.Length);
            //lZ4Compression.TryDecompress(buffer, out buffer);
            //LogDebug($"与元数据对比结果{bytes.AsSpan().SequenceEqual(buffer.UnreadSequence.First.Span)}");
            binarySerialization.Serialize(bytes, lZ4Compression, out var buffer);
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ProgramDirectory.ChekAndCreateFolder("test"), fileName);
        }
    }
}