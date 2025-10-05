

namespace ExtenderApp.Data
{
    /// <summary>
    /// 文件操作信息对象扩展类
    /// </summary>
    public static class FileOperateInfoExtensions
    {
        #region Move

        /// <summary>
        /// 将当前文件移动到指定目标文件对象。
        /// </summary>
        /// <param name="operate">要移动的文件操作信息对象。</param>
        /// <param name="targetOperate">目标文件操作对象。</param>
        /// <exception cref="ArgumentNullException">如果目标文件操作对象为空。</exception>
        /// <exception cref="InvalidOperationException">如果目标文件已存在。</exception>
        public static void Move(this FileOperateInfo operate, FileOperateInfo targetOperate)
        {
            if (targetOperate.LocalFileInfo.IsEmpty)
            {
                throw new ArgumentNullException(nameof(targetOperate));
            }

            //if (targetOperate.LocalFileInfo.Exists)
            //{
            //    throw new InvalidOperationException("目标文件已存在");
            //}

            operate.Move(targetOperate.LocalFileInfo.FullPath);
        }


        /// <summary>
        /// 将当前文件移动到指定本地文件信息对象。
        /// </summary>
        /// <param name="operate">要移动的文件操作信息对象。</param>
        /// <param name="localFileInfo">目标本地文件信息对象。</param>
        /// <exception cref="ArgumentNullException">如果本地文件信息对象为空。</exception>
        /// <exception cref="InvalidOperationException">如果目标文件已存在。</exception>
        public static void Move(this FileOperateInfo operate, LocalFileInfo localFileInfo)
        {
            if (localFileInfo.IsEmpty)
            {
                throw new ArgumentNullException(nameof(localFileInfo));
            }

            //if (localFileInfo.Exists)
            //{
            //    throw new InvalidOperationException("目标文件已存在");
            //}

            operate.Move(localFileInfo.FullPath);
        }


        /// <summary>
        /// 将当前文件移动到指定路径。
        /// </summary>
        /// <param name="operate">要移动的文件操作信息对象。</param>
        /// <param name="targetPath">目标路径。</param>
        /// <exception cref="ArgumentNullException">如果目标路径为空。</exception>
        public static void Move(this FileOperateInfo operate, string targetPath)
        {
            if (string.IsNullOrEmpty(targetPath))
            {
                throw new ArgumentNullException(nameof(targetPath));
            }

            if (System.IO.File.Exists(targetPath))
            {
                throw new InvalidOperationException();
            }

            System.IO.File.Move(operate.LocalFileInfo.FullPath, targetPath);
        }

        #endregion

        #region Delete

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="operate">文件操作信息对象</param>
        /// <exception cref="ArgumentNullException">如果operate.LocalFileInfo为空，则抛出ArgumentNullException异常</exception>
        public static void Delete(this FileOperateInfo operate)
        {
            if (operate.LocalFileInfo.IsEmpty)
            {
                throw new ArgumentNullException(nameof(operate.LocalFileInfo));
            }

            System.IO.File.Delete(operate.LocalFileInfo.FullPath);
        }

        #endregion

        #region Check

        /// <summary>
        /// 判断文件操作信息是否为只读模式。
        /// </summary>
        /// <param name="operate">文件操作信息对象。</param>
        /// <returns>如果文件操作信息为只读模式，则返回 true；否则返回 false。</returns>
        public static bool IsRead(this FileOperateInfo operate)
        {
            if (operate.IsEmpty)
            {
                return false;
            }

            return operate.FileAccess.ToFileAccess() == (FileAccess.ReadWrite | FileAccess.Read);
        }

        /// <summary>
        /// 判断文件操作信息是否为只写模式。
        /// </summary>
        /// <param name="operate">文件操作信息对象。</param>
        /// <returns>如果文件操作信息为只写模式，则返回 true；否则返回 false。</returns>
        public static bool IsWrite(this FileOperateInfo operate)
        {
            if (operate.IsEmpty)
            {
                return false;
            }

            return operate.FileAccess.ToFileAccess() == (FileAccess.ReadWrite | FileAccess.Write);
        }

        /// <summary>
        /// 判断文件操作信息是否为读写模式。
        /// </summary>
        /// <param name="operate">文件操作信息对象。</param>
        /// <returns>如果文件操作信息为读写模式，则返回 true；否则返回 false。</returns>
        public static bool IsReadWrite(this FileOperateInfo operate)
        {
            if (operate.IsEmpty)
            {
                return false;
            }

            return operate.FileAccess.ToFileAccess() == FileAccess.ReadWrite;
        }

        #endregion
    }
}
