using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 提供文件操作扩展方法的静态类
    /// </summary>
    public static class FileOperateExtensions
    {
        /// <summary>
        /// 获取文件的MD5值
        /// </summary>
        /// <param name="operate">IConcurrentOperate类型的实例</param>
        /// <returns>返回文件的MD5值</returns>
        /// <exception cref="ArgumentNullException">如果operate为null，则抛出ArgumentNullException异常</exception>
        /// <exception cref="Exception">如果operate不是FileConcurrentOperate类型，则抛出Exception异常</exception>
        public static string GetFileMD5(this IConcurrentOperate operate)
        {
            if (operate == null)
                ErrorUtil.ArgumentNull(nameof(operate), "文件操作数据不能为空");

            if (operate is not FileConcurrentOperate fileOperate)
            {
                throw new Exception("操作数据不是文件操作类型");
            }

            return fileOperate.GetFileMD5();
        }

        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="operate">文件并发操作实例</param>
        /// <returns>文件的MD5值</returns>
        /// <exception cref="ArgumentNullException">如果operate为null，则抛出ArgumentNullException异常</exception>
        public static string GetFileMD5(this FileConcurrentOperate operate)
        {
            operate.ArgumentNull(nameof(operate));

            return MD5Handle.GetMD5Hash(operate.Data.FStream);
        }
    }
}
