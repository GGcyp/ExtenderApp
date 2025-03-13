using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class FileSegmenter
    {
        public const int DefaultChunkSize = 1024 * 1024;

        private readonly ISplitterParser _splitterParser;
        private readonly ILinker _linker;

        private ExpectLocalFileInfo crruentFileInfo;
        private IConcurrentOperate fileOperate;

        public FileSegmenter(ILinker linker, ISplitterParser splitterParser)
        {
            _linker = linker;
            _splitterParser = splitterParser;
        }

        public void Start()
        {
            _linker.Register<SplitterDto>(ReceiveSplitterDto);
        }

        private void ReceiveSplitterDto(SplitterDto splitterDto)
        {
            _splitterParser.Write(crruentFileInfo, splitterDto.Bytes, splitterDto.ChunkIndex, fileOperate: fileOperate);
            splitterDto.Dispose();
        }

        public void SendFile(LocalFileInfo info, int chukSize = DefaultChunkSize)
        {
            ////ThrowNotConnected();

            ////检查大小，小的文件直接传输，大的文件需要分块传输
            var splitterInfo = _splitterParser.Create(info, chukSize, false);

            if (splitterInfo.Length <= DefaultChunkSize)
            {
                //小文件直接传输
                var splitterDto = _splitterParser.GetSplitterDto(info, 0, splitterInfo);
                _linker.Send(splitterDto);
                splitterDto.Dispose();
                return;
            }

            //大文件分段传输
            IConcurrentOperate? fileOperate = _splitterParser.GetOperate(info.CreateWriteOperate());
            for (uint i = 0; i < splitterInfo.ChunkCount; i++)
            {
                var splitterDto = _splitterParser.GetSplitterDto(info, i, splitterInfo, fileOperate);
                _linker.Send(splitterDto);
                splitterDto.Dispose();
            }
        }
    }
}
