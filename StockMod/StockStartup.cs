using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Mod;

namespace StockMod
{
    internal class StockStartup : ModEntityStartup
    {
        public override Type StartType => typeof(StockMainViewControl);

        public override string ScopeName => "Stock";

        public override void AddService(IServiceCollection services)
        {
            services.AddSingleton<StockMainViewControl>();
            services.AddTransient<StockMainViewModel>();
        }
    }
}
