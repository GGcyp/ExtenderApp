using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.IO.FileOperates
{
    /// <summary>
    /// 文件操作工厂：根据给定的 <see cref="FileOperateType"/> 创建对应的 <see cref="IFileOperate"/> 实例。
    /// </summary>
    /// <remarks>
    /// 设计说明：
    /// - 工厂本身无状态，线程安全。<br/>
    /// - 当新增文件操作类型时，在 <see cref="Create(FileOperateType, FileOperateInfo)"/> 的 switch 中扩展分支。<br/>
    /// - 默认分支回退到 <see cref="FileStreamFileOperate"/>，以获得通用的文件流读写能力。
    /// </remarks>
    internal static class FileOperateFactory
    {
        /// <summary>
        /// 根据指定类型与操作上下文创建文件操作对象。
        /// </summary>
        /// <param name="type">文件操作类型，例如 <see cref="FileOperateType.MemoryMapped"/>。</param>
        /// <param name="info">文件操作上下文，包含路径、模式与访问权限等信息。</param>
        /// <returns>
        /// 返回与 <paramref name="type"/> 对应的 <see cref="IFileOperate"/> 实例；
        /// 当类型未匹配时回退为 <see cref="FileStreamFileOperate"/>。
        /// </returns>
        public static IFileOperate Create(FileOperateType type, FileOperateInfo info)
        {
            return type switch
            {
                FileOperateType.MemoryMapped => new MemoryMappedFileOperate(info),
                FileOperateType.ConcurrentFileStream => new ConcurrentFileOperate(info),
                _ => new FileStreamFileOperate(info),
            };
        }
    }
}
