

namespace ExtenderApp.Data
{
    /// <summary>
    /// 定义文本数据的格式类型枚举
    /// </summary>
    public enum TextDataFormat
    {
        /// <summary>
        /// 纯文本格式（ANSI编码）
        /// </summary>
        Text = 0,

        /// <summary>
        /// Unicode文本格式
        /// </summary>
        UnicodeText = 1,

        /// <summary>
        /// 富文本格式（Rich Text Format）
        /// </summary>
        Rtf = 2,

        /// <summary>
        /// HTML格式
        /// </summary>
        Html = 3,

        /// <summary>
        /// 逗号分隔值格式（CSV）
        /// </summary>
        CommaSeparatedValue = 4,

        /// <summary>
        /// XAML格式（可扩展应用程序标记语言）
        /// </summary>
        Xaml = 5
    }
}
