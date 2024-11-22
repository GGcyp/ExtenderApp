namespace ExtenderApp.Common
{
    /// <summary>
    /// 文件解析器选项
    /// </summary>
    public struct FileParserOptions
    {
        public FileParserOptions(string libraryName = null, object setting = null)
        {
            LibraryName = libraryName;
            Options = setting;
            IsDefault = string.IsNullOrEmpty(LibraryName) && setting == null;
        }

        /// <summary>
        /// 库名称
        /// </summary>
        public string LibraryName { get; }

        /// <summary>
        /// 设置
        /// </summary>
        public object Options { get; }

        /// <summary>
        /// 是否为默认设置
        /// </summary>
        public bool IsDefault { get; }
    }
}
