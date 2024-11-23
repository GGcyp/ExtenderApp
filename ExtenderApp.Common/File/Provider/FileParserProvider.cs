using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.File
{
    /// <summary>
    /// 文件解析器提供者类
    /// </summary>
    /// <typeparam name="T">文件解析器类型，必须实现IFileParser接口</typeparam>
    internal abstract class FileParserProvider<T> : IFileParserProvider<T> where T : class, IFileParser
    {
        protected abstract string DefaultLibraryName { get; }

        public abstract FileExtensionType FileExtensionType {  get; }

        /// <summary>
        /// 存储文件解析器列表
        /// </summary>
        /// <remarks>因为不会有特别多的解析器，所以直接使用列表</remarks>
        protected readonly FileParserStore<T> _store;

        public FileParserProvider(FileParserStore<T> store)
        {
            _store = store;
        }

        public IFileParser? GetParser(string libraryName = null)
        {
            //如果没有默认解析库或者解析库不
            if (string.IsNullOrEmpty(libraryName))
                libraryName = DefaultLibraryName;

            if (string.IsNullOrEmpty(libraryName))
                libraryName = LibrarySetting.MICROSOFT_LIBRARY;

            for (int i = 0; i < _store.Count; i++)
            {
                var item = _store[i];
                if (item.LibraryName == libraryName)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
