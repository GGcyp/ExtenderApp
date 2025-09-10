using ExtenderApp.Media.FFmpegEngines;

namespace ExtenderApp.Media
{
    public class MediaEngine
    {
        private readonly FFmpegEngine _engine;

        public MediaEngine(FFmpegEngine engine)
        {
            _engine = engine;
        }

        public MediaPlayer OpenMedia(string mediaPath)
        {
            return null;
        }
    }
}
