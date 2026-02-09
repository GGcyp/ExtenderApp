

namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 定义水平对齐方式的枚举
    /// </summary>
    public enum ExHorizontalAlignment : byte
    {
        /// <summary>
        /// 左对齐
        /// </summary>
        Left = 0,
        /// <summary>
        /// 居中对齐
        /// </summary>
        Center = 1,
        /// <summary>
        /// 右对齐
        /// </summary>
        Right = 2,
        /// <summary>
        /// 拉伸对齐（填满可用空间）
        /// </summary>
        Stretch = 3
    }
}
