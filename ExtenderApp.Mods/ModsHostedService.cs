using System.IO;
using AppHost.Extensions.Hosting;
using ExtenderApp.Common;
using ExtenderApp.Mod;

namespace ExtenderApp.Mods
{
    internal class ModsHostedService : BackgroundService
    {
        private const string MOD_INIT_FILE_NAME = "init.json";

        public ModsHostedService(ModStore store, IHostEnvironment environment, IJsonPareserProvider provider)
        {
            string modFolderPath = Path.Combine(environment.ContentRootPath, AppSetting.AppModsFolderName);

            foreach (var dir in Directory.GetDirectories(modFolderPath))
            {
                var info = provider.Deserialize<ModeInfo>(new FileInfoData(Path.Combine(dir, MOD_INIT_FILE_NAME)));
                if (info is null) continue;

                ModDetails details = new ModDetails(info)
                {
                    ModPath = dir,
                };

                store.Add(details);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
