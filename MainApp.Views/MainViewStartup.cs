using AppHost.Builder;
using AppHost.Extensions.Hosting;

namespace MainApp.Views
{
    internal class MainViewStartup : Startup
    {
        public override void Start(IHostApplicationBuilder builder)
        {
            builder.Services.AddHosted<MainViewHostedService>();
        }
    }
}
