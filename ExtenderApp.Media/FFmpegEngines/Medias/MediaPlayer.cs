using ExtenderApp.Common;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Media.FFmpegEngines.Audios;
using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    public class MediaPlayer : DisposableObject, IFFmpegCodecScheduling<VideoFrame>
    {
        private readonly SortedDictionary<long, DataBuffer<VideoFrame, AudioFrame>> _frameDict;
        private readonly NativeIntPtr<AVFormatContext> formatContextIntPtr;
        private readonly CacheStateController _videoController;
        private readonly CacheStateController _audioController;
        private readonly int maxFrameCount;
        private readonly int minFrameCount;

        public VideoCodec? Video { get; private set; }

        public MediaPlayer(NativeIntPtr<AVFormatContext> formatContextIntPtr, CacheStateController videoController, CacheStateController audioController, int maxFrameCount, int minframeCount)
        {
            this.formatContextIntPtr = formatContextIntPtr;
            _frameDict = new();
            _videoController = videoController;
            _audioController = audioController;
        }

        public void Inject(VideoCodec video)
        {
            Video = video;
        }

        public DataBuffer<VideoFrame, AudioFrame>? ReadFrame(long pts)
        {
            if (!_frameDict.TryGetValue(pts, out var buffer))
            {
                return buffer;
            }

            _frameDict.Remove(pts);
            _videoController.OnFrameRemoved();
            return buffer;
        }

        public void Schedule(VideoFrame item)
        {
            if (item.IsEmpty)
                throw new Exception("传入视频帧不能为空");

            var buffer = GetBuffer(item.PTS);
            if (!buffer.Item1.IsEmpty)
                return;

            buffer.Item1 = item;
            _videoController.OnFrameAdded();
        }

        private DataBuffer<VideoFrame, AudioFrame> GetBuffer(long pts)
        {
            if (_frameDict.TryGetValue(pts, out var buffer))
            {
                return buffer;
            }
            buffer = DataBuffer<VideoFrame, AudioFrame>.GetDataBuffer();
            _frameDict.Add(pts, buffer);
            return buffer;
        }

        public bool CanSchedule()
        {
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            //ffmpeg.avformat_close_input(formatContextIntPtr.ValuePtr);
        }
    }
}
