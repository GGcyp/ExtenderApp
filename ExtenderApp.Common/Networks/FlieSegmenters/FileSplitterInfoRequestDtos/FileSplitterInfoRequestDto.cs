

namespace ExtenderApp.Common.Networks
{

    /// <summary>
    /// 文件分割请求信息数据传输对象
    /// </summary>
    internal struct FileSplitterInfoRequestDto
    {
        /// <summary>
        /// 文件哈希码
        /// </summary>
        public int FileHashCode { get; set; }

        /// <summary>
        /// 分割块大小
        /// </summary>
        public int SplitterSize { get; set; }
    }
}
