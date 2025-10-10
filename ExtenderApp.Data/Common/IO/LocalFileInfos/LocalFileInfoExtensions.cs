namespace ExtenderApp.Data
{
    /// <summary>
    /// LocalFileInfo 扩展类，提供 LocalFileInfo 的扩展方法。
    /// </summary>
    public static class LocalFileInfoExtensions
    {
        #region Move

        /// <summary>
        /// 将文件移动到指定位置
        /// </summary>
        /// <param name="info">要移动的文件信息</param>
        /// <param name="targetOperate">目标文件操作信息对象</param>
        /// <exception cref="ArgumentNullException">当目标文件信息为空时抛出</exception>
        /// <exception cref="InvalidOperationException">当目标文件已存在时抛出</exception>
        public static void Move(this LocalFileInfo info, FileOperateInfo targetOperate)
        {
            if (targetOperate.LocalFileInfo.IsEmpty)
            {
                throw new ArgumentNullException(nameof(targetOperate));
            }
            if (targetOperate.LocalFileInfo.Exists)
            {
                throw new InvalidOperationException("目标文件已存在");
            }
            info.Move(targetOperate.LocalFileInfo.FullPath);
        }

        /// <summary>
        /// 将当前 LocalFileInfo 对象移动到指定的目标 LocalFileInfo 对象的位置。
        /// </summary>
        /// <param name="info">当前 LocalFileInfo 对象。</param>
        /// <param name="targetInfo">目标 LocalFileInfo 对象。</param>
        /// <exception cref="InvalidOperationException">当目标文件已存在时抛出。</exception>
        public static void Move(this LocalFileInfo info, LocalFileInfo targetInfo)
        {
            if (targetInfo.IsEmpty || targetInfo.Exists)
            {
                throw new InvalidOperationException("目标文件已存在");
            }

            System.IO.File.Move(info.FullPath, targetInfo.FullPath);
        }

        /// <summary>
        /// 将当前 LocalFileInfo 对象移动到指定的目标路径。
        /// </summary>
        /// <param name="info">当前 LocalFileInfo 对象。</param>
        /// <param name="targetPath">目标路径。</param>
        /// <exception cref="ArgumentNullException">当目标路径为空时抛出。</exception>
        public static void Move(this LocalFileInfo info, string targetPath)
        {
            if (string.IsNullOrEmpty(targetPath))
            {
                throw new ArgumentNullException(nameof(targetPath));
            }

            System.IO.File.Move(info.FullPath, targetPath);
        }

        #endregion

        #region Delete

        /// <summary>
        /// 删除本地文件。
        /// </summary>
        /// <param name="info">包含文件信息的 LocalFileInfo 对象。</param>
        public static void Delete(this LocalFileInfo info)
        {
            if (info.IsEmpty)
                throw new ArgumentNullException(nameof(info));

            //System.IO.File.Delete(info.FullPath);
            info.FileInfo.Delete();
        }

        #endregion
    }
}
