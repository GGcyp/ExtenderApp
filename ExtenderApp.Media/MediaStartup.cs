
using ExtenderApp.Abstract;
using ExtenderApp.Services;
using ExtenderApp.Common;
using ExtenderApp.Media.Models;
using ExtenderApp.Media.ViewModels;
using ExtenderApp.Data;
using System.IO;
using ExtenderApp.FFmpegEngines;
using Microsoft.Extensions.DependencyInjection;


namespace ExtenderApp.Media
{
    internal class MediaStartup : PluginEntityStartup
    {
        private const string ffmpegFolderName = "ffmpegLibs";

        public override Type StartType => typeof(MediaMainView);

        public override void AddService(IServiceCollection services)
        {
            //View
            services.AddTransient<MediaMainView>();
            services.AddTransient<VideoView>();
            services.AddTransient<VideoListView>();


            //ViewModel
            services.AddTransient<MediaMainViewModel>();
            services.AddTransient<VideoViewModel>();
            services.AddTransient<VideoListViewModel>();


            //FFmpegEngine
            AddFFmpegEngines(services);
            services.AddSingleton<MediaEngine>();
        }

        public override void ConfigureBinaryFormatterStore(IBinaryFormatterStore store)
        {
            store.Add<MediaInfo, MediaInfoFormatter>();
            store.AddLocalDataFormatter<MediaModel, MediaModelFormatter>();
        }

        private static IServiceCollection AddFFmpegEngines(IServiceCollection services)
        {
            services.AddSingleton<FFmpegEngine>((p) =>
            {
                var details = p.GetRequiredService<PluginDetails>();
                var ffmpegPath = Path.Combine(details.PluginFolderPath!, ffmpegFolderName);

                FFmpegEngine engine = new(ffmpegPath);

                return engine;
            });
            return services;
        }
    }
}
