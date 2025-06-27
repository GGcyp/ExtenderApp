
namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 多文件模式下的文件信息
    /// </summary>
    public class TorrentFile
    {
        public string Path { get; }
        public long Length { get; }
        public string? Md5 { get; }

        public TorrentFile(string path, long length, string md5 = null)
        {
            Path = path;
            Length = length;
            Md5 = md5;
        }
    }
}
