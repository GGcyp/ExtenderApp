using System.IO;
using System.Runtime.InteropServices;
using ExtenderApp.Data;
using ExtenderApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.LAN
{
    internal class LanStartup : PluginEntityStartup
    {
        public override Type StartType => typeof(LANMainView);

        public override void AddService(IServiceCollection services)
        {
            services.AddSingleton<LANMainView>();
            services.AddSingleton<LANMainViewModel>();
        }

        public override void ConfigureDetails(PluginDetails details)
        {
            LoadNpcapDlls(Path.Combine(details.PluginFolderPath!, "npcapLibs"));
        }

        public static bool LoadNpcapDlls(string dllPath)
        {
            try
            {
                // 拼接完整路径，优先加载32/64位对应库（需确保DLL与程序位数一致）
                string wpcapPath = Path.Combine(dllPath, "wpcap.dll");
                string packetPath = Path.Combine(dllPath, "Packet.dll");

                // 手动加载两个核心库（顺序：先加载Packet.dll，再加载wpcap.dll）
                IntPtr packetLib = ProgramDirectory.LoadLibrary(packetPath);
                IntPtr wpcapLib = ProgramDirectory.LoadLibrary(wpcapPath);

                if (packetLib == IntPtr.Zero || wpcapLib == IntPtr.Zero)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
