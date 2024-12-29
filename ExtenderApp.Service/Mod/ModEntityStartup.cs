using AppHost.Builder;


namespace ExtenderApp.Services
{
    public abstract class ModEntityStartup : ScopeStartup
    {
        public abstract Type StartType { get; }
    }
}
