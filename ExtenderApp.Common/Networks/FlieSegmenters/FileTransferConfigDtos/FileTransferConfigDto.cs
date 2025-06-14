

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 文件传输配置结构体
    /// </summary>
    public struct FileTransferConfigDto
    {
        /// <summary>
        /// 链接器数量
        /// </summary>
        public int LinkerCount { get; set; }

        public FileTransferConfigDto(int linkerCount)
        {
            LinkerCount = linkerCount;
        }
    }
}
