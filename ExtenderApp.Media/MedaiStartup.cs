using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Services;
using ExtenderApp.Common;
using ExtenderApp.Media.Model;


namespace ExtenderApp.Media
{
    internal class MedaiStartup : ModEntityStartup
    {
        public override Type StartType => typeof(MediaMainView);

        public override void AddService(IServiceCollection services)
        {
            //View
            services.AddTransient<MediaMainView>();
            services.AddTransient<PlaybackView>();


            //ViewModel
            services.AddSingleton<MediaMainViewModel>();


            //Model
            services.AddSingleton<VideoModel>();
        }

        public override void ConfigureBinaryFormatterStore(IBinaryFormatterStore store)
        {
            store.Add<VideoInfo, VideoInfoFormatter>();
        }
    }
}
