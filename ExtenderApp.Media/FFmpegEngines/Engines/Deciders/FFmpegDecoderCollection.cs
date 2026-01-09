using System.Collections;
using ExtenderApp.Data;

namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// FFmpeg 解码器集合类。 封装视频和音频解码器的统一管理，便于解码流程的调度与资源释放。 支持根据流索引获取对应解码器，并自动管理解码器的生命周期。
    /// </summary>
    public class FFmpegDecoderCollection : DisposableObject, IEnumerable<FFmpegDecoder>
    {
        /// <summary>
        /// 内部维护的解码器实例数组。
        /// </summary>
        private readonly FFmpegDecoder[] _decoders;

        /// <summary>
        /// 根据流索引获取指定的解码器。
        /// </summary>
        /// <param name="index">要获取的解码器的流索引。</param>
        /// <returns>位于指定索引处的 <see cref="FFmpegDecoder"/>。</returns>
        public FFmpegDecoder this[int index] => _decoders[index];

        /// <summary>
        /// 获取解码器集合中的解码器数量。
        /// </summary>
        public int Count => _decoders.Length;

        /// <summary>
        /// 初始化 <see cref="FFmpegDecoderCollection"/> 实例，并根据上下文创建所有必需的解码器。
        /// </summary>
        /// <param name="engine">FFmpeg 引擎实例。</param>
        /// <param name="contexts">解码器上下文集合。</param>
        /// <param name="info">媒体基础信息。</param>
        /// <param name="settings">解码器设置参数。</param>
        internal FFmpegDecoderCollection(FFmpegDecoder[] decoders)
        {
            ArgumentNullException.ThrowIfNull(decoders);
            _decoders = decoders;
        }

        /// <summary>
        /// 释放由集合管理的所有解码器占用的托管资源。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            for (int i = 0; i < _decoders.Length; i++)
            {
                _decoders[i]?.Dispose();
            }
        }

        /// <summary>
        /// 根据媒体类型获取对应的解码器实例。
        /// </summary>
        /// <param name="mediaType">
        /// 要获取的媒体类型（例如 <see cref="FFmpegMediaType.AVMEDIA_TYPE_VIDEO"/> 或 <see cref="FFmpegMediaType.AVMEDIA_TYPE_AUDIO"/>）。
        /// </param>
        /// <returns>对应媒体类型的 <see cref="FFmpegDecoder"/> 实例；如果集合中不存在对应索引则可能抛出 <see cref="IndexOutOfRangeException"/>。</returns>
        public FFmpegDecoder? GetDecoder(FFmpegMediaType mediaType)
        {
            return _decoders.FirstOrDefault(d => d.MediaType == mediaType);
        }

        /// <summary>
        /// 检查集合中是否包含指定媒体类型的解码器。
        /// </summary>
        /// <param name="mediaType">要检查的媒体类型。</param>
        /// <returns>如果集合中包含指定媒体类型的解码器，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool ContainsDecoder(FFmpegMediaType mediaType)
        {
            return _decoders.Any(d => d.MediaType == mediaType);
        }

        /// <summary>
        /// 将所有解码器的内部缓冲区刷新，确保所有待处理的数据包均被处理。
        /// </summary>
        public void FlushAll()
        {
            Array.ForEach(_decoders, static decoder => decoder.FlushInternal());
        }

        /// <summary>
        /// 返回一个循环访问集合的枚举器。
        /// </summary>
        /// <returns>可用于循环访问集合的 <see cref="IEnumerator{T}"/>。</returns>
        public IEnumerator<FFmpegDecoder> GetEnumerator()
        {
            for (int i = 0; i < _decoders.Length; i++)
            {
                yield return _decoders[i];
            }
        }

        /// <summary>
        /// 返回一个循环访问集合的枚举器。
        /// </summary>
        /// <returns>可用于循环访问集合的 <see cref="IEnumerator"/>。</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}