
namespace MainApp.Common
{
    public interface IFileParser
    {
        /// <summary>
        /// 获取文件扩展类型
        /// </summary>
        FileExtensionType ExtensionType { get; }

        /// <summary>
        /// 处理文件
        /// </summary>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="processAction">文件处理完成后的回调函数</param>
        void ProcessFile(FileInfoData infoData, Action<object> processAction);
    }
}
