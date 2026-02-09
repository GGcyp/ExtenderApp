using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件操作提供者的抽象契约：负责创建、复用与释放 <see cref="IFileOperate"/> 实例。
    /// </summary>
    /// <remarks>
    /// 约定与建议：
    /// - 生命周期：调用方获取到 <see cref="IFileOperate"/> 后，应在使用完毕时调用 <see cref="ReleaseOperate(IFileOperate)"/> 或对应的释放方法，
    ///   以便实现有机会进行对象池复用或资源回收；如实现不支持复用，释放应等价于调用 IFileOperate.Dispose()。
    /// - 选择策略：若未显式指定 <see cref="FileOperateType"/>，实现可根据默认策略或运行环境（如优先 FileStream）选择具体实现；
    ///   若显式指定但不受支持，推荐抛出 <see cref="NotSupportedException"/>。
    /// - 线程安全：若提供者在内部复用/缓存实例，应保证并发访问安全或在文档中明确并发模型。
    /// </remarks>
    public interface IFileOperateProvider
    {
        /// <summary>
        /// 根据文件操作信息获取文件操作对象。
        /// </summary>
        /// <param name="info">文件操作信息（包含路径、模式与访问权限）。</param>
        /// <returns>文件操作对象实例。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="info"/> 无效或为空。</exception>
        /// <exception cref="FileNotFoundException">当需要打开已存在文件但目标不存在（视实现而定）。</exception>
        IFileOperate GetOperate(FileOperateInfo info);

        /// <summary>
        /// 根据文件操作信息与指定策略获取文件操作对象。
        /// </summary>
        /// <param name="info">文件操作信息（包含路径、模式与访问权限）。</param>
        /// <param name="type">文件操作实现策略（如 <see cref="FileOperateType.FileStream"/>、<see cref="FileOperateType.MemoryMapped"/>）。</param>
        /// <returns>文件操作对象实例。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="info"/> 无效或为空。</exception>
        /// <exception cref="NotSupportedException">当 <paramref name="type"/> 在当前平台或实现中不受支持。</exception>
        IFileOperate GetOperate(FileOperateInfo info, FileOperateType type);

        /// <summary>
        /// 释放与指定 <paramref name="info"/> 相关联的文件操作对象（若实现存在按信息缓存的实例）。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <remarks>
        /// 适用于基于 <see cref="FileOperateInfo"/> 索引到已创建实例的实现；若无缓存则为无操作（No-Op）。
        /// </remarks>
        void ReleaseOperate(FileOperateInfo info);

        /// <summary>
        /// 释放指定的文件操作对象实例。
        /// </summary>
        /// <param name="fileOperate">文件操作接口实例。</param>
        /// <remarks>
        /// 实现应确保释放底层句柄/映射/缓冲区；若实现使用对象池，此处应将实例归还池中。
        /// </remarks>
        void ReleaseOperate(IFileOperate fileOperate);

        /// <summary>
        /// 释放与指定 <paramref name="info"/> 相关联的文件操作对象，并返回被释放/回收的实例（若存在）。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <param name="fileOperate">返回被释放/回收的文件操作对象；若未找到则为 null。</param>
        /// <remarks>
        /// 便于调用方根据返回实例做进一步处理（如诊断或延迟销毁）；未命中缓存时返回 null。
        /// </remarks>
        void ReleaseOperate(FileOperateInfo info, out IFileOperate? fileOperate);
    }
}
