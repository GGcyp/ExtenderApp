

namespace ExtenderApp.Data
{
    /// <summary>
    /// 模组信息
    /// </summary>
    public struct ModeInfo
    {
        /// <summary>
        /// 模组标题
        /// </summary>
        public string? ModTitle { get; set; }

        /// <summary>
        /// 模组描述
        /// </summary>
        public string? ModDescription { get; set; }

        /// <summary>
        /// 模组版本号
        /// </summary>
        public string? ModVersion { get; set; }

        /// <summary>
        /// 获取或设置模块的版本信息。
        /// </summary>
        public string? ModVersionInformation {  get; set; }

        /// <summary>
        /// 模组主程序集名字，或在本文件夹下的地址
        /// </summary>
        public string? ModStartupDll { get; set; }
    }
}
