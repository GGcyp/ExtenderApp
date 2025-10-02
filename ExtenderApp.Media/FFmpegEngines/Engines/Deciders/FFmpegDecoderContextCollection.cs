

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 表示包含视频和音频解码器上下文的集合
    /// </summary>
    public struct FFmpegDecoderContextCollection
    {
        /// <summary>
        /// 视频解码器上下文
        /// </summary>
        public FFmpegDecoderContext VideoContext;

        /// <summary>
        /// 音频解码器上下文
        /// </summary>
        public FFmpegDecoderContext AudioContext;

        /// <summary>
        /// 检查集合是否为空（即视频和音频解码器上下文均为空）
        /// </summary>
        public bool IsEmpty => VideoContext.IsEmpty && AudioContext.IsEmpty;

        /// <summary>
        /// 使用指定的视频和音频解码器上下文初始化 <see cref="FFmpegDecoderContextCollection"/> 结构的新实例
        /// </summary>
        /// <param name="videoContext">视频解码器上下文</param>
        /// <param name="audioContext">音频解码器上下文</param>
        public FFmpegDecoderContextCollection(FFmpegDecoderContext videoContext, FFmpegDecoderContext audioContext)
        {
            VideoContext = videoContext;
            AudioContext = audioContext;
        }
    }
}
