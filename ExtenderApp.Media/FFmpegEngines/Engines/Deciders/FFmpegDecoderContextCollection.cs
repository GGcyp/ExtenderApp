using System.Collections;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 表示包含视频和音频解码器上下文的集合
    /// </summary>
    public struct FFmpegDecoderContextCollection : IEnumerable<FFmpegDecoderContext>
    {
        /// <summary>
        /// 视频解码器上下文
        /// </summary>
        private readonly FFmpegDecoderContext[] _decoderContexts;

        /// <summary>
        /// 获取指定媒体类型的解码器上下文
        /// </summary>
        /// <param name="type">指定媒体类型</param>
        /// <returns>指定媒体类型的解码器，如果没有返回空</returns>
        public FFmpegDecoderContext this[FFmpegMediaType type]
        {
            get
            {
                for (int i = 0; i < _decoderContexts.Length; i++)
                {
                    if (_decoderContexts[i].MediaType == type)
                    {
                        return _decoderContexts[i];
                    }
                }
                return FFmpegDecoderContext.Empty;
            }
        }

        /// <summary>
        /// 通过 AVMediaType 获取解码器上下文
        /// </summary>
        /// <param name="type">指定媒体类型</param>
        /// <returns>指定媒体类型的解码器，如果没有返回空</returns>
        public FFmpegDecoderContext this[AVMediaType type] => this[type.Convert()];

        /// <summary>
        /// 检查集合是否为空（即视频和音频解码器上下文均为空）
        /// </summary>
        public bool IsEmpty => _decoderContexts is null;

        /// <summary>
        /// 使用指定的视频和音频解码器上下文初始化 <see
        /// cref="FFmpegDecoderContextCollection"/> 结构的新实例
        /// </summary>
        /// <param name="videoContext">视频解码器上下文</param>
        /// <param name="audioContext">音频解码器上下文</param>
        public FFmpegDecoderContextCollection(FFmpegDecoderContext[] decoderContexts)
        {
            _decoderContexts = decoderContexts;
        }

        public IEnumerator<FFmpegDecoderContext> GetEnumerator()
        {
            for (int i = 0; i < _decoderContexts.Length; i++)
            {
                yield return _decoderContexts[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}