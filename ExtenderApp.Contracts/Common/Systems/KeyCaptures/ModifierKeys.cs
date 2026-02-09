
namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 表示键盘上的修饰键（如 Alt、Ctrl、Shift、Windows），可用于组合键检测。
    /// 支持位运算，可同时检测多个修饰键。
    /// </summary>
    public enum ModifierKeys : byte
    {
        /// <summary>
        /// 无修饰键。
        /// </summary>
        None = 0,

        /// <summary>
        /// Alt 键。
        /// </summary>
        Alt = 1,

        /// <summary>
        /// Control（Ctrl）键。
        /// </summary>
        Control = 2,

        /// <summary>
        /// Shift 键。
        /// </summary>
        Shift = 4,

        /// <summary>
        /// Windows（Win）键。
        /// </summary>
        Windows = 8
    }
}
