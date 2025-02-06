using ExtenderApp.Services;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common;

namespace ExtenderApp.LAN
{
    internal class LANStartup : ModEntityStartup
    {
        public override Type StartType => typeof(LANMainView);

        public override void AddService(IServiceCollection services)
        {
            services.AddSingleton<LANMainView>();
            services.AddSingleton<LANMainViewModel>();
        }

        public override void ConfigureBinaryFormatterStore(IBinaryFormatterStore store)
        {
            store.Add<LANModel, LANModelFormatter>();
        }
    }
}
