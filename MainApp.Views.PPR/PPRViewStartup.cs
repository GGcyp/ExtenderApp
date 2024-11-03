using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;

namespace MainApp.Views
{
    public class PPRViewStartup : Startup
    {
        public override void Start(IHostApplicationBuilder builder)
        {
            builder.Services.AddTransient<IPPRView, PPRView>();
        }
    }
}
