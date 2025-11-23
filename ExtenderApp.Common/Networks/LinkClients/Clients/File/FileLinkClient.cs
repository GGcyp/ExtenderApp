using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal class FileLinkClient : LinkClientAwareSender<IFileLinkClient, ITcpLinker>, IFileLinkClient
    {
        public event EventHandler<PushFileRequest>? RequestReceived;

        private readonly IFileOperateProvider _fileOperateProvider;
        private readonly ConcurrentDictionary<Guid, IFileOperate> _pushDict;

        public FileLinkClient(ITcpLinker linker, IServiceProvider provider) : base(linker)
        {
            _pushDict = new();
            _fileOperateProvider = provider.GetRequiredService<IFileOperateProvider>();

            FormatterManager ??= new LinkClientFormatterManager();
            FormatterManager.AddBinaryLinkClientFormatters<PushFileResponse>(provider, OnDataPacket);
            FormatterManager.AddBinaryLinkClientFormatters<PushFileRequest>(provider, OnRequest);
            FormatterManager.AddBinaryLinkClientFormatters<FileDataPacket>(provider, OnResponse);
        }

        private void OnRequest(PushFileRequest request)
        {
            RequestReceived?.Invoke(this, request);
        }

        private void OnResponse(FileDataPacket response)
        {
        }

        private void OnDataPacket(PushFileResponse packet)
        {
        }

        public ValueTask<Result> PushFileAsync(FileOperateInfo info, int chunkSize = 65536, CancellationToken token = default)
        {
            if (info.IsEmpty)
                // 对于同步抛出的异常，直接 throw 是最清晰的
                return ValueTask.FromException<Result>(new InvalidOperationException("文件地址不能为空"));
            if (info.IsWrite())
                return ValueTask.FromException<Result>(new InvalidOperationException("无法读取文件，操作类型为只写入"));

            var operate = _fileOperateProvider.GetOperate(info);
            return PrivatePushFileAsync(operate, chunkSize, token);
        }

        public ValueTask<Result> PushFileAsync(IFileOperate operate, int chunkSize = 65536, CancellationToken token = default)
        {
            if (operate is null)
                return ValueTask.FromException<Result>(new ArgumentNullException(nameof(operate)));
            if (!operate.CanRead)
                return ValueTask.FromException<Result>(new InvalidOperationException("当前文件无法读取"));

            return PrivatePushFileAsync(operate, chunkSize, token);
        }

        private async ValueTask<Result> PrivatePushFileAsync(IFileOperate operate, int chunkSize = 65536, CancellationToken token = default)
        {
            var fileId = operate.GetFileGuid();
            var fileLength = operate.Info.Length;
            var chunkCount = (fileLength + chunkSize - 1) / chunkSize;
            PushFileRequest request = new(fileId, operate.Info.FileName, fileLength, chunkCount, chunkSize);

            if (!_pushDict.TryAdd(fileId, operate))
            {
                return Result.Error(new InvalidOperationException("文件ID重复，可能已有相同文件正在传输。"));
            }

            try
            {
                // 等待发送操作完成
                var sendResult = await SendAsync(request, token);
                if (sendResult.IsSuccess)
                {
                    // 这里可以等待对方的响应，或者直接认为请求发送成功
                    return Result.Success();
                }
                else
                {
                    // 如果发送失败，返回一个包含套接字错误的 Result
                    return Result.Error(sendResult.SocketError ?? new Exception("发送文件推送请求失败。"));
                }
            }
            catch (Exception ex)
            {
                // 如果在发送过程中发生异常，捕获并返回一个错误 Result
                _pushDict.TryRemove(fileId, out _); // 清理字典
                return Result.Error(ex);
            }
        }

        private void FileDataPacket()
        {

        }
    }
}