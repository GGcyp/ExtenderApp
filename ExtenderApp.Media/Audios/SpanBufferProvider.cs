using ExtenderApp.Data;
using NAudio.Wave;

namespace ExtenderApp.Media.Audios
{
    /// <summary>
    /// 基于固定大小字节数组的环形缓冲区实现，作为 NAudio 的 <see cref="IWaveProvider"/> 数据源。
    /// - 写入端通过 <see cref="AddSamples(ByteBlock)"/> 将来自解码器/网络的音频字节追加到缓冲；
    /// - 读取端通过 <see cref="Read(byte[], int, int)"/> 由 NAudio 拉取数据并播放。
    /// 实现为不可自动增长：当写入数据超过缓冲容量时，会覆盖最旧数据以腾出空间（覆盖策略）。 线程安全：对读/写操作使用内部锁保证并发安全。
    /// </summary>
    internal class SpanBufferProvider : IWaveProvider
    {
        /// <summary>
        /// 环形缓冲底层字节数组。
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        /// 缓冲所能表示的持续时间（基于 WaveFormat.AverageBytesPerSecond）。
        /// </summary>
        private readonly TimeSpan _bufferDuration;

        /// <summary>
        /// 写指针（指向下一次写入的索引）。
        /// </summary>
        private int writePosition;

        /// <summary>
        /// 读指针（指向下一次读取的索引）。
        /// </summary>
        private int readPosition;

        /// <summary>
        /// 当前缓冲中可供读取的字节数（已写入但未读取的字节数）。
        /// </summary>
        private int availableBytes;

        /// <summary>
        /// 用于读/写操作的独占锁对象，保证线程安全。
        /// </summary>
        private readonly object sync = new();

        /// <summary>
        /// 当前缓冲提供的数据格式（采样率/位深/通道数）。
        /// </summary>
        public WaveFormat WaveFormat { get; }

        /// <summary>
        /// 缓冲长度（字节）。
        /// </summary>
        public int BufferLength { get; }

        /// <summary>
        /// 是否在读取时始终返回请求长度（不足时填充静音）。默认 true（与 NAudio.BufferedWaveProvider.ReadFully 行为一致）。
        /// </summary>
        public bool ReadFully { get; set; }

        /// <summary>
        /// 缓冲所能表示的持续时间（只读）。
        /// </summary>
        public TimeSpan BufferDuration => _bufferDuration;

        /// <summary>
        /// 使用指定的 <paramref name="waveFormat"/> 构造一个固定大小的环形缓冲区提供器。 缓冲大小按每秒字节数 * 5 计算（可播放约 5 秒数据，基于 waveFormat.AverageBytesPerSecond）。
        /// </summary>
        /// <param name="waveFormat">音频格式描述。</param>
        public SpanBufferProvider(WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;
            BufferLength = waveFormat.AverageBytesPerSecond * 5;
            _bufferDuration = TimeSpan.FromSeconds((double)BufferLength / (double)WaveFormat.AverageBytesPerSecond);
            _buffer = new byte[BufferLength];
            ReadFully = true;

            writePosition = 0;
            readPosition = 0;
            availableBytes = 0;
        }

        public void AddSamples(ReadOnlySpan<byte> samples)
        {
            if (samples.IsEmpty || samples.Length == 0)
                return;

            lock (sync)
            {
                int freeSpace = BufferLength - availableBytes;
                // 如果数据比可用空间大，则丢弃最旧的数据以腾出空间（移动 readPosition）
                if (samples.Length > freeSpace)
                {
                    int need = samples.Length - freeSpace;
                    // 丢弃 need 字节
                    if (need >= availableBytes)
                    {
                        // 丢弃所有已有数据
                        readPosition = 0;
                        writePosition = 0;
                        availableBytes = 0;
                    }
                    else
                    {
                        readPosition = (readPosition + need) % BufferLength;
                        availableBytes -= need;
                    }
                }

                // 写入数据（可能需要两段写入）
                int toWrite = samples.Length;
                int firstCopy = Math.Min(toWrite, BufferLength - writePosition);
                samples.Slice(0, firstCopy).CopyTo(_buffer.AsSpan(writePosition, firstCopy));
                writePosition += firstCopy;
                if (writePosition >= BufferLength) writePosition = 0;

                int remaining = toWrite - firstCopy;
                if (remaining > 0)
                {
                    samples.Slice(firstCopy, remaining).CopyTo(_buffer.AsSpan(writePosition, remaining));
                    writePosition += remaining;
                    if (writePosition >= BufferLength) writePosition = 0;
                }

                availableBytes = Math.Min(BufferLength, availableBytes + toWrite);
            }
        }

        /// <summary>
        /// 从环形缓冲读取数据，供 NAudio 播放器调用。
        /// - 返回实际读取的字节数（可能小于请求的 <paramref name="count"/>，当缓冲不足时会返回可用数据长度）；
        /// - 若当前无可读数据，返回 0（NAudio 将以此决定是否播放静音或重试）。
        /// </summary>
        /// <param name="buffer">目标字节数组，不能为 null。</param>
        /// <param name="offset">写入目标数组的起始偏移。</param>
        /// <param name="count">请求读取的字节数。</param>
        /// <returns>实际读取并写入到 <paramref name="buffer"/> 的字节数。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="offset"/> 或 <paramref name="count"/> 无效时抛出。
        /// </exception>
        public int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException();

            lock (sync)
            {
                // 如果没有缓冲数据
                if (availableBytes == 0)
                {
                    if (ReadFully)
                    {
                        Array.Clear(buffer, offset, count);
                        return count;
                    }
                    return 0;
                }

                int toRead = Math.Min(count, availableBytes);

                int firstCopy = Math.Min(toRead, BufferLength - readPosition);
                Array.Copy(_buffer, readPosition, buffer, offset, firstCopy);
                readPosition = (readPosition + firstCopy) % BufferLength;

                int remaining = toRead - firstCopy;
                if (remaining > 0)
                {
                    Array.Copy(_buffer, readPosition, buffer, offset + firstCopy, remaining);
                    readPosition = (readPosition + remaining) % BufferLength;
                }

                availableBytes -= toRead;

                // 若请求多于实际读取，按 ReadFully 决定是否填充静音
                if (toRead < count)
                {
                    if (ReadFully)
                    {
                        Array.Clear(buffer, offset + toRead, count - toRead);
                        return count;
                    }
                }

                return toRead;
            }
        }

        /// <summary>
        /// 清理缓冲区，重置读写指针和可用字节计数。
        /// </summary>
        public void ClearBuffer()
        {
            lock (sync)
            {
                writePosition = 0;
                readPosition = 0;
                availableBytes = 0;
            }
        }
    }
}