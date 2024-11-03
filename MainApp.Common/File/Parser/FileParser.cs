namespace MainApp.Common.File
{
    /// <summary>
    /// 文件解析器基类
    /// </summary>
    /// <remarks>
    /// 该类为文件解析器的基类，定义了文件解析器的基本接口和行为。
    /// </remarks>
    internal abstract class FileParser : IFileParser
    {
        public abstract FileExtensionType ExtensionType { get; }

        /// <summary>
        /// 获取文件提供者拓展对象。
        /// 仅在需要使用时在构造函数内获取、调用。
        /// </summary>
        protected readonly IFileAccessProvider _fileAccessProvider;

        public FileParser(IFileAccessProvider provider)
        {
            _fileAccessProvider = provider;
        }

        public void ProcessFile(FileInfoData infoData, Action<object> processAction)
        {
            switch(infoData.FileAccess)
            {
                case FileAccess.Read:
                    Read(infoData, processAction);
                    break;
                case FileAccess.Write:
                    Write(infoData, processAction);
                    break;
                default:
                    throw new ArgumentException(nameof(FileParser));
            }
        }

        /// <summary>
        /// 读取文件的抽象方法
        /// </summary>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="processAction">读取完成后的回调函数</param>
        protected abstract void Read(FileInfoData infoData, Action<object> processAction);

        /// <summary>
        /// 写入文件的抽象方法
        /// </summary>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="processAction">写入完成后的回调函数</param>
        protected abstract void Write(FileInfoData infoData, Action<object> processAction);
    }
}
