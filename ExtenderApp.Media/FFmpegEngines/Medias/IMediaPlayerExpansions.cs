namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// <see cref="IMediaPlayer"/> 的扩展方法集合。
    /// <para>
    /// 提供与播放器输出端（<see cref="IMediaOutput"/>）交互的便捷封装，例如音频音量读取/设置。
    /// </para>
    /// </summary>
    public static class IMediaPlayerExpansions
    {
        /// <summary>
        /// 获取播放器当前音频输出的音量值。
        /// <para>
        /// 该方法会尝试从 <see cref="IMediaPlayer.FrameProcessCollection"/> 中获取音频输出（<see cref="FFmpegMediaType.AUDIO"/>），
        /// 并在输出实现为 <see cref="IAudioOutput"/> 时返回其 <see cref="IAudioOutput.Volume"/>。
        /// </para>
        /// <para>
        /// 当播放器为 null、未绑定音频输出、或输出不是 <see cref="IAudioOutput"/> 时，返回 0。
        /// </para>
        /// </summary>
        /// <param name="mediaPlayer">播放器实例。</param>
        /// <returns>音量值；无法获取时返回 0。</returns>
        public static float GetVolume(this IMediaPlayer mediaPlayer)
        {
            if (mediaPlayer is null)
                return 0;

            var output = mediaPlayer.FrameProcessCollection.GetMediaOutput(FFmpegMediaType.AUDIO);
            if (output is null || output is not IAudioOutput audioOutput)
                return 0;

            return audioOutput.Volume;
        }

        /// <summary>
        /// 设置播放器当前音频输出的音量值。
        /// <para>
        /// 该方法会尝试从 <see cref="IMediaPlayer.FrameProcessCollection"/> 中获取音频输出（<see cref="FFmpegMediaType.AUDIO"/>），
        /// 并在输出实现为 <see cref="IAudioOutput"/> 时设置其 <see cref="IAudioOutput.Volume"/>。
        /// </para>
        /// <para>
        /// 当播放器为 null、未绑定音频输出、或输出不是 <see cref="IAudioOutput"/> 时，此方法不执行任何操作。
        /// </para>
        /// </summary>
        /// <param name="mediaPlayer">播放器实例。</param>
        /// <param name="volume">要设置的音量值。</param>
        public static void SetVolume(this IMediaPlayer mediaPlayer, float volume)
        {
            if (mediaPlayer is null)
                return;

            var output = mediaPlayer.FrameProcessCollection.GetMediaOutput(FFmpegMediaType.AUDIO);
            if (output is null || output is not IAudioOutput audioOutput)
                return;

            audioOutput.Volume = volume;
        }
    }
}