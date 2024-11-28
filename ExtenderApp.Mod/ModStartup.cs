using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;
using ExtenderApp.Mods;


namespace ExtenderApp.Mod
{
    internal class ModStartup : Startup
    {
        ////private const string MOD_INIT_FILE_NAME = "init.json";
        ////private const string MOD_PACKAGE_NAME = "packge";

        //public override void Start(IHostApplicationBuilder builder)
        //{
        //    //var environment = builder.HostEnvironment;
        //    //string modFolderPath = Path.Combine(environment.ContentRootPath, AppSetting.AppModsFolderName);

        //    //ModStore store = new();

        //    //foreach (var dir in Directory.GetDirectories(modFolderPath))
        //    //{
        //    //    var infoData = new FileInfoData(Path.Combine(dir, MOD_INIT_FILE_NAME));
        //    //    if (!infoData.Exists) continue;

        //    //    //解析模组的信息
        //    //    ModeInfo info;
        //    //    using (FileStream stream = new FileStream(infoData.Path, FileMode.Open, FileAccess.Read))
        //    //    {
        //    //        info = JsonSerializer.Deserialize<ModeInfo>(stream);
        //    //    }
        //    //    if (string.IsNullOrEmpty(info.ModStartupDll)) continue;

        //    //    //加载模组主程序集
        //    //    ModDetails details = new ModDetails(info);
        //    //    //details.AddModAssembly(builder, dir);

        //    //    ////添加模组依赖库
        //    //    //var packPath = Path.Combine(dir, AppSetting.AppPackFolderName);
        //    //    //if (Directory.Exists(packPath))
        //    //    //{
        //    //    //    foreach (var packDllPath in Directory.GetFiles(Path.Combine(dir, AppSetting.AppPackFolderName), "*.dll"))
        //    //    //    {
        //    //    //        Assembly.LoadFile(packDllPath);
        //    //    //    }
        //    //    //}

        //    //    store.Add(details);
        //    //}

        //    //builder.Services.AddSingleton(store);

        //    base.Start(builder);
        //}

        public override void AddService(IServiceCollection services)
        {
            services.AddHosted<ModsHostedService>();
            services.AddSingleton<ModStore>();
            services.AddSingleton<IModLoader, ModLoader>();
        }
    }
}
