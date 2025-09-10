using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtenderApp.Media.FFmpegEngines
{
    public interface IMediaFrame
    {
        long Timestamp { get; }
        MediaFrameType FrameType { get; } // Video/Audio
        byte[] Data { get; }
    }
    public enum MediaFrameType { Video, Audio }

    public interface IMediaParser
    {
        IEnumerable<IMediaFrame> Parse(string uri);
    }

    public interface IMediaPlayer
    {
        void Play();
        void Pause();
        void Stop();
        IMediaFrame ReadNextFrame();
    }
}
