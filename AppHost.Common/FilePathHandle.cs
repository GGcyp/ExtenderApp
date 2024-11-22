
namespace AppHost.Common
{
    /// <summary>
    /// 文件路径处理类。
    /// </summary>
    public static class FilePathHandle
    {
        /// <summary>
        /// 递归获取指定文件夹下所有文件路径，并添加到给定列表中。
        /// </summary>
        /// <param name="folderPath">要搜索的文件夹路径。</param>
        /// <param name="files">用于存储搜索结果的字符串列表，按引用传递。</param>
        /// <param name="searchPattern">搜索模式，默认为"*.*"，表示搜索所有文件。</param>
        /// <exception cref="ArgumentNullException">如果传入的<paramref name="files"/>为null，则抛出此异常。</exception>
        public static void GetAllFiles(string folderPath, ref List<string> files, string searchPattern = "*.*")
        {
            try
            {
                // 获取文件夹中直接的文件（不包括子文件夹）
                files.AddRange(Directory.GetFiles(folderPath, searchPattern));

                // 递归遍历子文件夹
                foreach (string subDir in Directory.GetDirectories(folderPath))
                {
                    GetAllFiles(subDir, ref files, searchPattern);// 递归调用
                }
            }
            catch (Exception ex)
            {
                // 异常处理，例如记录日志
                new IOException("An error occurred: " + ex.Message);
            }
        }
    }
}
