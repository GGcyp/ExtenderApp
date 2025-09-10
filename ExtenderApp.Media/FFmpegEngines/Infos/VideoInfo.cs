using System.IO;
using System.Windows.Threading;
using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// 表示视频的基本信息，如分辨率、时长、帧率、编码等。
    /// </summary>
    public class VideoInfo : DispatcherObject
    {
        /// <summary>
        /// 视频宽度（像素）
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// 视频高度（像素）
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// 视频时长（秒）
        /// </summary>
        public double Duration { get; }

        /// <summary>
        /// 视频时长（TimeSpan）
        /// </summary>
        public TimeSpan DurationTimeSpan { get; }

        /// <summary>
        /// 视频帧率
        /// </summary>
        public double FrameRate { get; }

        /// <summary>
        /// 视频编码格式
        /// </summary>
        public string CodecName { get; }

        /// <summary>
        /// 视频流的比特率（bps）
        /// </summary>
        public long BitRate { get; set; }

        /// <summary>
        /// 视频源地址（文件路径或流媒体Uri）
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// 文件信息（如果视频源是本地文件）
        /// </summary>
        public FileInfo? FileInfo { get; }

        /// <summary>
        /// 是否为流视频（时长小于等于0）
        /// </summary>
        public bool IsStreamVideo => Duration <= 0;

        public VideoInfo(string uri, int width, int height, double duration, double frameRate, string codecName, long bitRate)
        {
            this.Uri = uri;

            Width = width;
            Height = height;
            BitRate = bitRate;
            Duration = duration;
            FrameRate = frameRate;
            CodecName = codecName;
            DurationTimeSpan = TimeSpan.FromSeconds(duration);
            FileInfo = File.Exists(uri) ? new FileInfo(uri) : null;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Width);
            hash.Add(Height);
            hash.Add(Duration);
            hash.Add(FrameRate);
            hash.Add(CodecName);

            if (FileInfo != null)
            {
                hash.Add(FileInfo.Length);
                hash.Add(FileInfo.CreationTime);
            }
            else
            {
                hash.Add(Uri);
            }
            return hash.ToHashCode();
        }
    }
}
