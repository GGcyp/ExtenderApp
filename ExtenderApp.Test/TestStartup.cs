using ExtenderApp.Common;
using ExtenderApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Test
{
    internal class TestStartup : PluginEntityStartup
    {
        public override Type StartType => typeof(TestMainView);

        public override void AddService(IServiceCollection services)
        {
            services.AddView<TestMainView, TestMainViewModel>();

            services.AddViewModel<TestMainViewModel>();
        }
    }
}