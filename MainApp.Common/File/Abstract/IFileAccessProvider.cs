namespace MainApp.Common
{
    public interface IFileAccessProvider
    {
        /// <summary>
        /// 根据文件扩展名获取文件解析器
        /// </summary>
        /// <param name="extension">文件扩展名类型</param>
        /// <returns>与指定文件扩展名类型对应的文件解析器，若未找到则返回null</returns>
        IFileParser? GetParser(FileExtensionType extension);
    }
}
