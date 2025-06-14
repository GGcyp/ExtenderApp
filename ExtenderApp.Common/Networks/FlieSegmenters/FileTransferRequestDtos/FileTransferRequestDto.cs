

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 文件传输请求数据传输对象
    /// </summary>
    public struct FileTransferRequestDto
    {
        /// <summary>
        /// 文件信息数据传输对象数组
        /// </summary>
        public FileInfoDto[] FileInfoDtos { get; set; }

        /// <summary>
        /// 初始化文件传输请求数据传输对象
        /// </summary>
        /// <param name="fileInfoDtos">文件信息数据传输对象数组</param>
        /// <param name="timestamp">时间戳</param>
        public FileTransferRequestDto(FileInfoDto[] fileInfoDtos)
        {
            FileInfoDtos = fileInfoDtos;
        }
    }
}
