

using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    public interface ILinkClient : ILinker
    {
        ILinkClientFormatterManager? FormatterManager { get; }
        ILinkClientPluginManager? PluginManager { get; }

        ValueTask<SocketOperationResult> SendAsync<T>(T data);
        void SetClientFormatterManager(ILinkClientFormatterManager formatterManager);
        void SetClientPluginManager(ILinkClientPluginManager pluginManager);
    }
}
