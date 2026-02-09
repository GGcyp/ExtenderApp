using ExtenderApp.Contracts;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 输入格式信息封装，便于托管代码访问 AVInputFormat 相关字段。
    /// </summary>
    public struct FFmpegInputFormat
    {
        /// <summary>
        /// 获取空的输入格式实例。
        /// </summary>
        public static FFmpegInputFormat Empty => new(NativeIntPtr<AVInputFormat>.Empty);

        /// <summary>
        /// 原生 AVInputFormat 指针封装。
        /// </summary>
        public NativeIntPtr<AVInputFormat> InputFormatIntPtr { get; }

        /// <summary>
        /// 格式短名称（如 "mp4"、"avi"）。
        /// </summary>
        public unsafe string Name => FFmpegFormatExpansion.PtrToString(InputFormatIntPtr.Value->name);

        /// <summary>
        /// 格式全名描述（如 "MPEG-4 Part 14"）。
        /// </summary>
        public unsafe string LongName => FFmpegFormatExpansion.PtrToString(InputFormatIntPtr.Value->long_name);

        /// <summary>
        /// 支持的文件扩展名（如 "mp4,mov"），逗号分隔。
        /// </summary>
        public unsafe string Extensions => FFmpegFormatExpansion.PtrToString(InputFormatIntPtr.Value->extensions);

        /// <summary>
        /// 支持的 MIME 类型（如 "video/mp4"）。
        /// </summary>
        public unsafe string MimeType => FFmpegFormatExpansion.PtrToString(InputFormatIntPtr.Value->mime_type);

        /// <summary>
        /// 格式特性标志，参见 FFmpeg AVFMT_* 常量。
        /// </summary>
        public unsafe int Flags => InputFormatIntPtr.Value->flags;

        /// <summary>
        /// 编解码器标签指针（用于格式与编解码器的关联）。
        /// </summary>
        public unsafe NativeIntPtr<AVCodecTag> CodecTag => new(InputFormatIntPtr.Value->codec_tag);

        /// <summary>
        /// 检查 AVInputFormat 指针是否为空。
        /// </summary>
        public bool IsEmpty => InputFormatIntPtr.IsEmpty;

        /// <summary>
        /// 构造函数，使用原生 AVInputFormat 指针初始化。
        /// </summary>
        /// <param name="ptr">AVInputFormat 指针封装。</param>
        internal FFmpegInputFormat(NativeIntPtr<AVInputFormat> ptr)
        {
            InputFormatIntPtr = ptr;
        }

        public unsafe static implicit operator AVInputFormat*(FFmpegInputFormat format) => format.InputFormatIntPtr;
    }
}
