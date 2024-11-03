namespace MainApp.Common.File
{
    internal class FileAccessProvider : IFileAccessProvider
    {
        private readonly FileParserStore _store;

        public FileAccessProvider(FileParserStore store)
        {
            _store = store;
        }

        public IFileParser? GetParser(FileExtensionType extension)
        {
            return _store.First(e => e.ExtensionType == extension);
        }
    }
}
