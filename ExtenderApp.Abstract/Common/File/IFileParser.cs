using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件解析器接口
    /// </summary>
    public interface IFileParser
    {
        /// <summary>
        /// 获取文件扩展类型
        /// </summary>
        /// <returns>文件扩展类型</returns>
        FileExtensionType ExtensionTypeType { get; }
    }
}
