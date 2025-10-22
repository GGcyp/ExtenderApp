

namespace ExtenderApp.Abstract
{
    public interface IPersistentPlugin : IClientPlugin
    {
        void Inject(IClient client);
    }
}
