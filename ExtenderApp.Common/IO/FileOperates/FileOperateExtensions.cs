using System.IO.MemoryMappedFiles;
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
        /// <typeparam name="TData">泛型参数，继承自 FileOperateData 类</typeparam>
        /// <param name="operate">当前对象，实现了 IConcurrentOperate 接口，泛型参数为 MemoryMappedViewAccessor 和 TData</param>
        /// <returns>返回文件的 MD5 值</returns>
        /// <exception cref="ArgumentNullException">如果 parser 参数为空，则抛出此异常</exception>
        public static string GetFileMD5<TData>(this IConcurrentOperate<MemoryMappedViewAccessor, TData> operate)
            where TData : FileOperateData
        {
            operate.ArgumentNull(nameof(operate));

            return MD5Handle.GetMD5Hash(operate.Data.FileStream);
        }
    }
}
