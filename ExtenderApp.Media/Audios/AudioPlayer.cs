using ExtenderApp.Media.FFmpegEngines;
using NAudio.Wave;

namespace ExtenderApp.Media.Audios
{
    public class AudioPlayer
    {
        public WaveOutEvent WaveOut { get; }
        public BufferedWaveProvider BufferedWave { get; }

        private double volume;

        public double Volume
        {
            get => volume;
            set
            {
                WaveOut.Volume = (float)value;
                volume = value;
            }
        }

        public AudioPlayer(FFmpegInfo info, double volume = 0) : this(info.SampleRate, 16, info.Channels, volume)
        {
        }

        public AudioPlayer(int rate, int bits, int channels, double volume) : this(new WaveFormat(rate, bits, channels), volume)
        {
        }

        public AudioPlayer(WaveFormat format, double volume = 0.5f)
        {
            BufferedWave = new(format);
            WaveOut = new WaveOutEvent();
            WaveOut.Init(BufferedWave);
            Volume = 0.5;
        }
    }
}