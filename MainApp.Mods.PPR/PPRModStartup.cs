using AppHost.Extensions.DependencyInjection;
using MainApp.Models.Converters;
using MainApp.Models.Converters.Extensions;

namespace MainApp.Mods.PPR
{
    public class PPRModStartup : ModStartup
    {
        public override void CreateModDetails(ModDetails details)
        {
            details.Title = "工程进度插件";
            details.Description = "记录工程进度数据";
        }

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
 