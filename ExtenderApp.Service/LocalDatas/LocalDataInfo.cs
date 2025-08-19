using ExtenderApp.Data;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 本地数据信息的类
    /// </summary>
    internal class LocalDataInfo
    {
        /// <summary>
        /// 获取本地文件信息
        /// </summary>
        public ExpectLocalFileInfo FileInfo { get; }

        /// <summary>
        /// 获取或设置本地数据
        /// </summary>
        public LocalData LocalData { get; set; }

        public LocalDataInfo(string folderPath, string fileName, LocalData localData) : this(new ExpectLocalFileInfo(folderPath, fileName), localData)
        {

        }

        public LocalDataInfo(ExpectLocalFileInfo fileInfo, LocalData localData)
        {
            FileInfo = fileInfo;
            LocalData = localData;
        }

        public void Save()
        {
            LocalData.SaveData(FileInfo);
        }
    }
}
