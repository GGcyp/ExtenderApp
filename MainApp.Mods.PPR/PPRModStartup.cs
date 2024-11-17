using AppHost.Extensions.DependencyInjection;
using MainApp.Models.Converters;
using MainApp.Models.Converters.Extensions;
using AppHost.Builder;

namespace MainApp.Mod.PPR
{
    public class PPRModStartup : Startup
    {
        protected override void AddService(IServiceCollection services)
        {
            services.AddTransient<PPRViewModel>();
            services.AddTransient<IPPRModel, PPRModel>();
            services.AddModelConverterExecutor<IPPRModel>(s =>
            {
                s.AddModelConvertPolicy<PPRXmlReaderPolicy>();
                s.AddModelConvertPolicy<PPRExcelPolicy>();
            });
        }
    }
}
 