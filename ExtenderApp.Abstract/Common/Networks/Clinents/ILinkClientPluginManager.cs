

namespace ExtenderApp.Abstract
{
    public interface ILinkClientPluginManager : ILinkClientPlugin
    {
        void AddPlugin(ILinkClientPlugin plugin);
    }
}
