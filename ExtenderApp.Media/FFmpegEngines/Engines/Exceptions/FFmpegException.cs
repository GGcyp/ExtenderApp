

namespace ExtenderApp.Media.FFmpegEngines
{
    public class FFmpegException : Exception
    {
        public FFmpegException()
        {
        }

        public FFmpegException(string? message) : base(message)
        {
        }

        public FFmpegException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
