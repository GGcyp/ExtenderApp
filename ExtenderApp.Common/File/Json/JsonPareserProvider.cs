using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.File
{
    /// <summary>
    /// JsonPareserProvider 类是一个内部类，继承自 FileParserProvider<IJsonParser> 并实现了 IJsonPareserProvider 接口。
    /// 该类用于提供 JSON 解析器。
    /// </summary>
    internal class JsonPareserProvider : FileParserProvider<IJsonParser>, IJsonPareserProvider
    {
        public override FileExtensionType FileExtensionType => FileExtensionType.Json;

        protected override string DefaultLibraryName => LibrarySetting.MICROSOFT_LIBRARY;

        public JsonPareserProvider(JsonParserStore store) : base(store)
        {
        }
    }
}
