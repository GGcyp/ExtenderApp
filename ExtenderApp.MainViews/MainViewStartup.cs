﻿using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.MainViews.ViewModels;
using ExtenderApp.MainViews.Models;
using ExtenderApp.MainViews.Windows;
using ExtenderApp.MainViews.Views;

namespace ExtenderApp.MainViews
{
    internal class MainViewStartup : Startup
    {
        public static string MainViewTitle = "Main";

        public override void AddService(IServiceCollection services)
        {
            //services.AddSingleton<IMainWindow, MainViewWindow>();
            services.AddTransient<IWindow, ExtenderDefaultWindow>();
            services.AddTransient<ExtenderDefaultWindowViewModel>();
            services.AddSingleton<IMainWindowFactory, MainWindowFactory>();

            services.AddTransient<PluginView>();
            services.AddTransient<PluginViewModle>();
            services.AddTransient<MainWindowViewModel>();

            services.AddTransient<IMainView, MainView>();
            services.AddTransient<MainView>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainView_RunView>();
            services.AddTransient<MainView_RunViewModel>();
            services.AddSingleton<MainModel>();

            services.AddTransient<SettingsView>();
            services.AddTransient<SettingsViewModel>();
        }
    }
}
