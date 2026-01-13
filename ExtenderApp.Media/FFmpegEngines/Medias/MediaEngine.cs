namespace ExtenderApp.FFmpegEngines.Medias
{
    public class MediaEngine
    {
        private readonly FFmpegEngine _engine;

        public MediaEngine(FFmpegEngine engine)
        {
            _engine = engine;
        }

        public IMediaPlayer OpenMedia(Uri mediaUri, FFmpegDecoderSettings? settings = null, Dictionary<string, string>? options = null)
        {
            return OpenMedia(mediaUri.IsFile ? mediaUri.LocalPath : mediaUri.ToString(), settings);
        }

        public IMediaPlayer OpenMedia(string mediaPath, FFmpegDecoderSettings? settings = null, Dictionary<string, string>? options = null)
        {
            settings = settings ?? new();

            var context = _engine.OpenUri(mediaPath, options);
            var ffmpegDecoderController = _engine.CreateDecoderController(context, settings);
            FrameProcessController frameProcessController = new(ffmpegDecoderController);
            return new MediaPlayer(ffmpegDecoderController, frameProcessController);
        }
    }
}