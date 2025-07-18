﻿using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Services;
using ExtenderApp.Common.Networks;

namespace ExtenderApp.Test
{
    internal class TestStartup : PluginEntityStartup
    {
        public override Type StartType => typeof(TestMainView);

        public override void AddService(IServiceCollection services)
        {
            services.AddTransient<TestMainView>();

            services.AddTransient<TestMainViewModel>();
        }
    }
}
