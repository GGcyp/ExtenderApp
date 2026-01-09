using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines.Decoders;

namespace ExtenderApp.FFmpegEngines.Medias
{
    internal class FrameProcessController : DisposableObject
    {
        public FrameProcessCollection FrameProcesses;
        public FrameProcessController(FFmpegDecoderCollection decoders)
        {
            FrameProcesses = new (decoders);
        }

        public void Processing()
        {

        }
    }
}