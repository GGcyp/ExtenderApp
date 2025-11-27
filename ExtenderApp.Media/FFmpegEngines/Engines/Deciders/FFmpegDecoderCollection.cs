using System.Collections;
using ExtenderApp.Data;

namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// FFmpeg 解码器集合类。
    /// 封装视频和音频解码器的统一管理，便于解码流程的调度与资源释放。 支持根据流索引获取对应解码器，并自动管理解码器的生命周期。
    /// </summary>
    public class FFmpegDecoderCollection : DisposableObject, IEnumerable<FFmpegDecoder>
    {
        /// <summary>
        /// 内部维护的解码器实例数组。
        /// </summary>
        private readonly FFmpegDecoder[] _decoders;

        public FFmpegDecoder this[int index] => _decoders[index];

        /// <summary>
        /// 获取解码器集合中的解码器数量。
        /// </summary>
        public int Length => _decoders.Length;

        /// <summary>
        /// 获取视频解码器实例。如果不存在则返回 <c>null</c>。
        /// </summary>
        public FFmpegVideoDecoder? VideoDecoder { get; }

        /// <summary>
        /// 获取音频解码器实例。如果不存在则返回 <c>null</c>。
        /// </summary>
        public FFmpegAudioDecoder? AudioDecoder { get; }

        /// <summary>
        /// 初始化 <see cref="FFmpegDecoderCollection"/> 实例，并根据上下文创建所有必需的解码器。
        /// </summary>
        /// <param name="engine">FFmpeg 引擎实例。</param>
        /// <param name="contexts">解码器上下文集合。</param>
        /// <param name="info">媒体基础信息。</param>
        /// <param name="settings">解码器设置参数。</param>
        internal FFmpegDecoderCollection(FFmpegEngine engine, FFmpegDecoderContextCollection contexts, FFmpegInfo info, FFmpegDecoderSettings settings)
        {
            _decoders = new FFmpegDecoder[contexts.Length];

            for (int i = 0; i < contexts.Length; i++)
            {
                var context = contexts[i];
                _decoders[i] = FFmpegDecoderFactory.CreateDecoder(engine, context, info, settings);
            }
        }

        protected override void DisposeManagedResources()
        {
            for (int i = 0; i < _decoders.Length; i++)
            {
                _decoders[i]?.Dispose();
            }
        }

        public T? GetDecoder<T>(FFmpegMediaType mediaType) where T : FFmpegDecoder
        {
            return _decoders[(int)mediaType] as T;
        }

        /// <summary>
        /// 刷新解码器集合的所有解码器缓存状态。
        /// 此操作会重置所有解码器的内部缓存，通常在执行跳转（Seek）操作后调用，以确保解码从新的位置干净地开始。
        /// </summary>
        internal void Flush()
        {
            foreach (var decoder in _decoders)
            {
                decoder.Flush();
            }
        }

        public IEnumerator<FFmpegDecoder> GetEnumerator()
        {
            for (int i = 0; i < _decoders.Length; i++)
            {
                yield return _decoders[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}