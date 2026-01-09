using ExtenderApp.FFmpegEngines.Decoders;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 表示 FFmpeg 解码器的只读集合。
    /// </summary>
    /// <remarks>
    /// 该集合通常按媒体类型（<see cref="FFmpegMediaType"/>）组织解码器，并提供按索引与按类型两种访问方式。
    /// </remarks>
    public interface IFFmpegDecoderCollection : IEnumerable<FFmpegDecoder>
    {
        /// <summary>
        /// 获取集合中的解码器数量。
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取指定索引处的解码器。
        /// </summary>
        /// <param name="index">从零开始的索引。</param>
        /// <returns>对应索引的 <see cref="FFmpegDecoder"/>。</returns>
        FFmpegDecoder this[int index] { get; }

        /// <summary>
        /// 根据媒体类型获取解码器。
        /// </summary>
        /// <param name="mediaType">媒体类型。</param>
        /// <returns>
        /// 若存在对应类型的解码器则返回其实例；否则返回 <see langword="null"/>。
        /// </returns>
        FFmpegDecoder? GetDecoder(FFmpegMediaType mediaType);

        /// <summary>
        /// 确定集合中是否包含指定媒体类型的解码器。
        /// </summary>
        /// <param name="mediaType">媒体类型。</param>
        /// <returns>存在则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        bool ContainsDecoder(FFmpegMediaType mediaType);
    }
}