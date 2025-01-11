using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;


namespace ExtenderApp.Services
{
    public abstract class ModEntityStartup : ScopeStartup
    {
        public abstract Type StartType { get; }

        public override void ConfigureScopeOptions(ScopeOptions options)
        {
            options.ScopeName = GetType().Name;
        }

        public virtual void ConfigureBinaryFormatterStore(IBinaryFormatterStore store)
        {

        }
    }
}
