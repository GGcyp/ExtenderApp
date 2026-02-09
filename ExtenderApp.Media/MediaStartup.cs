using System.IO;
using ExtenderApp.Common;
using ExtenderApp.Contracts;
using ExtenderApp.FFmpegEngines;
using ExtenderApp.FFmpegEngines.Medias;
using ExtenderApp.Media.ViewModels;
using ExtenderApp.Media.Views;
using ExtenderApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Media
{
    internal class MediaStartup : PluginEntityStartup
    {
        private const string ffmpegFolderName = "ffmpegLibs";

        public override Type StartType => typeof(MediaMainView);

        public override void AddService(IServiceCollection services)
        {
            services.AddView<MediaMainView, MediaMainViewModel>();
            services.AddViewModel<MediaMainViewModel>();

            //FFmpegEngine
            AddFFmpegEngines(services);
            services.AddSingleton<MediaEngine>();
        }

        private static IServiceCollection AddFFmpegEngines(IServiceCollection services)
        {
            services.AddSingleton((p) =>
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