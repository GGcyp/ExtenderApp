using AppHost.Builder;


namespace ExtenderApp.Mod
{
    public abstract class ModEntityStartup : ScopeStartup
    {
        public abstract Type StartType { get; }
    }
}
