using ExtenderApp.Mods;

namespace ExtenderApp.Mod
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
        public string Title => modeInfo.ModTitle;

        /// <summary>
        /// 模组描述
        /// </summary>
        public string Description => modeInfo.ModDescription;

        /// <summary>
        /// 模组版本号
        /// </summary>
        public string Version => modeInfo.ModVersion;

        /// <summary>
        /// 模组主程序集名字，或在本文件夹下的地址
        /// </summary>
        public string StartupDll => modeInfo.ModStartupDll;

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
