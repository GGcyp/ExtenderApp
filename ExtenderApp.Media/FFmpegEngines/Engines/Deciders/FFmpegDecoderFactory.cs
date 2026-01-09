namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// FFmpeg 解码器工厂。
    /// <para>
    /// 根据 <see cref="FFmpegDecoderContext.MediaType"/> 创建对应的具体解码器实现，并向其注入控制器上下文与解码设置。
    /// </para>
    /// <para>
    /// 设计要点：
    /// <list type="bullet">
    /// <item><description>集中创建逻辑：避免在控制器中散落多处 <c>new</c> 与类型分支。</description></item>
    /// <item><description>类型安全：通过 <c>switch</c> 表达式显式覆盖支持的媒体类型；不支持类型直接抛异常。</description></item>
    /// <item><description>依赖注入：将共享的 <see cref="FFmpegDecoderControllerContext"/>（含引擎与代际）传入解码器，保证 Seek/flush 语义一致。</description></item>
    /// </list>
    /// </para>
    /// </summary>
    internal static class FFmpegDecoderFactory
    {
        /// <summary>
        /// 根据 <paramref name="context"/> 里的媒体类型创建解码器实例。
        /// </summary>
        /// <param name="context">
        /// 解码器上下文（包含媒体类型、流索引及底层 codec/stream 指针等）。
        /// <para>
        /// 注意：该上下文通常持有原生指针，其生命周期应覆盖解码器的使用期。
        /// </para>
        /// </param>
        /// <param name="controllerContext">
        /// 控制器上下文（共享 <see cref="FFmpegEngine"/>、共享 <see cref="FFmpegContext"/>、以及代际 generation 管理）。
        /// </param>
        /// <param name="settings">
        /// 解码器设置（缓存大小、目标像素格式/音频输出格式等）。
        /// </param>
        /// <returns>
        /// 返回具体的 <see cref="FFmpegDecoder"/> 实例：
        /// <list type="bullet">
        /// <item><description><see cref="FFmpegVideoDecoder"/>：当 <see cref="FFmpegMediaType.VIDEO"/>。</description></item>
        /// <item><description><see cref="FFmpegAudioDecoder"/>：当 <see cref="FFmpegMediaType.AUDIO"/>。</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// 当 <paramref name="context"/> 的媒体类型不是受支持的类型（例如字幕/数据流等）时抛出。
        /// </exception>
        public static FFmpegDecoder CreateDecoder(FFmpegDecoderContext context, FFmpegDecoderControllerContext controllerContext, FFmpegDecoderSettings settings)
        {
            return context.MediaType switch
            {
                FFmpegMediaType.VIDEO => new FFmpegVideoDecoder(context, controllerContext, settings),
                FFmpegMediaType.AUDIO => new FFmpegAudioDecoder(context, controllerContext, settings),
                _ => throw new NotSupportedException($"不支持的解码器类型: {context.MediaType}"),
            };
        }
    }
}