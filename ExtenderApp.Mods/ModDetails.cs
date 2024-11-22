using ExtenderApp.Mods;

namespace ExtenderApp.Mod
{
    /// <summary>
    /// 模组详情类
    /// </summary>
    public sealed class ModDetails
    {
        public ModDetails(ModeInfo modeInfo)
        {
            this.modeInfo = modeInfo;
        }

        private ModeInfo modeInfo;

        /// <summary>
        /// 模组标题
        /// </summary>
        public string Title => modeInfo.ModTitle;

        /// <summary>
        /// 模组描述
        /// </summary>
        public string Description => modeInfo.ModDescription;

        /// <summary>
        /// 模组主程序集地址
        /// </summary>
        public string? ModPath { get; set; }

        /// <summary>
        /// 模组窗口类型
        /// </summary>
        public Type? viewType { get; set; }
    }
}
