namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// 帧处理集合接口：用于维护“解码器（ <see cref="IFFmpegDecoder"/>）”与“媒体输出（ <see cref="IMediaOutput"/>）”之间的关联关系， 以支持播放/渲染等场景下按媒体类型（音频/视频）路由帧数据。
    /// </summary>
    public interface IFrameProcessCollection
    {
        /// <summary>
        /// 添加或替换一个媒体输出实例。
        /// <para>当集合中已存在相同 <see cref="IMediaOutput.MediaType"/> 的输出时，应替换为新的输出实例。</para>
        /// </summary>
        /// <param name="mediaOutput">要添加或替换的媒体输出实例。</param>
        void AddMediaOutput(IMediaOutput mediaOutput);

        /// <summary>
        /// 获取指定媒体类型对应的“解码器 + 输出”组合。
        /// </summary>
        /// <param name="mediaType">媒体类型（例如音频或视频）。</param>
        /// <returns>
        /// 返回一个元组：
        /// <list type="bullet">
        /// <item>
        /// <description><c>decoder</c>：指定媒体类型的解码器；如果不存在返回 <see langword="null"/>。</description>
        /// </item>
        /// <item>
        /// <description><c>output</c>：指定媒体类型的输出；如果不存在返回 <see langword="null"/>。</description>
        /// </item>
        /// </list>
        /// </returns>
        (IFFmpegDecoder? decoder, IMediaOutput? output) GetDecoderAndOutput(FFmpegMediaType mediaType);

        /// <summary>
        /// 获取指定媒体类型所绑定的媒体输出实例。
        /// </summary>
        /// <param name="mediaType">要查找的媒体类型。</param>
        /// <returns>如果存在对应输出则返回该 <see cref="IMediaOutput"/>；否则返回 <see langword="null"/>。</returns>
        IMediaOutput? GetMediaOutput(FFmpegMediaType mediaType);

        /// <summary>
        /// 当播放器状态发生变化时调用此方法。
        /// </summary>
        /// <param name="state">新的播放状态</param>
        void PlayerStateChange(PlayerState state);
    }
}