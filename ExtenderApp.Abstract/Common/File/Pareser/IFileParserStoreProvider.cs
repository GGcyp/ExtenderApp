using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件解释器厂库提供者
    /// </summary>
    public interface IFileParserStoreProvider
    {
        /// <summary>
        /// 添加文件解释器到指定文件厂库中
        /// </summary>
        /// <param name="fileType">目标文件类型</param>
        /// <param name="parser">添加的解释器</param>
        void AddParser(FileExtensionType fileType, IFileParser parser);
    }
}
