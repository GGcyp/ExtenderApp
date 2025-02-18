﻿using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Services;


namespace ExtenderApp.ML
{
    internal class MachineLearningStartup : PluginEntityStartup
    {
        public override Type StartType => typeof(MachineLearningMainView);

        public override void AddService(IServiceCollection services)
        {
            services.AddTransient<MachineLearningMainView>();
        }
    }
}
