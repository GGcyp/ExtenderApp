using AppHost.Extensions.DependencyInjection;


namespace ExtenderApp.Mod
{
    public abstract class ModEntityStartup : ScopeStartup
    {
        public abstract Type StartType { get; }
    }
}
