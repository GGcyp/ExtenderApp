
namespace ExtenderApp.Media.FFmpegEngines.Videos
{
    public class Video
    {
        public VideoInfo Info { get; private set; }

        public Video(VideoInfo info)
        {
            Info = info;
        }
    }
}
