using ExtenderApp.Common.Error;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 提供文件操作扩展方法的静态类
    /// </summary>
    public static class FileOperateExtensions
    {
        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <typeparam name="TData">文件操作数据的类型，继承自FileOperateData</typeparam>
        /// <param name="operate">文件并发操作实例</param>
        /// <returns>文件的MD5值</returns>
        /// <exception cref="ArgumentNullException">如果operate为null，则抛出ArgumentNullException异常</exception>
        public static string GetFileMD5<TData>(this FileConcurrentOperate operate)
            where TData : FileOperateData
        {
            operate.ArgumentNull(nameof(operate));

            return MD5Handle.GetMD5Hash(operate.Data.FStream);
        }
    }
}
