

using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 文件信息数据传输对象（DTO）结构体
    /// </summary>
    public struct FileInfoDto
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }

        public FileInfoDto(string fileName, long fileSize)
        {
            FileName = fileName;
            FileSize = fileSize;
        }

        public override int GetHashCode()
        {
            Utility.GetSimpleConsistentHash(FileName, out int hash);
            return hash + (int)FileSize;
        }

        public static implicit operator FileInfoDto(FileInfo info)
        {
            FileInfoDto dto = new FileInfoDto();

            if (info != null)
            {
                dto.FileName = info.Name;

                if (info.Exists)
                    dto.FileSize = info.Length;
            }

            return dto;
        }

        public static implicit operator FileInfoDto(LocalFileInfo info)
        {
            return info.FileInfo;
        }
    }
}
