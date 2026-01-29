using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    public interface ILinkClientAwareSender : ILinkClient
    {
        ILinkClientPluginManager? PluginManager { get; }
        ILinkClientFramer? Framer { get; }
        ILinkClientFormatterManager? FormatterManager { get; }


        void SetClientFramer(ILinkClientFramer framer);
        void SetClientPluginManager(ILinkClientPluginManager pluginManager);
        void SetClientFormatterManager(ILinkClientFormatterManager formatterManager);


        void Send<T>(T data);
        ValueTask<Result<SocketOperationValue>> SendAsync<T>(T data, CancellationToken token = default);
    }
}