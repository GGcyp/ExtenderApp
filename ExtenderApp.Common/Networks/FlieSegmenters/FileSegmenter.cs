using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class FileSegmenter
    {
        private const int DefaultLinkerCapacity = 2;
        public const int DefaultChuckSize = 1024 * 1024;

        private readonly ILinkerFactory _linkerFactory;
        private readonly IFileParserStore _fileParserStore;
        private readonly Lazy<ConcurrentDictionary<int, LocalFileInfo>> _dictLazy;
        private readonly Lazy<ConcurrentQueue<(FileInfoDto, LocalFileInfo)>> _getFileInfoLazy;
        private readonly Lazy<ConcurrentQueue<ILinker>> _linkersLazy;

        private ISplitterParser splitterParser => _fileParserStore.SplitterParser;
        private ConcurrentDictionary<int, LocalFileInfo> fileInfoDict => _dictLazy.Value;
        private ConcurrentQueue<(FileInfoDto, LocalFileInfo)> getFileInfoQueue => _getFileInfoLazy.Value;
        private ConcurrentQueue<ILinker> linkerQueue => _linkersLazy.Value;


        private SplitterInfo currentSplitterInfo;
        private LocalFileInfo currentLocalFileInfo;
        private FileConcurrentOperate currentFileOperate;
        private ILinker mainLinker;
        private volatile int isGetFileing;
        private int linkerCount;
        private int linkerCapacity;

        public event Action<FileInfoDto[]>? OnReceiveFileInfos;

        public FileSegmenter(ILinkerFactory linkerFactory, IFileParserStore fileParserStore)
        {
            _linkerFactory = linkerFactory;
            _fileParserStore = fileParserStore;
            _dictLazy = new();
            _getFileInfoLazy = new();
        }

        public void SetMainLinker(ILinker mainLinker)
        {
            this.mainLinker = mainLinker;
            Set(mainLinker);
            //mainLinker.Register<FileTransferRequestDto>(ReceiveFileTransferRequestDto);
            //mainLinker.Register<FileTransferConfigDto>(ReceiveFileTransferConfigDto);
            //mainLinker.Register<FileSplitterInfoRequestDto>(ReceiveFileSplitterInfoRequestDto);
            //mainLinker.Register<SplitterInfo>(ReceiveSplitterInfo);
        }

        private void Set(ILinker linker)
        {
            //linker.Register<SplitterDto>(ReceiveSplitterDto);
        }

        #region Receive

        private void ReceiveSplitterDto(SplitterDto splitterDto, ILinker linker)
        {
            //if (!splitterDto.CompliantMD5())
            //{
            //    splitterParser.Write(currentFileOperate, currentSplitterInfo, splitterDto);
            //}

            currentSplitterInfo.LoadChunk(splitterDto);

            splitterParser.WriteAsync(currentFileOperate, currentSplitterInfo, splitterDto);

            if (linkerCount > linkerCapacity)
            {
                //_linkerFactory.ReleaseLinker(linker);
            }
            else
            {
                linkerQueue.Enqueue(linker);
            }

            if (Interlocked.CompareExchange(ref isGetFileing, 1, 0) == 0)
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ => GetFileExecute(), null);
            }
        }

        private void ReceiveFileTransferRequestDto(FileTransferRequestDto dto)
        {
            OnReceiveFileInfos?.Invoke(dto.FileInfoDtos);
        }

        private void ReceiveFileTransferConfigDto(FileTransferConfigDto dto)
        {
            linkerCapacity = dto.LinkerCount > 0 ? dto.LinkerCount : DefaultLinkerCapacity;
        }

        private void ReceiveFileSplitterInfoRequestDto(FileSplitterInfoRequestDto dto)
        {
            //if (!fileInfoDict.TryGetValue(dto.FileHashCode, out var localFileInfo))
            //{
            //    mainLinker.Send(new ErrorDto()
            //    {
            //        StatrCode = 404,
            //        Message = string.Format("未找到指定文件:{0}", dto.FileHashCode.ToString())
            //    });
            //}
            //var splitterInfo = splitterParser.CreateInfoForFile(localFileInfo, false);
            //mainLinker.Send(splitterInfo);
        }

        private void ReceiveSplitterInfo(SplitterInfo splitterInfo)
        {
            currentSplitterInfo = splitterInfo;
        }

        #endregion

        #region SendFile

        public void SendCanGetFileAsync(LocalFileInfo info)
        {
            if (mainLinker == null)
                throw new ArgumentNullException(nameof(mainLinker));

            fileInfoDict.TryAdd(info.GetHashCode(), info);

            FileTransferRequestDto fileRequestDto = new FileTransferRequestDto(new FileInfoDto[1] { info });
            //mainLinker.SendAsync(fileRequestDto);
        }

        public void SendCanGetFilesAsync(IEnumerable<LocalFileInfo> infos)
        {
            if (mainLinker == null)
                throw new ArgumentNullException(nameof(mainLinker));

            var sendArray = new FileInfoDto[infos.Count()];
            int index = 0;
            foreach (var fileInfo in infos)
            {
                fileInfoDict.TryAdd(((FileInfoDto)fileInfo).GetHashCode(), fileInfo);
                sendArray[index] = fileInfo;
                index++;
            }

            FileTransferRequestDto fileRequestDto = new FileTransferRequestDto(sendArray);
            //mainLinker.SendAsync(fileRequestDto);
        }

        #endregion

        #region GetFile

        public void GetFileAsync(FileInfoDto fileInfo, LocalFileInfo localFileInfo)
        {
            getFileInfoQueue.Enqueue((fileInfo, localFileInfo));
            if (Interlocked.CompareExchange(ref isGetFileing, 1, 0) == 0)
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ => GetFileExecute(), null);
            }
        }

        public void GetFilesAsync(IEnumerable<(FileInfoDto, LocalFileInfo)> fileInfos)
        {
            foreach (var fileInfo in fileInfos)
            {
                getFileInfoQueue.Enqueue(fileInfo);
            }

            if (Interlocked.CompareExchange(ref isGetFileing, 1, 0) == 0)
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ => GetFileExecute(), null);
            }
        }

        private void GetFileExecute()
        {
            while (getFileInfoQueue.TryDequeue(out var item))
            {
                //mainLinker.SendAsync()
            }
        }

        #endregion

        private void Sending(LocalFileInfo fileInfo, int chukSize)
        {
            //检查大小，小的文件直接传输，大的文件需要分块传输
            //var splitterInfo = splitterParser.Create(info, chukSize, false);

            //if (splitterInfo.Length <= DefaultSendLength)
            //{
            //    //小文件直接传输
            //    var splitterDto = splitterParser.GetSplitterDto(info, 0, splitterInfo);
            //    //_linker.Send(splitterDto);
            //    splitterDto.Dispose();
            //    return;
            //}

            ////大文件分段传输
            //IConcurrentOperate? fileOperate = splitterParser.GetOperate(info.CreateWriteOperate());
            //for (uint i = 0; i < splitterInfo.ChunkCount; i++)
            //{
            //    var splitterDto = splitterParser.GetSplitterDto(info, i, splitterInfo, fileOperate);
            //    //_linker.Send(splitterDto);
            //    splitterDto.Dispose();
            //}
        }
    }
}
