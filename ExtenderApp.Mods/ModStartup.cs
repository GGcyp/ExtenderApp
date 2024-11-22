using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using AppHost.Extensions.Hosting;
using ExtenderApp.Mods;

namespace ExtenderApp.Mod
{
    internal class ModStartup : Startup
    {
        protected override void AddService(IServiceCollection services)
        {
            services.AddSingleton<ModStore>();
            services.AddHosted<ModsHostedService>();
        }
    }
}
