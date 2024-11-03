using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;

namespace MainApp.Mods
{
    internal class ModsStartup : Startup
    {
        public override void Start(IHostApplicationBuilder builder)
        {
            builder.FindStarupForFolder(AppSetting.AppModelFolderName, out List<ModStartup> list);

            ModStore modDetails = new ModStore(list.Count);
            for(int i = 0; i < list.Count; i++)
            {
                ModDetails details = new();
                list[i].CreateModDetails(details);
                modDetails.Add(details);
            }

            builder.Services.AddSingleton(modDetails);
        }

        protected override void AddService(IServiceCollection services)
        {
            services.AddSingleton<ModStore>();
        }
    }
}
