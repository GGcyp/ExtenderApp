using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Mod;
using MachineLearning.view;

namespace MachineLearning
{
    internal class MachineLearningStartup : ModEntityStartup
    {
        public override Type StartType => typeof(MachineLearningMainView);

        public override string ScopeName => "MachineLearning";

        public override void AddService(IServiceCollection services)
        {
            services.AddTransient<MachineLearningMainView>();
        }
    }
}
