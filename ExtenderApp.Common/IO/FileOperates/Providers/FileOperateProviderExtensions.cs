using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common
{
    /// <summary>
    /// FileOperateProviderExtensions 类的文档注释
    /// </summary>
    public static class FileOperateProviderExtensions
    {
        /// <summary>
        /// 通过提供的文件操作提供者和本地文件信息获取文件操作对象。
        /// </summary>
        /// <param name="provider">文件操作提供者接口。</param>
        /// <param name="info">本地文件信息。</param>
        /// <returns>返回文件操作对象。</returns>
        public static IFileOperate GetOperate(this IFileOperateProvider provider, LocalFileInfo info)
        {
            return provider.GetOperate(info.CreateReadWriteOperate());
        }
    }
}
