using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Services;
using ExtenderApp.Common;
using ExtenderApp.Media.Model;


namespace ExtenderApp.Media
{
    internal class MedaiStartup : PluginEntityStartup
    {
        public override Type StartType => typeof(MediaMainView);

        public override void AddService(IServiceCollection services)
        {
            //View
            services.AddTransient<MediaMainView>();
            services.AddTransient<VideoView>();
            services.AddTransient<VideoListView>();


            //ViewModel
            services.AddTransient<MediaMainViewModel>();
            services.AddTransient<VideoViewModle>();
            services.AddTransient<VideoListViewModle>();
        }

        public override void ConfigureBinaryFormatterStore(IBinaryFormatterStore store)
        {
            store.Add<VideoInfo, VideoInfoFormatter>();
            store.Add<MediaModel, MediaModelFormatter>();
        }
    }
}
