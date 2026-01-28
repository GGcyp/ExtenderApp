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

        public IMediaPlayer OpenMedia(Uri mediaUri, FFmpegDecoderSettings? settings = null, Dictionary<string, string>? options = null)
        {
            var context = _engine.Open(mediaUri, options);
            return CreateMediaPlayer(context, settings);
        }

        public IMediaPlayer OpenMedia(FFmpegInfo info, FFmpegDecoderSettings? settings = null, Dictionary<string, string>? options = null)
        {
            var context = _engine.Open(info, options);
            return CreateMediaPlayer(context, settings);
        }

        private IMediaPlayer CreateMediaPlayer(FFmpegContext context, FFmpegDecoderSettings? settings)
        {
            settings ??= new FFmpegDecoderSettings();
            var ffmpegDecoderController = _engine.CreateDecoderController(context, settings);
            var frameProcessController = CreateFrameProcessController(ffmpegDecoderController);
            return new MediaPlayer(ffmpegDecoderController, frameProcessController);
        }

        private IFrameProcessController CreateFrameProcessController(IFFmpegDecoderController decoderController)
        {
            return new FrameProcessController(decoderController);
        }

        public FFmpegInfo CreateFFmpegInfo(Uri mediaUri)
        {
            return _engine.CreateFFmpegInfo(mediaUri);
        }
    }
}