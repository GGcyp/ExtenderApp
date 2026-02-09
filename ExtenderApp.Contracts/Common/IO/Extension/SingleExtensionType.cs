
using System.Diagnostics.CodeAnalysis;

namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 唯一文件后缀拓展名
    /// </summary>
    internal struct SingleExtensionType : IEquatable<SingleExtensionType>
    {
        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string Extension { get; private set; }

        /// <summary>
        /// 获取或设置筛选器字符串，该字符串确定在文件对话框（如<see cref="Microsoft.Win32.OpenFileDialog"/>或<see cref="Microsoft.Win32.SaveFileDialog"/>）中显示哪些类型的文件。
        /// 筛选器字符串应包含筛选器的说明，后跟竖线(|)和筛选模式。多个筛选器说明和模式对还必须以竖线分隔。在一个筛选器模式中的多个扩展名必须用分号分隔。
        /// 例如: "图像文件(*.bmp, *.jpg)|*.bmp;*.jpg|所有文件(*.*)|*.*" 表示显示两组筛选器：一组是图像文件（.bmp和.jpg），另一组是所有文件。
        /// </summary>
        /// <value>
        /// 包含筛选器信息的<see cref="string"/>。如果为空或格式不正确，则可能不会按预期过滤文件类型。
        /// </value>
        /// <exception cref="System.ArgumentException">
        /// 如果提供的筛选器字符串不符合预期的格式，则可能会抛出此异常，尽管这通常取决于使用筛选器字符串的具体上下文（如<see cref="Microsoft.Win32.OpenFileDialog"/>的<see cref="Microsoft.Win32.FileDialog.Filter"/>属性）。
        /// 注意：并非所有使用筛选器字符串的API都会直接抛出<see cref="System.ArgumentException"/>；有些可能会静默地忽略无效格式。
        /// </exception>
        public string Filter => string.Format("*{0}", Extension);

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(Extension);

        public SingleExtensionType(string extension)
        {
            Extension = extension;
        }

        public bool Equals(SingleExtensionType obj)
        {
            if (IsEmpty && obj.IsEmpty)
                return true;

            if ((IsEmpty && !obj.IsEmpty) || (!IsEmpty && obj.IsEmpty))
                return false;

            return obj.Extension == Extension;
        }

        public static bool operator ==(SingleExtensionType left, SingleExtensionType right)
        {
            return left.Extension == right.Extension;
        }

        public static bool operator !=(SingleExtensionType left, SingleExtensionType right)
        {
            return !(left == right);
        }

        public static SingleExtensionType Empty => new SingleExtensionType(string.Empty);
    }
}
