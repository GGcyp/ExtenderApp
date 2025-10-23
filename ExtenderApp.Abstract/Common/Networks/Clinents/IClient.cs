

using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    public interface IClient : ILinker
    {
        IClientFormatterManager? FormatterManager { get; }
        IClientPluginManager? PluginManager { get; }

        ValueTask<SocketOperationResult> SendAsync<T>(T data);
        void SetClientFormatterManager(IClientFormatterManager formatterManager);
        void SetClientPluginManager(IClientPluginManager pluginManager);
    }
}
