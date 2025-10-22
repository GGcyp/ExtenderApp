using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    public interface IClientPluginManager
    {
        void AddPlugin(IClientPlugin plugin);

        void InvokePlugins<T>(Action<T, LinkerClientContext> action, LinkerClientContext context) where T : IClientPlugin;
        void InvokePlugins<T>(IClient client) where T : IPersistentPlugin;
    }
}
