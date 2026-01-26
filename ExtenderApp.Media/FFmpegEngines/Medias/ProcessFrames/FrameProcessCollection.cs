using ExtenderApp.Data;

namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// 帧处理集合：维护“解码器（ <see cref="IFFmpegDecoder"/>）”与“媒体输出（ <see cref="IMediaOutput"/>）”之间的映射关系。
    /// <para>主要用于播放管线中按 <see cref="FFmpegMediaType"/> 查询对应的解码器与输出，并将播放器状态变更广播给所有输出。</para>
    /// </summary>
    internal class FrameProcessCollection : DisposableObject, IFrameProcessCollection
    {
        /// <summary>
        /// 解码器集合（通常包含音频/视频解码器）。
        /// </summary>
        private readonly IFFmpegDecoderCollection _decoders;

        /// <summary>
        /// 已注册的媒体输出列表（按媒体类型区分，通常最多各一个）。
        /// </summary>
        private readonly List<IMediaOutput> _mediaOutputs;

        /// <summary>
        /// 创建帧处理集合。
        /// </summary>
        /// <param name="decoders">解码器集合。</param>
        public FrameProcessCollection(IFFmpegDecoderCollection decoders)
        {
            _decoders = decoders;
            _mediaOutputs = new();
        }

        /// <summary>
        /// 根据媒体类型获取已注册的媒体输出。
        /// </summary>
        /// <param name="mediaType">媒体类型（例如音频或视频）。</param>
        /// <returns>存在则返回对应输出；否则返回 <see langword="null"/>。</returns>
        public IMediaOutput? GetMediaOutput(FFmpegMediaType mediaType)
        {
            for (int i = 0; i < _mediaOutputs.Count; i++)
            {
                var output = _mediaOutputs[i];
                if (output.MediaType == mediaType)
                {
                    return output;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取指定媒体类型的解码器与输出组合。
        /// </summary>
        /// <param name="mediaType">媒体类型（例如音频或视频）。</param>
        /// <returns>
        /// 返回一个元组：
        /// <list type="bullet">
        /// <item>
        /// <description><c>decoder</c>：对应媒体类型的解码器；不存在则为 <see langword="null"/>。</description>
        /// </item>
        /// <item>
        /// <description><c>output</c>：对应媒体类型的输出；不存在则为 <see langword="null"/>。</description>
        /// </item>
        /// </list>
        /// </returns>
        public (IFFmpegDecoder? decoder, IMediaOutput? output) GetDecoderAndOutput(FFmpegMediaType mediaType)
        {
            IFFmpegDecoder? decoder = null;
            foreach (var d in _decoders)
            {
                if (d.MediaType == mediaType)
                {
                    decoder = d;
                    break;
                }
            }

            var output = GetMediaOutput(mediaType);
            return (decoder, output);
        }

        /// <summary>
        /// 添加或替换指定媒体类型的输出实例。
        /// <para>若已存在相同 <see cref="IMediaOutput.MediaType"/> 的输出，则会先释放旧输出，再替换为新输出； 否则将新输出追加到列表中。</para>
        /// </summary>
        /// <param name="mediaOutput">要添加或替换的输出实例。</param>
        public void AddMediaOutput(IMediaOutput mediaOutput)
        {
            for (int i = 0; i < _mediaOutputs.Count; i++)
            {
                var output = _mediaOutputs[i];
                if (output.MediaType == mediaOutput.MediaType)
                {
                    output.Dispose();
                    _mediaOutputs[i] = mediaOutput;
                    return;
                }
            }

            _mediaOutputs.Add(mediaOutput);
        }

        /// <summary>
        /// 通知所有输出播放器状态发生变化（播放/暂停/停止等）。
        /// </summary>
        /// <param name="state">新的播放器状态。</param>
        public void PlayerStateChange(PlayerState state)
        {
            for (int i = 0; i < _mediaOutputs.Count; i++)
            {
                var output = _mediaOutputs[i];
                output.PlayerStateChange(state);
            }
        }

        /// <summary>
        /// 释放集合持有的托管资源。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            foreach (var output in _mediaOutputs)
            {
                output.Dispose();
            }
        }
    }
}