namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// 媒体引擎门面。
    /// <para>
    /// 负责把“媒体路径/URI”转换为可播放的 <see cref="IMediaPlayer"/>： 内部完成打开媒体（ <see cref="FFmpegEngine.OpenUri(string, Dictionary{string, string}?)"/>）、 创建解码控制器、并绑定帧处理控制器。
    /// </para>
    /// </summary>
    public class MediaEngine
    {
        private readonly FFmpegEngine _engine;

        /// <summary>
        /// 初始化 <see cref="MediaEngine"/>。
        /// </summary>
        /// <param name="engine">FFmpeg 引擎实例，用于打开媒体并创建解码器控制器。</param>
        /// <exception cref="ArgumentNullException"><paramref name="engine"/> 为 null 时抛出。</exception>
        public MediaEngine(FFmpegEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        /// <summary>
        /// 打开媒体并返回播放器实例。
        /// <para>
        /// 当 <paramref name="mediaUri"/> 为本地文件时使用 <see cref="Uri.LocalPath"/>； 否则使用 <see cref="Uri.ToString()"/> 作为 FFmpeg 打开地址（例如 http/https/rtsp 等）。
        /// </para>
        /// </summary>
        /// <param name="mediaUri">媒体 URI（本地文件或网络地址）。</param>
        /// <param name="settings">解码器设置；为 null 时使用默认值。</param>
        /// <param name="options">FFmpeg 打开选项（AVDictionary），例如超时、探测大小、rtsp_transport 等。</param>
        /// <returns>可用于播放控制的 <see cref="IMediaPlayer"/> 实例。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="mediaUri"/> 为 null 时抛出。</exception>
        /// <exception cref="FFmpegException">FFmpeg 打开失败/解析失败时抛出。</exception>
        public IMediaPlayer OpenMedia(Uri mediaUri, FFmpegDecoderSettings? settings = null, Dictionary<string, string>? options = null)
        {
            ArgumentNullException.ThrowIfNull(mediaUri);

            return OpenMedia(mediaUri.IsFile ? mediaUri.LocalPath : mediaUri.ToString(), settings, options);
        }

        /// <summary>
        /// 打开媒体并返回播放器实例。
        /// <para>典型流程：
        /// <list type="number">
        /// <item>
        /// <description>调用 <see cref="FFmpegEngine.OpenUri(string, Dictionary{string, string}?)"/> 打开媒体，构建 <see cref="FFmpegContext"/>。</description>
        /// </item>
        /// <item>
        /// <description>调用引擎扩展创建解码控制器（ <c>CreateDecoderController</c>）。</description>
        /// </item>
        /// <item>
        /// <description>创建 <see cref="FrameProcessController"/> 负责帧投递/节拍控制。</description>
        /// </item>
        /// <item>
        /// <description>返回 <see cref="MediaPlayer"/>。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="mediaPath">媒体路径或 URL。</param>
        /// <param name="settings">解码器设置；为 null 时使用默认值。</param>
        /// <param name="options">FFmpeg 打开选项（AVDictionary）。</param>
        /// <returns>可用于播放控制的 <see cref="IMediaPlayer"/> 实例。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="mediaPath"/> 为 null 时抛出。</exception>
        /// <exception cref="FFmpegException">FFmpeg 打开失败/解析失败时抛出。</exception>
        public IMediaPlayer OpenMedia(string mediaPath, FFmpegDecoderSettings? settings = null, Dictionary<string, string>? options = null)
        {
            ArgumentNullException.ThrowIfNull(mediaPath);

            settings ??= new FFmpegDecoderSettings();

            var context = _engine.OpenUri(mediaPath, options);
            var ffmpegDecoderController = _engine.CreateDecoderController(context, settings);
            FrameProcessController frameProcessController = new(ffmpegDecoderController);
            return new MediaPlayer(ffmpegDecoderController, frameProcessController);
        }

        public FFmpegInfo CreateFFmpegInfo(string uri)
        {
            return _engine.CreateFFmpegInfo(uri);
        }
    }
}