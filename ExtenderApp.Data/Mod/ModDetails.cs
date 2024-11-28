using System.Runtime.Loader;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 模组详情类
    /// </summary>
    public sealed class ModDetails
    {
        private readonly ModeInfo modeInfo;

        /// <summary>
        /// 模组标题
        /// </summary>
        public string? Title => modeInfo.ModTitle;

        /// <summary>
        /// 模组描述
        /// </summary>
        public string? Description => modeInfo.ModDescription;

        /// <summary>
        /// 模组版本号
        /// </summary>
        public string? Version => modeInfo.ModVersion;

        /// <summary>
        /// 获取模式信息的版本信息。
        /// </summary>
        /// <returns>返回版本信息字符串，如果没有版本信息则返回null。</returns>
        public string? VersionInformation => modeInfo.ModVersionInformation;

        /// <summary>
        /// 模组主程序集名字，或在本文件夹下的地址
        /// </summary>
        public string? StartupDll => modeInfo.ModStartupDll;

        /// <summary>
        /// 获取或设置程序集的加载上下文。
        /// </summary>
        public AssemblyLoadContext? LoadContext { get; set; }

        /// <summary>
        /// 模组的文件位置
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 模组视图启动类类型
        /// </summary>
        public Type StartupType { get; set; }

        public ModDetails(ModeInfo modeInfo)
        {
            this.modeInfo = modeInfo;
        }
    }
}
