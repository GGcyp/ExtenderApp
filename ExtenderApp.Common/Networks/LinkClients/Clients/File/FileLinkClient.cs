using System.Net;
using System.Threading.Tasks.Sources;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal class FileLinkClient : TransferLinkClient<ITcpLinker>, IFileLinkClient, IValueTaskSource<Result>
    {
        private const int DefaultChunkSize = 65536;
        private const int FileProt = 88883;

        private readonly IServiceProvider _provider;
        private readonly IFileOperateProvider _fileOperateProvider;
        private readonly SemaphoreSlim _slim;

        private ITcpListenerLinker? listenerLinker;
        private ManualResetValueTaskSourceCore<Result> vts;
        public short Version => vts.Version;

        public event EventHandler<FileRequestDecision>? RequestReceived;

        public FileLinkClient(ITcpLinker linker, IServiceProvider provider) : base(linker)
        {
            vts = new();
            _slim = new(1, 1);
            _fileOperateProvider = provider.GetRequiredService<IFileOperateProvider>();
            _provider = provider;

            //FormatterManager!.AddBinaryFormatter<FileDataPacket>(OnFilePacket);
            //FormatterManager!.AddBinaryFormatter<FileDtoRequest>(OnRequest);
            //FormatterManager!.AddBinaryFormatter<FileResponse>(OnResponse);
        }

        private void OnResponse(LinkClientReceivedValue<FileResponse> response)
        {
            if (!response.Value.IsAccepted)
            {
                vts.SetResult(Result.Failure("文件推送请求被拒绝。"));
                return;
            }
        }

        private void OnFilePacket(LinkClientReceivedValue<FileDataPacket> packet)
        {
            throw new NotImplementedException();
        }

        private void OnRequest(LinkClientReceivedValue<FileDtoRequest> request)
        {
            OnRequestAsync(request).ConfigureAwait(false);
        }

        private async Task OnRequestAsync(FileDtoRequest request)
        {
            FileRequestDecision decision = new(request);
            RequestReceived?.Invoke(this, decision);

            var result = await decision.GetResult();

            if (!result)
            {
                await SendAsync(new FileResponse(false));
                return;
            }

            var bitField = result.Value;
            var requestList = request.FileDtos;
            ValueOrList<FileDto> dtos = new(bitField.TrueCount);
            for (int i = 0; i < bitField.Length; i++)
            {
                if (bitField[i])
                {
                    dtos.Add(requestList[i]);
                }
            }
            FileResponse response = new(result, dtos);
            await SendAsync(response);

            CreateListenerLinker();
        }

        #region Push

        public ValueTask<Result> PushFileAsync(FileOperateInfo info, int chunkSize = DefaultChunkSize, CancellationToken token = default)
        {
            if (info.IsEmpty)
                // 对于同步抛出的异常，直接 throw 是最清晰的
                return ValueTask.FromException<Result>(new InvalidOperationException("文件地址不能为空"));
            if (info.IsWrite())
                return ValueTask.FromException<Result>(new InvalidOperationException("无法读取文件，操作类型为只写入"));

            var operate = _fileOperateProvider.GetOperate(info, FileOperateType.ConcurrentFileStream);
            return PrivatePushFileAsync(operate, chunkSize, token);
        }

        public ValueTask<Result> PushFileAsync(IFileOperate operate, int chunkSize = 65536, CancellationToken token = default)
        {
            if (operate is null)
                return ValueTask.FromException<Result>(new ArgumentNullException(nameof(operate)));
            if (!operate.CanRead)
                return ValueTask.FromException<Result>(new InvalidOperationException("当前文件无法读取"));

            _slim.Wait(token);

            return PrivatePushFileAsync(operate, chunkSize, token);
        }

        private ValueTask<Result> PrivatePushFileAsync(IFileOperate operate, int chunkSize = DefaultChunkSize, CancellationToken token = default)
        {
            vts.Reset();
            var dto = GetFileDto(operate, chunkSize);

            FileDtoRequest request = FileDtoRequest.Push(dto);
            return PrivateSendRequestAsync(request, token);
        }

        #endregion Push

        private ValueTask<Result> PrivateSendRequestAsync(FileDtoRequest request, CancellationToken token)
        {
            // 异步发送请求
            var sendValueTask = SendAsync(request, token);

            // 检查发送操作是否已同步完成
            if (sendValueTask.IsCompletedSuccessfully)
            {
                var result = sendValueTask.GetAwaiter().GetResult();
                if (!result)
                {
                    return ValueTask.FromException<Result>(result.Exception ?? new Exception("发送文件推送请求失败。"));
                }
            }

            // 操作将异步完成，返回 false
            return new ValueTask<Result>(this, Version);
        }

        private FileDto GetFileDto(IFileOperate operate, int chunkSize)
        {
            var fileId = Guid.NewGuid();
            var fileName = operate.Info.FileName;
            var fileSize = operate.Info.Length;
            var chunkCount = (fileSize + chunkSize - 1) / chunkSize;
            return new FileDto(fileId, fileName, fileSize, chunkCount, chunkSize);
        }

        private void CreateListenerLinker()
        {
            if (listenerLinker is not null)
                return;

            listenerLinker = _provider.GetRequiredService<ITcpListenerLinker>();
            listenerLinker.Bind(new IPEndPoint(IPAddress.Any, FileProt));
            listenerLinker.Accept += OnAccept;
            listenerLinker.Listen();
        }

        private void OnAccept(object? sender, ITcpLinker e)
        {
            throw new NotImplementedException();
        }

        #region ValueTaskSource

        public Result GetResult(short token)
        {
            try
            {
                return vts.GetResult(token);
            }
            finally
            {
                _slim.Release(); // 在获取结果后释放信号量
            }
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            return vts.GetStatus(token);
        }

        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            vts.OnCompleted(continuation, state, token, flags);
        }

        #endregion ValueTaskSource
    }
}