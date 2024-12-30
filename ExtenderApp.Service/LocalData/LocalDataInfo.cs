using ExtenderApp.Data;

namespace ExtenderApp.Service
{
    internal class LocalDataInfo
    {
        public LocalFileInfo LocalFileInfo { get; }

        public LocalData? LocalData { get; set; }

        public LocalDataInfo(string filePath) : this(new LocalFileInfo(filePath))
        {

        }

        public LocalDataInfo(LocalFileInfo localFileInfo)
        {
            LocalFileInfo = localFileInfo;
        }
    }
}
