using System.IO;
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

        public MediaPlayer OpenMedia(string mediaPath, FFmpegDecoderSettings? settings = null)
        {
            //if (string.IsNullOrWhiteSpace(mediaPath))
            //{
            //    throw new ArgumentException("媒体路径不能为空", nameof(mediaPath));
            //}
            //if (!File.Exists(mediaPath))
            //{
            //    throw new FileNotFoundException("媒体文件未找到", mediaPath);
            //}
            settings = settings ?? new();

            var context = _engine.OpenUri(mediaPath);
            CancellationTokenSource source = new();
            var controller = _engine.CreateDecoderController(context, settings);
            return new MediaPlayer(controller, source, settings);
        }
    }
}
