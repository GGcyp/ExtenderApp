using ExtenderApp.Data;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 输出格式信息封装，便于托管代码访问 AVOutputFormat 相关字段。
    /// </summary>
    public struct FFmpegOutputFormat
    {
        /// <summary>
        /// 空输出格式实例。
        /// </summary>
        public static FFmpegOutputFormat Empty => new(NativeIntPtr<AVOutputFormat>.Empty);

        /// <summary>
        /// 原生 AVOutputFormat 指针封装。
        /// </summary>
        NativeIntPtr<AVOutputFormat> outputFormatIntPtr;

        /// <summary>
        /// 格式短名称（如 "mp4"、"avi"）。
        /// </summary>
        public unsafe string Name => FFmpegFormatExpansion.PtrToString(outputFormatIntPtr.Value->name);

        /// <summary>
        /// 格式全名描述（如 "MPEG-4 Part 14"）。
        /// </summary>
        public unsafe string LongName => FFmpegFormatExpansion.PtrToString(outputFormatIntPtr.Value->long_name);

        /// <summary>
        /// 支持的 MIME 类型（如 "video/mp4"）。
        /// </summary>
        public unsafe string MimeType => FFmpegFormatExpansion.PtrToString(outputFormatIntPtr.Value->mime_type);

        /// <summary>
        /// 默认文件扩展名（如 "mp4,mov"），逗号分隔。
        /// </summary>
        public unsafe string DefaultExtension => FFmpegFormatExpansion.PtrToString(outputFormatIntPtr.Value->extensions);

        /// <summary>
        /// 格式特性标志，参见 FFmpeg AVFMT_* 常量。
        /// </summary>
        public unsafe int Flags => outputFormatIntPtr.Value->flags;

        /// <summary>
        /// 默认音频编解码器 ID。
        /// </summary>
        public unsafe AVCodecID AudioCodec => outputFormatIntPtr.Value->audio_codec;

        /// <summary>
        /// 默认视频编解码器 ID。
        /// </summary>
        public unsafe AVCodecID VideoCodec => outputFormatIntPtr.Value->video_codec;

        /// <summary>
        /// 默认字幕编解码器 ID。
        /// </summary>
        public unsafe AVCodecID SubtitleCodec => outputFormatIntPtr.Value->subtitle_codec;

        /// <summary>
        /// 编解码器标签指针（用于格式与编解码器的关联）。
        /// </summary>
        public unsafe NativeIntPtr<AVCodecTag> CodecTag => new(outputFormatIntPtr.Value->codec_tag);

        /// <summary>
        /// 获取输出格式是否为空。
        /// </summary>
        public bool IsEmpty => outputFormatIntPtr.IsEmpty;

        /// <summary>
        /// 构造函数，使用原生 AVOutputFormat 指针初始化。
        /// </summary>
        /// <param name="outputFormatIntPtr">AVOutputFormat 指针封装。</param>
        public FFmpegOutputFormat(NativeIntPtr<AVOutputFormat> outputFormatIntPtr)
        {
            this.outputFormatIntPtr = outputFormatIntPtr;
        }
    }
}
