
namespace ExtenderApp.Common
{
    /// <summary>
    /// 文件解析器接口
    /// </summary>
    public interface IFileParser
    {
        /// <summary>
        /// 获取解析器的库名称
        /// </summary>
        string LibraryName { get; }

        /// <summary>
        /// 解析文件
        /// </summary>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="callback">处理动作</param>
        /// <param name="setting">文件设置，默认为默认值</param>
        void Parser(FileInfoData infoData, Action<object?> callback, object? options = null);

        /// <summary>
        /// 解析器方法
        /// </summary>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="obj">待解析的对象</param>
        /// <param name="callback">解析完成后的回调函数</param>
        /// <param name="setting">可选的设置参数，默认为null</param>
        void Parser(FileInfoData infoData, object obj, Action<object?> callback, object? options = null);
    }
}
