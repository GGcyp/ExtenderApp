using System.Buffers;
using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines;
using NAudio.Wave;
using SoundTouch;

namespace ExtenderApp.Media.Audios
{
    public class AudioPlayer : DisposableObject
    {
        private const double DefaultRateSpeed = 1.0f;

        private readonly WaveOutEvent _waveOut;
        private readonly BufferedWaveProvider _bufferedWave;
        private readonly SoundTouchProcessor _soundTouch;

        private float volume;

        public float Volume
        {
            get => volume;
            set
            {
                _waveOut.Volume = value;
                volume = value;
            }
        }

        private double rate;

        public double Rate
        {
            get => rate;
            set
            {
                rate = value;
                _soundTouch.Rate = value * 100;
            }
        }

        private double tempo;

        public double Tempo
        {
            get => tempo;
            set
            {
                tempo = value;
                _soundTouch.Tempo = value * 100;
            }
        }

        public AudioPlayer(FFmpegDecoderSettings settings, float volume = 0) : this(settings.SampleRate, FFmpegEngine.GetBytesPerSample(settings) * 8, settings.Channels, volume)
        {
        }

        public AudioPlayer(int rate, int bits, int channels, float volume) : this(new WaveFormat(rate, bits, channels), volume)
        {
        }

        internal AudioPlayer(WaveFormat format, float volume)
        {
            _bufferedWave = new(format);
            _waveOut = new WaveOutEvent();
            _waveOut.Init(_bufferedWave);
            _soundTouch = new SoundTouchProcessor
            {
                SampleRate = format.SampleRate,
                Channels = format.Channels,
            };
            Volume = volume;
            //初始为原速
            Rate = DefaultRateSpeed;
            Tempo = DefaultRateSpeed;
        }

        public void Play()
        {
            if (_waveOut.PlaybackState != PlaybackState.Playing)
            {
                _waveOut.Play();
            }
        }

        public void Pause()
        {
            if (_waveOut.PlaybackState == PlaybackState.Playing)
            {
                _waveOut.Pause();
            }
        }

        public void Stop()
        {
            if (_waveOut.PlaybackState != PlaybackState.Stopped)
            {
                _waveOut.Stop();
                _bufferedWave.ClearBuffer();
            }
        }

        public void Clear()
        {
            _bufferedWave.ClearBuffer();
        }

        public void AddSamples(AudioFrame frame)
        {
            AddSamples(frame.Data, 0, frame.Length);
        }

        public void AddSamples(byte[] buffer)
        {
            AddSamples(buffer, 0, buffer.Length);
        }

        public void AddSamples(byte[] buffer, int offset, int count)
        {
            byte[] result = null;
            if (Tempo != DefaultRateSpeed || Rate != DefaultRateSpeed)
            {
                float[] floatSamples = ConvertPcm16BytesToFloat(buffer, offset, count);
                int numSamples = floatSamples.Length / _soundTouch.Channels;
                _soundTouch.PutSamples(floatSamples, numSamples);
                ArrayPool<float>.Shared.Return(floatSamples);
                // 获取可输出样本数
                int available = _soundTouch.AvailableSamples;
                if (available <= 0)
                    return;

                float[] processed = ArrayPool<float>.Shared.Rent(available * _soundTouch.Channels);
                int samplesProcessed = _soundTouch.ReceiveSamples(processed, available);

                // 将 float 数组转换回 byte 数组
                count = samplesProcessed * _soundTouch.Channels * 2;
                byte[] processedBytes = ArrayPool<byte>.Shared.Rent(count);
                for (int i = 0; i < samplesProcessed * _soundTouch.Channels; i++)
                {
                    short s = (short)(Math.Clamp(processed[i], -1f, 1f) * 32767);
                    processedBytes[i * 2] = (byte)(s & 0xFF);
                    processedBytes[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
                }

                ArrayPool<float>.Shared.Return(processed);
                ArrayPool<byte>.Shared.Return(processedBytes);
            }
            else
            {
                result = buffer;
            }

            try
            {
                _bufferedWave.AddSamples(result, offset, count);
            }
            catch (Exception ex)
            {
                _bufferedWave.ClearBuffer();
            }
        }

        private float[] ConvertPcm16BytesToFloat(byte[] pcmBytes, int offset, int length)
        {
            int samples = length / 2;
            float[] floatSamples = ArrayPool<float>.Shared.Rent(samples);
            for (int i = offset; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(pcmBytes, i * 2);
                floatSamples[i] = sample / 32768f; // 归一化到[-1,1]
            }
            return floatSamples;
        }

        protected override void DisposeManagedResources()
        {
            _waveOut.Stop();
            _waveOut.Dispose();
            _bufferedWave.ClearBuffer();
        }
    }
}