using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件解析接口
    /// </summary>
    public interface IFileParserProvider
    {
        /// <summary>
        /// 提供解释器的文件扩展名
        /// </summary>
        FileExtensionType FileExtensionType { get; }

        /// <summary>
        /// 根据库名称获取文件解析器
        /// </summary>
        /// <param name="LibraryName">库名称，默认为null</param>
        /// <returns>返回文件解析器对象，若未找到则返回null</returns>
        IFileParser? GetParser(string LibraryName = null);
    }

    /// <summary>
    /// 文件解析接口
    /// </summary>
    /// <typeparam name="T">表示文件解析器的类型，该类型必须实现 <see cref="IFileParser"/> 接口。</typeparam>
    public interface IFileParserProvider<T> : IFileParserProvider where T : IFileParser
    {

    }
}
