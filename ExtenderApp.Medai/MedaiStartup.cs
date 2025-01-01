using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Services;


namespace ExtenderApp.Medai
{
    internal class MedaiStartup : ModEntityStartup
    {
        public override Type StartType => typeof(MedaiMainView);

        public override void AddService(IServiceCollection services)
        {
            //View
            services.AddTransient<MedaiMainView>();
            services.AddTransient<PlaybackView>();


            //ViewModel
            services.AddSingleton<MedaiMainViewModel>();


            //Model
            services.AddSingleton<VideoModel>();
        }
    }
}
