using System.IO;
using ExtenderApp.FFmpegEngines;

namespace ExtenderApp.Media.Models
{
    /// <summary>
    /// 视频信息类，用于存储视频相关的各种信息。
    /// </summary>
    public class MediaInfo : IEquatable<MediaInfo>
    {
        /// <summary>
        /// 视频文件的本地存储路径或网络路径（如果支持在线播放等情况）。
        /// </summary>
        public Uri MediaUri { get; set; }

        /// <summary>
        /// 视频已观看时长。
        /// </summary>
        public long MediaWatchedPosition { get; set; }

        /// <summary>
        /// 视频总时长。
        /// </summary>
        public long TotalVideoDuration { get; set; }

        /// <summary>
        /// 视频的标题，通常可以从视频文件元数据或者相关播放平台获取（如果有对应接口）。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 视频的分辨率，用宽度和高度表示，可以是从视频文件属性中解析得到。
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 视频的高度，与 <see cref="Width"/> 共同表示视频的分辨率。
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 音频采样率（Hz）。
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// 音频声道数。
        /// </summary>
        public int Channels { get; set; }

        /// <summary>
        /// 媒体时长（秒）。
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// 视频帧率（FPS）。
        /// </summary>
        public double Rate { get; set; }

        /// <summary>
        /// 媒体码率（比特率，单位：bps）。
        /// </summary>
        public long BitRate { get; set; }

        /// <summary>
        /// 视频的播放次数统计，可用于应用程序内分析用户观看习惯等。
        /// </summary>
        public int PlayCount { get; set; }

        /// <summary>
        /// 视频的像素格式。
        /// </summary>
        public FFmpegPixelFormat PixelFormat { get; set; }

        /// <summary>
        /// 媒体源 URI 或文件路径。
        /// </summary>
        public FFmpegSampleFormat SampleFormat { get; set; }

        /// <summary>
        /// 视频的收藏状态，布尔值表示是否被用户收藏，方便实现个性化功能，如收藏列表展示等。
        /// </summary>
        public bool IsFavorite { get; set; }

        public MediaInfo()
        {
            Rate = 0;
            Width = 0;
            Height = 0;
            BitRate = 0;
            Channels = 0;
            Duration = 0;
            PlayCount = 0;
            SampleRate = 0;
            IsFavorite = false;
            MediaWatchedPosition = 0;
            TotalVideoDuration = 0;
            PixelFormat = FFmpegPixelFormat.PIX_FMT_NONE;
            SampleFormat = FFmpegSampleFormat.SAMPLE_FMT_NONE;
            IsFavorite = false;

            MediaUri = default!;
            Title = default!;
        }

        public MediaInfo(Uri mediaUri)
        {
            Rate = 0;
            Width = 0;
            Height = 0;
            BitRate = 0;
            Channels = 0;
            Duration = 0;
            PlayCount = 0;
            SampleRate = 0;
            IsFavorite = false;
            MediaWatchedPosition = 0;
            TotalVideoDuration = 0;
            PixelFormat = FFmpegPixelFormat.PIX_FMT_NONE;
            SampleFormat = FFmpegSampleFormat.SAMPLE_FMT_NONE;
            IsFavorite = false;

            MediaUri = mediaUri;
            Title = mediaUri.IsFile ? Path.GetFileName(mediaUri.LocalPath) : mediaUri.AbsoluteUri;
        }

        public MediaInfo(FFmpegInfo info)
        {
            TotalVideoDuration = info.Duration;
            Width = info.Width;
            Height = info.Height;
            PixelFormat = info.PixelFormat;
            SampleFormat = info.SampleFormat;
            Rate = info.Rate;
            BitRate = info.BitRate;
            SampleRate = info.SampleRate;
            Channels = info.Channels;
            Duration = info.Duration;
            PlayCount = 0;
            IsFavorite = false;
            MediaWatchedPosition = 0;

            MediaUri = info.MediaUri;
            Title = info.MediaUri.IsFile ? Path.GetFileName(info.MediaUri.LocalPath) : info.MediaUri.AbsoluteUri;
        }

        public static bool operator ==(MediaInfo? left, MediaInfo? right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(MediaInfo? left, MediaInfo? right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is MediaInfo info && MediaUri == info.MediaUri;
        }

        public bool Equals(MediaInfo? other)
        {
            return other is not null && MediaUri == other.MediaUri;
        }

        public override int GetHashCode()
        {
            return MediaUri.GetHashCode();
        }

        public static implicit operator MediaInfo(FFmpegInfo info)
            => new MediaInfo(info);
    }
}