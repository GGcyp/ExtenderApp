using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using ExtenderApp.Common;
using ExtenderApp.Contracts;
using ExtenderApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.LAN
{
    internal class LanStartup : PluginEntityStartup
    {
        public override Type StartType => typeof(LANMainView);
        private const string NpcapUninstallerPath = @"C:\Program Files\Npcap\Uninstall.exe"; // 默认卸载程序路径

        public override void AddService(IServiceCollection services)
        {
            services.AddView<LANMainView, LANMainViewModel>();
            services.AddViewModel<LANMainViewModel>();
        }

        public override void ConfigureDetails(PluginDetails details)
        {
            //LoadNpcapDlls(Path.Combine(details.PluginFolderPath!, "npcapLibs"));
            SilentInstallNpcap(Path.Combine(details.PluginFolderPath!, "npcapApp", "npcap-1.85.exe"));
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

        /// <summary>
        /// 静默安装 Npcap（无界面，后台执行）
        /// </summary>
        private static void SilentInstallNpcap(string appPath)
        {
            try
            {
                // 配置进程启动参数（静默安装参数 /S）
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = appPath,
                    Arguments = "/S", // 核心静默参数
                    WindowStyle = ProcessWindowStyle.Hidden, // 隐藏命令行窗口
                    CreateNoWindow = true, // 不创建新窗口
                    UseShellExecute = true, // 必须为true才能获取管理员权限
                    Verb = "runas" // 强制以管理员身份启动（即使已管理员运行，此参数不冲突）
                };

                // 启动进程并等待完成
                using (var process = Process.Start(processStartInfo))
                {
                    process?.WaitForExit(); // 等待安装完成（约10-30秒，取决于系统）
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 静默卸载 Npcap（无界面，后台执行）
        /// </summary>
        private static void SilentUninstallNpcap()
        {
            // 配置进程启动参数（静默卸载参数 /S，可选 /no_kill=yes 不终止依赖进程）
            var processStartInfo = new ProcessStartInfo
            {
                FileName = NpcapUninstallerPath,
                Arguments = "/S", // 核心静默参数（如需不终止进程：Arguments = "/S /no_kill=yes"）
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = true,
                Verb = "runas"
            };

            // 启动进程并等待完成
            using (var process = Process.Start(processStartInfo))
            {
                process?.WaitForExit(); // 等待卸载完成（约5-15秒）
            }
        }

        /// <summary>
        /// 校验当前程序是否以管理员身份运行
        /// </summary>
        private static bool IsRunningAsAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}