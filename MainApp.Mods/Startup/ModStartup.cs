using AppHost.Builder;

namespace MainApp.Mods
{
    public abstract class ModStartup : Startup
    {
        public abstract void CreateModDetails(ModDetails details);
    }
}
