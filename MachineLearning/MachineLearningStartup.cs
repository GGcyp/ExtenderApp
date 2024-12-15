using AppHost.Extensions.DependencyInjection;
using ExtenderApp.ML.View;
using ExtenderApp.Mod;

namespace ExtenderApp.ML
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
