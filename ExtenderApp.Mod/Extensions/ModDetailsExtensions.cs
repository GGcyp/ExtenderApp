using AppHost.Builder;
using ExtenderApp.Mods;
using System;

namespace ExtenderApp.Mod
{
    /// <summary>
    /// 模组信息扩展
    /// </summary>
    public static class ModDetailsExtensions
    {
        /// <summary>
        /// 添加模组程序集
        /// </summary>
        /// <param name="info">模组信息</param>
        /// <param name="builder">主机程序</param>
        /// <returns>模组加载类</returns>
        public static ModDetails AddModAssembly(this ModDetails modDetails, IHostApplicationBuilder builder, string modFolderPath)
        {
            if (string.IsNullOrEmpty(modDetails.StartupDll)) return modDetails;

            string modStartupDllPath = Path.Combine(modFolderPath, modDetails.StartupDll);
            builder.FindStarupForLoadAssemblyFile(modStartupDllPath, out ModEntityStartup startup);
            modDetails.StartupType = startup.StartType;
            return modDetails;
        }
    }
}
