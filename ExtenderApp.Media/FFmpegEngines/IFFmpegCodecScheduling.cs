

namespace ExtenderApp.Media.FFmpegEngines
{
    public interface IFFmpegCodecScheduling<T>
    {
        void Schedule(T item);  
    }
}
