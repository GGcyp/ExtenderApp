using AppHost.Extensions.Hosting;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;

namespace ExtenderApp.Mod
{
    internal class ModsHostedService : BackgroundService
    {
        private const string MOD_INIT_FILE_NAME = "init.json";

        public ModsHostedService(ModStore store, IPathProvider pathProvider, IJsonPareserProvider provider, IScopeExecutor executor)
        {
            string modFolderPath = pathProvider.ModsPath;

            foreach (var dir in Directory.GetDirectories(modFolderPath))
            {
                var infoData = new FileInfoData(Path.Combine(dir, MOD_INIT_FILE_NAME));
                if (!infoData.Exists) continue;

                //解析模组的信息
                ModeInfo info = provider.Deserialize<ModeInfo>(infoData);

                if (string.IsNullOrEmpty(info.ModStartupDll)) continue;

                //加载模组主程序集
                ModDetails details = new ModDetails(info);
                details.Path = dir;

                store.Add(details);
                //details.StartupType = executor.LoadScope<ModEntityStartup>(Path.Combine(dir, details.StartupDll)).StartType;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
