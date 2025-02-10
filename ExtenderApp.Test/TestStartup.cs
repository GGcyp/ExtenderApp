using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Service;
using ExtenderApp.Services;

namespace ExtenderApp.Test
{
    internal class TestStartup : ModEntityStartup
    {
        public override Type StartType => typeof(TestMainView);

        public override void AddService(IServiceCollection services)
        {
            services.AddTransient<TestMainView>();

            services.AddSingleton<TestMainViewModel>();
        }
    }
}
