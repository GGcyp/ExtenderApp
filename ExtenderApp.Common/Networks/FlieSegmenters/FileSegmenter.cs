using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class FileSegmenter
    {
        public const int DefaultSendLength = 1024 * 1024;

        private readonly ILinkerFactory _linkerFactory;
        private readonly IFileParserStore _fileParserStore;
        private readonly Lazy<ConcurrentQueue<LocalFileInfo>> _queueLazy;

        private ISplitterParser splitterParser => _fileParserStore.SplitterParser;
        private ConcurrentQueue<LocalFileInfo> localFileQueue => _queueLazy.Value;

        private LocalFileInfo currentLocalFileInfo;
        private ILinker mainLinker;

        public FileSegmenter(ILinkerFactory linkerFactory, IFileParserStore fileParserStore)
        {
            _linkerFactory = linkerFactory;
            _fileParserStore = fileParserStore;
        }

        public void SetMainLinker(ILinker mainLinker)
        {
            this.mainLinker = mainLinker;
            Set(mainLinker);
        }

        private void Set(ILinker linker)
        {
            linker.Register<SplitterDto>(ReceiveSplitterDto);
        }

        private void ReceiveSplitterDto(SplitterDto splitterDto)
        {
            //splitterParser.Write(crruentFileInfo, splitterDto.Bytes, splitterDto.ChunkIndex, fileOperate: fileOperate);
            splitterDto.Dispose();
        }

        public void SendFileAsync(LocalFileInfo info, int chukSize = DefaultSendLength)
        {
            ////ThrowNotConnected();

            ////检查大小，小的文件直接传输，大的文件需要分块传输
            var splitterInfo = splitterParser.Create(info, chukSize, false);

            if (splitterInfo.Length <= DefaultSendLength)
            {
                //小文件直接传输
                var splitterDto = splitterParser.GetSplitterDto(info, 0, splitterInfo);
                //_linker.Send(splitterDto);
                splitterDto.Dispose();
                return;
            }

            //大文件分段传输
            IConcurrentOperate? fileOperate = splitterParser.GetOperate(info.CreateWriteOperate());
            for (uint i = 0; i < splitterInfo.ChunkCount; i++)
            {
                var splitterDto = splitterParser.GetSplitterDto(info, i, splitterInfo, fileOperate);
                //_linker.Send(splitterDto);
                splitterDto.Dispose();
            }
        }
    }
}
