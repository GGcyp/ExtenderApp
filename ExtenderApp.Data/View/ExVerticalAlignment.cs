

namespace ExtenderApp.Data
{
    /// <summary>
    /// 定义垂直对齐方式的枚举，继承自byte类型
    /// </summary>
    public enum ExVerticalAlignment : byte
    {
        /// <summary>
        /// 顶部对齐
        /// </summary>
        Top = 0,

        /// <summary>
        /// 居中对齐
        /// </summary>
        Center = 1,

        /// <summary>
        /// 底部对齐
        /// </summary>
        Bottom = 2,

        /// <summary>
        /// 拉伸对齐（填满可用空间）
        /// </summary>
        Stretch = 3
    }
}
