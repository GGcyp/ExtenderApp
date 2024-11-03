using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using MainApp.Abstract;

namespace MainApp.MainView
{
    internal class MainViewStartup : Startup
    {
        public override void Start(IHostApplicationBuilder builder)
        {
            AddService(builder.Services);
        }

        private void AddService(IServiceCollection services)
        {
            services.AddScoped<IMainView, MainViewWindow>();
            services.AddScoped<MainViewModel>();
        }
    }
}
