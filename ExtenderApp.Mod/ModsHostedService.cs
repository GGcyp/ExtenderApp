using System.IO;
using AppHost.Extensions.Hosting;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;
using ExtenderApp.Mod;

namespace ExtenderApp.Mods
{
    internal class ModsHostedService : BackgroundService
    {
        public ModsHostedService()
        {

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
