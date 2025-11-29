using ExtenderApp.Data;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 表示一个从 FFmpeg 解码器输出的媒体帧。 这是一个只读结构体，封装了帧的原始数据和显示时间戳（PTS）。
    /// </summary>
    public readonly struct FFmpegFrame : IDisposable
    {
        /// <summary>
        /// 获取帧的原始数据块。 对于视频，这通常是像素数据（如 RGB 或
        /// YUV）；对于音频，这是 PCM 采样数据。
        /// </summary>
        public ByteBlock Block { get; }

        /// <summary>
        /// 获取帧的显示时间戳（Presentation Timestamp）。 该时间戳用于控制帧的显示或播放顺序，是音视频同步的关键。
        /// </summary>
        public long Pts { get; }

        /// <summary>
        /// 获取一个值，该值指示此帧是否为空（即不包含任何数据）。
        /// </summary>
        public bool IsEmpty => Block.IsEmpty;

        public FFmpegFrame(ByteBlock block, long pts)
        {
            Block = block;
            Pts = pts;
        }

        /// <summary>
        /// 释放由 <see cref="Block"/> 属性持有的资源。
        /// </summary>
        public void Dispose()
        {
            Block.Dispose();
        }
    }
}