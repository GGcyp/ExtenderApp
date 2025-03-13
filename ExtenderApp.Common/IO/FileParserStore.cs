using ExtenderApp.Abstract;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件解析器存储类，实现了 IFileParserStore 接口
    /// </summary>
    internal class FileParserStore : IFileParserStore
    {
        /// <summary>
        /// 获取二进制解析器
        /// </summary>
        public IBinaryParser BinaryParser { get; }

        /// <summary>
        /// 获取分隔符解析器
        /// </summary>
        public ISplitterParser SplitterParser { get; }

        /// <summary>
        /// 获取 JSON 解析器
        /// </summary>
        public IJsonParser JsonParser { get; }

        /// <summary>
        /// 初始化 FileParserStore 类的新实例
        /// </summary>
        /// <param name="binaryParser">二进制解析器</param>
        /// <param name="splitterParser">分隔符解析器</param>
        /// <param name="jsonParser">JSON 解析器</param>
        public FileParserStore(IBinaryParser binaryParser, ISplitterParser splitterParser, IJsonParser jsonParser)
        {
            BinaryParser = binaryParser;
            SplitterParser = splitterParser;
            JsonParser = jsonParser;
        }
    }
}
