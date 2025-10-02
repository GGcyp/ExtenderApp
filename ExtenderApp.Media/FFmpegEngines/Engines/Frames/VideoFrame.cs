using System.Buffers;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 表示单个视频帧的数据结构。
    /// 包含帧的原始字节数据及其在视频流中的索引。
    /// </summary>
    public readonly struct VideoFrame : IDisposable
    {
        /// <summary>
        /// 视频帧的原始字节数据。
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// 视频帧在视频流中的索引（从0开始）。
        /// </summary>
        public long Pts { get; }

        /// <summary>
        /// 当前视频帧的宽度。
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// 当前视频帧的高度。
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// 当前视频帧的行跨度（以字节为单位）。
        /// </summary>
        public int Stride { get; }

        /// <summary>
        /// 判断当前视频帧是否为空。
        /// </summary>
        public bool IsEmpty => Data == null;

        public VideoFrame(byte[] data, long pts, int width, int height, int stride)
        {
            Data = data;
            Pts = pts;
            Width = width;
            Height = height;
            Stride = stride;
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(Data);
        }
    }
}
