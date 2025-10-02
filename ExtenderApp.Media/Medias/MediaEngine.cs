﻿using System.IO;
using ExtenderApp.FFmpegEngines;

namespace ExtenderApp.Media
{
    public class MediaEngine
    {
        private readonly FFmpegEngine _engine;

        public MediaEngine(FFmpegEngine engine)
        {
            _engine = engine;
        }

        public MediaPlayer OpenMedia(Uri mediaUri, FFmpegDecoderSettings? settings = null)
        {
            return OpenMedia(mediaUri.IsFile ? mediaUri.LocalPath : mediaUri.ToString(), settings);
        }

        public MediaPlayer OpenMedia(string mediaPath, FFmpegDecoderSettings? settings = null)
        {
            settings = settings ?? new();

            var context = _engine.OpenUri(mediaPath);
            CancellationTokenSource source = new();
            var controller = _engine.CreateDecoderController(context, settings, source);
            return new MediaPlayer(controller, source, settings);
        }
    }
}
