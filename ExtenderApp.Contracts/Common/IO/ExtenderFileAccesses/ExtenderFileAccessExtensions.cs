using System.IO.MemoryMappedFiles;

namespace ExtenderApp.Contracts
{
    /// <summary>
    /// <see cref="ExtenderFileAccess"/> 与 BCL 访问枚举之间的互转与能力判断扩展。
    /// </summary>
    public static class ExtenderFileAccessExtensions
    {
        /// <summary>
        /// 将 <see cref="ExtenderFileAccess"/> 转换为 <see cref="FileAccess"/>。
        /// </summary>
        /// <param name="extenderFileAccess">自定义访问模式。</param>
        /// <returns>对应的 <see cref="FileAccess"/> 值。</returns>
        /// <exception cref="ArgumentOutOfRangeException">传入的访问模式不受支持。</exception>
        /// <remarks>
        /// CopyOnWrite 将映射为 <see cref="FileAccess.Write"/>；ReadExecute 映射为 <see cref="FileAccess.Read"/>。
        /// </remarks>
        public static FileAccess ToFileAccess(this ExtenderFileAccess extenderFileAccess)
        {
            return extenderFileAccess switch
            {
                ExtenderFileAccess.Read => FileAccess.Read,
                ExtenderFileAccess.Write => FileAccess.Write,
                ExtenderFileAccess.ReadWrite => FileAccess.ReadWrite,
                ExtenderFileAccess.CopyOnWrite => FileAccess.Write,
                ExtenderFileAccess.ReadExecute => FileAccess.Read,
                ExtenderFileAccess.ReadWriteExecute => FileAccess.ReadWrite,
                _ => throw new ArgumentOutOfRangeException(nameof(extenderFileAccess), extenderFileAccess, null)
            };
        }

        /// <summary>
        /// 将 <see cref="FileAccess"/> 转换为 <see cref="ExtenderFileAccess"/>。
        /// </summary>
        /// <param name="fileAccess">BCL 文件访问模式。</param>
        /// <returns>对应的 <see cref="ExtenderFileAccess"/> 值。</returns>
        /// <exception cref="ArgumentOutOfRangeException">传入的访问模式不受支持。</exception>
        public static ExtenderFileAccess FromFileAccess(this FileAccess fileAccess)
        {
            return fileAccess switch
            {
                FileAccess.Read => ExtenderFileAccess.Read,
                FileAccess.Write => ExtenderFileAccess.Write,
                FileAccess.ReadWrite => ExtenderFileAccess.ReadWrite,
                _ => throw new ArgumentOutOfRangeException(nameof(fileAccess), fileAccess, null)
            };
        }

        /// <summary>
        /// 将 <see cref="ExtenderFileAccess"/> 转换为 <see cref="MemoryMappedFileAccess"/>。
        /// </summary>
        /// <param name="extenderFileAccess">自定义访问模式。</param>
        /// <returns>对应的 <see cref="MemoryMappedFileAccess"/> 值。</returns>
        /// <exception cref="ArgumentOutOfRangeException">传入的访问模式不受支持。</exception>
        /// <remarks>
        /// 注意：.NET 的 CreateFromFile 级别不允许仅 <see cref="MemoryMappedFileAccess.Write"/> 的映射；
        /// 但此处仍提供到枚举的转换，具体可用性取决于调用点（如创建视图时也不接受 Write-only）。
        /// </remarks>
        public static MemoryMappedFileAccess ToMemoryMappedFileAccess(this ExtenderFileAccess extenderFileAccess)
        {
            return extenderFileAccess switch
            {
                ExtenderFileAccess.Read => MemoryMappedFileAccess.Read,
                ExtenderFileAccess.Write => MemoryMappedFileAccess.Write,
                ExtenderFileAccess.ReadWrite => MemoryMappedFileAccess.ReadWrite,
                ExtenderFileAccess.CopyOnWrite => MemoryMappedFileAccess.CopyOnWrite,
                ExtenderFileAccess.ReadExecute => MemoryMappedFileAccess.ReadExecute,
                ExtenderFileAccess.ReadWriteExecute => MemoryMappedFileAccess.ReadWriteExecute,
                _ => throw new ArgumentOutOfRangeException(nameof(extenderFileAccess), extenderFileAccess, null)
            };
        }

        /// <summary>
        /// 将 <see cref="MemoryMappedFileAccess"/> 转换为 <see cref="ExtenderFileAccess"/>。
        /// </summary>
        /// <param name="memoryMappedFileAccess">内存映射文件访问模式。</param>
        /// <returns>对应的 <see cref="ExtenderFileAccess"/> 值。</returns>
        /// <exception cref="ArgumentOutOfRangeException">传入的访问模式不受支持。</exception>
        public static ExtenderFileAccess FromMemoryMappedFileAccess(this MemoryMappedFileAccess memoryMappedFileAccess)
        {
            return memoryMappedFileAccess switch
            {
                MemoryMappedFileAccess.Read => ExtenderFileAccess.Read,
                MemoryMappedFileAccess.Write => ExtenderFileAccess.Write,
                MemoryMappedFileAccess.ReadWrite => ExtenderFileAccess.ReadWrite,
                MemoryMappedFileAccess.CopyOnWrite => ExtenderFileAccess.CopyOnWrite,
                MemoryMappedFileAccess.ReadExecute => ExtenderFileAccess.ReadExecute,
                MemoryMappedFileAccess.ReadWriteExecute => ExtenderFileAccess.ReadWriteExecute,
                _ => throw new ArgumentOutOfRangeException(nameof(memoryMappedFileAccess), memoryMappedFileAccess, null)
            };
        }

        /// <summary>
        /// 判断是否具备“可写能力”（包括写时复制与读写执行）。
        /// </summary>
        /// <param name="extenderFileAccess">自定义访问模式。</param>
        /// <returns>true 表示可写；否则为 false。</returns>
        public static bool IsWritable(this ExtenderFileAccess extenderFileAccess)
        {
            return extenderFileAccess is ExtenderFileAccess.Write or ExtenderFileAccess.ReadWrite or ExtenderFileAccess.CopyOnWrite or ExtenderFileAccess.ReadWriteExecute;
        }

        /// <summary>
        /// 判断是否具备“可读能力”（包括写时复制与可执行相关组合）。
        /// </summary>
        /// <param name="extenderFileAccess">自定义访问模式。</param>
        /// <returns>true 表示可读；否则为 false。</returns>
        public static bool IsReadable(this ExtenderFileAccess extenderFileAccess)
        {
            return extenderFileAccess is ExtenderFileAccess.Read or ExtenderFileAccess.ReadWrite or ExtenderFileAccess.CopyOnWrite or ExtenderFileAccess.ReadExecute or ExtenderFileAccess.ReadWriteExecute;
        }
    }
}
