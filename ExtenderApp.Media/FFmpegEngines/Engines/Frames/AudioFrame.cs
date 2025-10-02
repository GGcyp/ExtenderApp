

using System.Buffers;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 表示一帧音频数据，包含PCM数据及相关音频参数。
    /// 支持通过Dispose方法归还底层字节数组到共享内存池，避免频繁分配和释放内存。
    /// </summary>
    public struct AudioFrame : IDisposable
    {
        /// <summary>
        /// 音频PCM数据缓冲区（来自ArrayPool，需归还）。
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// 有效音频数据长度（字节）。
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 采样率（Hz），如44100。
        /// </summary>
        public int SampleRate { get; }

        /// <summary>
        /// 声道数，如1=单声道，2=立体声。
        /// </summary>
        public int ChannelCount { get; }

        /// <summary>
        /// 每采样点的位深度，如16。
        /// </summary>
        public int BitsPerSample { get; }

        /// <summary>
        /// 帧的时间戳（PTS），用于音视频同步。
        /// </summary>
        public long Pts { get; }

        /// <summary>
        /// 获取当前音频帧是否为空（无数据或长度为0）。
        /// </summary>
        public bool IsEmpty => Data == null || Length == 0;

        /// <summary>
        /// 音频帧的持续时间（微秒），根据采样率、声道数和位深度计算得出。
        /// </summary>
        public long Duration { get; }

        /// <summary>
        /// 构造音频帧实例。
        /// </summary>
        /// <param name="data">PCM数据缓冲区。</param>
        /// <param name="length">有效数据长度。</param>
        /// <param name="sampleRate">采样率。</param>
        /// <param name="channelCount">声道数。</param>
        /// <param name="bitsPerSample">位深度。</param>
        /// <param name="pts">时间戳。</param>
        public AudioFrame(byte[] data, int length, int sampleRate, int channelCount, int bitsPerSample, long pts, long duration)
        {
            Data = data;
            Length = length;
            SampleRate = sampleRate;
            ChannelCount = channelCount;
            BitsPerSample = bitsPerSample;
            Pts = pts;
            Duration = duration;
        }

        /// <summary>
        /// 归还底层PCM数据缓冲区到共享内存池，释放资源。
        /// </summary>
        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(Data);
        }
    }
}
