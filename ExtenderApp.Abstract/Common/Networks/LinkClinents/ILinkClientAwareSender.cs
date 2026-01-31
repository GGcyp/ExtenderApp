using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    public interface ILinkClientAwareSender : ILinkClient
    {
        ILinkClientFormatterManager? FormatterManager { get; }

        void SetClientFormatterManager(ILinkClientFormatterManager formatterManager);

        Result<SocketOperationValue> Send<T>(T data);

        ValueTask<Result<SocketOperationValue>> SendAsync<T>(T data, CancellationToken token = default);
    }
}