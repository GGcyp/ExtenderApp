﻿using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.MainViews
{
    internal class MainViewStartup : Startup
    {
        public override void AddService(IServiceCollection services)
        {
            services.AddSingleton<IMainWindow, MainViewWindow>();
            services.AddSingleton<DisplayDetailsList>();

            AddMainView(services);

            services.AddTransient<PluginView>();
            services.AddTransient<PluginViewModle>();
        }

        private void AddMainView(IServiceCollection services)
        {
            services.AddTransient<IMainView, MainView>();
            services.AddTransient<MainView_Run>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainModel>();
        }
    }
}
