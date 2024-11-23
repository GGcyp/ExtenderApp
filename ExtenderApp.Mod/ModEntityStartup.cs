using AppHost.Builder;


namespace ExtenderApp.Mod
{
    public abstract class ModEntityStartup : Startup
    {
        public abstract Type StartType { get; }
    }
}
