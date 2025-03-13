

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件解析器存储接口
    /// </summary>
    public interface IFileParserStore
    {
        /// <summary>
        /// 获取二进制解析器
        /// </summary>
        /// <returns>二进制解析器</returns>
        public IBinaryParser BinaryParser { get; }

        /// <summary>
        /// 获取分隔符解析器
        /// </summary>
        /// <returns>分隔符解析器</returns>
        public ISplitterParser SplitterParser { get; }

        /// <summary>
        /// 获取JSON解析器
        /// </summary>
        /// <returns>JSON解析器</returns>
        public IJsonParser JsonParser { get; }
    }
}
