using System.Buffers;
using ExtenderApp.Contracts;
using ExtenderApp.Media.Audios;
using NAudio.Wave;
using SoundTouch;

namespace ExtenderApp.FFmpegEngines.Medias.Outputs
{
    /// <summary>
    /// 媒体音频输出
    /// </summary>
    internal class AudioOutput : DisposableObject, IAudioOutput
    {
        private const double DefaultRateSpeed = 1.0f;

        private readonly WaveOutEvent _waveOut;
        private readonly SpanBufferProvider _bufferedWave;
        private readonly SoundTouchProcessor _soundTouch;

        public double Tempo
        {
            get => _soundTouch.Tempo;
            set
            {
                if (_soundTouch.Tempo != value)
                    _soundTouch.Tempo = value;
            }
        }

        public float Volume
        {
            get => _waveOut.Volume;
            set
            {
                if (_waveOut.Volume != value)
                {
                    float volume = value > 1.0f ? 1.0f : value;
                    _waveOut.Volume = volume;
                }
            }
        }

        public double SpeedRatio
        {
            get => _soundTouch.Rate;
            set
            {
                if (_soundTouch.Rate != value)
                    _soundTouch.Rate = value;
            }
        }

        public FFmpegMediaType MediaType => FFmpegMediaType.AUDIO;

        public AudioOutput(FFmpegDecoderSettings settings)
        {
            // 初始化音频播放器，使用解码器的采样率和通道配置
            WaveFormat format = new(settings.SampleRate, settings.GetBytesPerSample() * 8, settings.Channels);

            _bufferedWave = new(format);
            _waveOut = new();
            _waveOut.Init(_bufferedWave);
            _soundTouch = new SoundTouchProcessor
            {
                SampleRate = format.SampleRate,
                Channels = format.Channels,
            };

            Volume = 0.0f;
            //初始为原速
            SpeedRatio = DefaultRateSpeed;
            Tempo = DefaultRateSpeed;
        }

        public void PlayerStateChange(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Playing:
                    Play();
                    break;

                case PlayerState.Seeking:
                case PlayerState.Paused:
                    Pause();
                    break;

                case PlayerState.Stopped:
                    Stop();
                    break;
            }
        }

        private void Stop()
        {
            if (_waveOut.PlaybackState != PlaybackState.Stopped)
            {
                _waveOut.Stop();
                _bufferedWave.ClearBuffer();
            }
        }

        private void Pause()
        {
            if (_waveOut.PlaybackState == PlaybackState.Playing)
            {
                _waveOut.Pause();
            }
        }

        private void Play()
        {
            if (_waveOut.PlaybackState != PlaybackState.Playing)
            {
                _bufferedWave.ClearBuffer();
                _waveOut.Play();
            }
        }

        public void WriteFrame(FFmpegFrame frame)
        {
            // 将解码后的音频帧写入播放器缓冲
            AddSamples(frame.Block);
        }

        public void AddSamples(ReadOnlySpan<byte> span)
        {
            if (Tempo != DefaultRateSpeed || SpeedRatio != DefaultRateSpeed)
            {
                float[] floatSamples = ConvertPcm16BytesToFloat(span);
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
                int count = samplesProcessed * _soundTouch.Channels * 2;
                byte[] processedBytes = ArrayPool<byte>.Shared.Rent(count);
                for (int i = 0; i < samplesProcessed * _soundTouch.Channels; i++)
                {
                    short s = (short)(Math.Clamp(processed[i], -1f, 1f) * 32767);
                    processedBytes[i * 2] = (byte)(s & 0xFF);
                    processedBytes[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
                }

                try
                {
                    _bufferedWave.AddSamples(processedBytes.AsSpan(0, count));
                }
                catch (Exception)
                {
                    _bufferedWave.ClearBuffer();
                }

                ArrayPool<float>.Shared.Return(processed);
                ArrayPool<byte>.Shared.Return(processedBytes);
            }
            else
            {
                try
                {
                    _bufferedWave.AddSamples(span);
                }
                catch (Exception)
                {
                    _bufferedWave.ClearBuffer();
                }
            }
        }

        public void AddSamples(byte[] buffer, int offset, int count)
        {
            if (Tempo != DefaultRateSpeed || SpeedRatio != DefaultRateSpeed)
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

                try
                {
                    _bufferedWave.AddSamples(processedBytes.AsSpan(0, count));
                }
                catch (Exception)
                {
                    _bufferedWave.ClearBuffer();
                }

                ArrayPool<float>.Shared.Return(processed);
                ArrayPool<byte>.Shared.Return(processedBytes);
            }
            else
            {
                try
                {
                    _bufferedWave.AddSamples(buffer.AsSpan(offset, count));
                }
                catch (Exception)
                {
                    _bufferedWave.ClearBuffer();
                }
            }
        }

        private float[] ConvertPcm16BytesToFloat(ReadOnlySpan<byte> span)
        {
            int samples = span.Length / 2;
            float[] floatSamples = ArrayPool<float>.Shared.Rent(samples);
            for (int i = 0; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(span.Slice(i * 2));
                floatSamples[i] = sample / 32768f; // 归一化到[-1,1]
            }
            return floatSamples;
        }

        private float[] ConvertPcm16BytesToFloat(byte[] pcmBytes, int offset, int length)
        {
            int samples = length / 2;
            float[] floatSamples = ArrayPool<float>.Shared.Rent(samples);
            for (int i = 0; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(pcmBytes, offset + i * 2);
                floatSamples[i] = sample / 32768f; // 归一化到[-1,1]
            }
            return floatSamples;
        }

        protected override void DisposeManagedResources()
        {
            _waveOut.Stop();
            _waveOut.Dispose();
            _bufferedWave.Dispose();
        }
    }
}