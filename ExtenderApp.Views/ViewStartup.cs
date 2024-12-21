using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;

namespace ExtenderApp.Views
{
    internal class ViewStartup : Startup
    {
        public override void Start(IHostApplicationBuilder builder)
        {
            builder.Services.AddHosted<MainViewHostedService>();
            builder.Services.AddSingleton<IDispatcherService>(new Dispatcher_WPF());
        }
    }
}
