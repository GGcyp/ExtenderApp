
namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// 表示视频处理过程中发生的异常。
    /// 提供对相关 <see cref="VideoCodec"/> 实例的引用，便于异常追踪和处理。
    /// </summary>
    public class FFmpegCodecException : FFmpegException
    {
        /// <summary>
        /// 发生异常时关联的实例。
        /// </summary>
        public FFmpegCodecBase Value { get; private set; }

        /// <summary>
        /// 使用指定的视频实例、异常消息和内部异常初始化 <see cref="FFmpegCodecException"/> 类的新实例。
        /// </summary>
        /// <param name="video">发生异常的视频实例。</param>
        /// <param name="message">描述异常的消息。</param>
        /// <param name="innerException">导致当前异常的内部异常。</param>
        public FFmpegCodecException(FFmpegCodecBase value, string message, Exception innerException) : base(message, innerException)
        {
            Value = value;
        }

        /// <summary>
        /// 使用指定的视频实例和异常消息初始化 <see cref="FFmpegCodecException"/> 类的新实例。
        /// </summary>
        /// <param name="video">发生异常的视频实例。</param>
        /// <param name="message">描述异常的消息。</param>
        public FFmpegCodecException(FFmpegCodecBase value, string message) : this(value, message, null)
        {
        }

        /// <summary>
        /// 使用指定的视频实例初始化 <see cref="FFmpegCodecException"/> 类的新实例。
        /// </summary>
        /// <param name="video">发生异常的视频实例。</param>
        public FFmpegCodecException(FFmpegCodecBase value) : this(value, string.Empty, null)
        {
        }
    }
}
