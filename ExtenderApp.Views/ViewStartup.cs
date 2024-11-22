using AppHost.Builder;
using AppHost.Extensions.Hosting;

namespace ExtenderApp.Views
{
    internal class ViewStartup : Startup
    {
        public override void Start(IHostApplicationBuilder builder)
        {
            builder.Services.AddHosted<MainViewHostedService>();
        }
    }
}
