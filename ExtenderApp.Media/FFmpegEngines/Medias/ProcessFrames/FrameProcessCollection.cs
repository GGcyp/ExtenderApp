using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines.Decoders;

namespace ExtenderApp.FFmpegEngines.Medias
{
    internal class FrameProcessCollection : DisposableObject
    {
        private readonly FFmpegDecoderCollection _decoders;
        private readonly IMediaOutput[] _mediaOutputs;

        public FrameProcessCollection(FFmpegDecoderCollection decoders)
        {
            _decoders = decoders;
            _mediaOutputs = new IMediaOutput[_decoders.Count];
        }
    }
}