using System.Reflection;
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
        public LocalData? LocalData { get; set; }

        /// <summary>
        /// 获取或设置序列化方法的信息。
        /// </summary>
        public MethodInfo SerializeMethodInfo { get; set; }

        /// <summary>
        /// 初始化 LocalDataInfo 类的新实例
        /// </summary>
        /// <param name="filePath">本地文件的路径</param>
        /// <param name="localDataType">本地数据类型</param>
        public LocalDataInfo(string folderPath, string fileName, MethodInfo serializeMethodInfo) : this(new ExpectLocalFileInfo(folderPath, fileName), serializeMethodInfo)
        {

        }

        /// <summary>
        /// 初始化 LocalDataInfo 类的新实例
        /// </summary>
        /// <param name="localFileInfo">本地文件信息</param>
        /// <param name="localDataType">本地数据类型</param>
        public LocalDataInfo(ExpectLocalFileInfo fileInfo, MethodInfo serializeMethodInfo)
        {
            FileInfo = fileInfo;
            SerializeMethodInfo = serializeMethodInfo;
        }
    }
}
