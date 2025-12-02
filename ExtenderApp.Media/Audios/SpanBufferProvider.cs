using NAudio.Wave;

namespace ExtenderApp.Media.Audios
{
    /// <summary>
    /// 基于固定大小字节数组的环形缓冲区实现，作为 NAudio 的 <see cref="IWaveProvider"/> 数据源。
    /// 核心特性：
    /// - 写入端：支持 <see cref="AddSamples"/> 系列方法追加音频数据（支持 Span/byte[] 输入）
    /// - 读取端：通过 <see cref="Read"/> 供 NAudio 拉取数据播放
    /// - 溢出策略：固定大小，写入超容量时覆盖最旧数据
    /// - 线程安全：读写操作通过独占锁保证并发安全
    /// - 帧对齐：自动确保缓冲长度为音频帧大小的整数倍，避免播放失真
    /// </summary>
    internal class SpanBufferProvider : IWaveProvider
    {
        #region 私有字段

        /// <summary>
        /// 环形缓冲底层存储
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        /// 缓冲总时长（预计算，避免重复计算）
        /// </summary>
        private readonly TimeSpan _bufferDuration;

        /// <summary>
        /// 写指针（下一次写入的起始索引）
        /// </summary>
        private int _writePosition;

        /// <summary>
        /// 读指针（下一次读取的起始索引）
        /// </summary>
        private int _readPosition;

        /// <summary>
        /// 当前可用读取字节数（已写入未读取）
        /// </summary>
        private int _availableBytes;

        /// <summary>
        /// 音频帧大小（bitsPerSample/8 * channels），用于确保数据对齐
        /// </summary>
        private readonly int _frameSize;

        #endregion 私有字段

        #region 公共属性

        /// <summary>
        /// 音频数据格式（采样率/位深/通道数）
        /// </summary>
        public WaveFormat WaveFormat { get; }

        /// <summary>
        /// 缓冲总长度（字节，已确保为帧大小的整数倍）
        /// </summary>
        public int BufferLength { get; }

        /// <summary>
        /// 是否填充不足数据（不足时补静音），默认与 NAudio 行为一致（true）
        /// </summary>
        public bool ReadFully { get; set; } = true;

        /// <summary>
        /// 缓冲总时长（只读）
        /// </summary>
        public TimeSpan BufferDuration => _bufferDuration;

        /// <summary>
        /// 当前可用读取字节数（线程安全）
        /// </summary>
        public int AvailableBytes
        {
            get
            {
                return _availableBytes;
            }
        }

        /// <summary>
        /// 缓冲填充比例（0.0 ~ 1.0，线程安全）
        /// </summary>
        public double FillPercentage
        {
            get
            {
                return (double)_availableBytes / BufferLength;
            }
        }

        #endregion 公共属性

        #region 构造函数（重载增强灵活性）

        /// <summary>
        /// 使用默认5秒缓冲时长构造
        /// </summary>
        /// <param name="waveFormat">音频格式描述</param>
        /// <exception cref="ArgumentNullException">waveFormat 为 null 时抛出</exception>
        /// <exception cref="InvalidOperationException">音频格式无效（帧大小计算异常）</exception>
        public SpanBufferProvider(WaveFormat waveFormat)
            : this(waveFormat, TimeSpan.FromSeconds(5))
        {
        }

        /// <summary>
        /// 指定缓冲时长构造（自动计算对齐后的缓冲长度）
        /// </summary>
        /// <param name="waveFormat">音频格式描述</param>
        /// <param name="bufferDuration">期望的缓冲总时长（实际时长可能因帧对齐略有偏差）</param>
        /// <exception cref="ArgumentNullException">waveFormat 为 null 时抛出</exception>
        /// <exception cref="ArgumentOutOfRangeException">bufferDuration ≤ 0 时抛出</exception>
        /// <exception cref="InvalidOperationException">音频格式无效（帧大小计算异常）</exception>
        public SpanBufferProvider(WaveFormat waveFormat, TimeSpan bufferDuration)
        {
            WaveFormat = waveFormat ?? throw new ArgumentNullException(nameof(waveFormat));
            if (bufferDuration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(bufferDuration), "缓冲时长必须大于0");

            // 计算帧大小（确保数据按帧对齐，避免播放噪音）
            _frameSize = waveFormat.BitsPerSample / 8 * waveFormat.Channels;
            if (_frameSize <= 0)
                throw new InvalidOperationException("无效的音频格式：帧大小计算结果小于等于0");

            // 计算缓冲长度（向上取整到帧大小的整数倍）
            double requiredBytes = waveFormat.AverageBytesPerSecond * bufferDuration.TotalSeconds;
            int rawLength = (int)Math.Ceiling(requiredBytes);
            BufferLength = AlignToFrameSize(rawLength);

            // 初始化核心字段
            _buffer = new byte[BufferLength];
            _bufferDuration = TimeSpan.FromSeconds((double)BufferLength / waveFormat.AverageBytesPerSecond);
            _writePosition = 0;
            _readPosition = 0;
            _availableBytes = 0;
        }

        /// <summary>
        /// 直接指定缓冲字节长度构造（自动对齐帧大小）
        /// </summary>
        /// <param name="waveFormat">音频格式描述</param>
        /// <param name="bufferLengthInBytes">期望的缓冲长度（字节）</param>
        /// <exception cref="ArgumentNullException">waveFormat 为 null 时抛出</exception>
        /// <exception cref="ArgumentOutOfRangeException">bufferLengthInBytes ≤ 0 时抛出</exception>
        /// <exception cref="InvalidOperationException">音频格式无效（帧大小计算异常）</exception>
        public SpanBufferProvider(WaveFormat waveFormat, int bufferLengthInBytes)
        {
            WaveFormat = waveFormat ?? throw new ArgumentNullException(nameof(waveFormat));
            if (bufferLengthInBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferLengthInBytes), "缓冲长度必须大于0");

            // 计算帧大小并校验
            _frameSize = waveFormat.BitsPerSample / 8 * waveFormat.Channels;
            if (_frameSize <= 0)
                throw new InvalidOperationException("无效的音频格式：帧大小计算结果小于等于0");

            // 对齐到帧大小
            BufferLength = AlignToFrameSize(bufferLengthInBytes);

            // 初始化核心字段
            _buffer = new byte[BufferLength];
            _bufferDuration = TimeSpan.FromSeconds((double)BufferLength / waveFormat.AverageBytesPerSecond);
            _writePosition = 0;
            _readPosition = 0;
            _availableBytes = 0;
        }

        #endregion 构造函数（重载增强灵活性）

        #region 公共方法

        /// <summary>
        /// 追加音频数据到环形缓冲（支持 Span 输入，高性能）
        /// </summary>
        /// <param name="samples">待写入的音频字节数据（空数据直接忽略）</param>
        public void AddSamples(ReadOnlySpan<byte> samples)
        {
            if (samples.IsEmpty)
                return;

            int freeSpace = BufferLength - _availableBytes;
            int samplesLength = samples.Length;

            // 溢出处理：数据超过剩余空间时，覆盖最旧数据
            if (samplesLength > freeSpace)
            {
                int bytesToDiscard = samplesLength - freeSpace;
                // 移动读指针，丢弃旧数据（取模确保环形特性）
                _readPosition = (_readPosition + bytesToDiscard) % BufferLength;
                _availableBytes -= bytesToDiscard;
            }

            // 分两段写入（处理环形边界）
            int writeToEndLength = Math.Min(samplesLength, BufferLength - _writePosition);
            // 第一段：从写指针到缓冲末尾
            samples.Slice(0, writeToEndLength).CopyTo(_buffer.AsSpan(_writePosition, writeToEndLength));

            int remainingLength = samplesLength - writeToEndLength;
            if (remainingLength > 0)
            {
                // 第二段：从缓冲开头到剩余长度
                samples.Slice(writeToEndLength, remainingLength).CopyTo(_buffer.AsSpan(0, remainingLength));
            }

            // 更新写指针（取模简化逻辑，避免边界判断）
            _writePosition = (_writePosition + samplesLength) % BufferLength;
            // 更新可用字节数
            _availableBytes += samplesLength;
        }

        /// <summary>
        /// 追加音频数据到环形缓冲（支持 byte[] 输入，兼容旧代码）
        /// </summary>
        /// <param name="samples">待写入的音频字节数组</param>
        /// <param name="offset">数组起始偏移量</param>
        /// <param name="count">待写入的字节数</param>
        /// <exception cref="ArgumentNullException">samples 为 null 时抛出</exception>
        /// <exception cref="ArgumentOutOfRangeException">偏移量或长度无效时抛出</exception>
        public void AddSamples(byte[] samples, int offset, int count)
        {
            if (samples == null)
                throw new ArgumentNullException(nameof(samples));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "偏移量不能为负数");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "写入长度不能为负数");
            if (offset + count > samples.Length)
                throw new ArgumentOutOfRangeException($"偏移量({offset}) + 写入长度({count}) 超过源数组长度({samples.Length})");

            AddSamples(samples.AsSpan(offset, count));
        }

        /// <summary>
        /// 从缓冲读取数据（NAudio 播放器调用入口）
        /// </summary>
        /// <param name="buffer">目标存储数组</param>
        /// <param name="offset">目标数组起始偏移量</param>
        /// <param name="count">请求读取的字节数</param>
        /// <returns>实际读取的字节数（ReadFully=true 时返回 count，否则返回可用字节数）</returns>
        /// <exception cref="ArgumentNullException">buffer 为 null 时抛出</exception>
        /// <exception cref="ArgumentOutOfRangeException">偏移量或长度无效时抛出</exception>
        public int Read(byte[] buffer, int offset, int count)
        {
            // 输入参数严格校验
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "偏移量不能为负数");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "读取长度不能为负数");
            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException($"偏移量({offset}) + 读取长度({count}) 超过目标数组长度({buffer.Length})");

            int bytesRead = 0;

            // 最多读取可用字节数
            int actualReadLength = Math.Min(count, _availableBytes);
            if (actualReadLength <= 0)
                goto FillSilenceIfNeeded; // 无数据可读，直接跳转到静音填充

            // 分两段读取（处理环形边界）
            int readToEndLength = Math.Min(actualReadLength, BufferLength - _readPosition);
            // 第一段：从读指针到缓冲末尾
            _buffer.AsSpan(_readPosition, readToEndLength).CopyTo(buffer.AsSpan(offset, readToEndLength));

            int remainingReadLength = actualReadLength - readToEndLength;
            if (remainingReadLength > 0)
            {
                // 第二段：从缓冲开头到剩余长度
                _buffer.AsSpan(0, remainingReadLength).CopyTo(buffer.AsSpan(offset + readToEndLength, remainingReadLength));
            }

            // 更新读指针和可用字节数
            _readPosition = (_readPosition + actualReadLength) % BufferLength;
            _availableBytes -= actualReadLength;
            bytesRead = actualReadLength;

        FillSilenceIfNeeded:
            // 不足时填充静音（PCM 静音为 0 字节）
            if (ReadFully && bytesRead < count)
            {
                int silenceLength = count - bytesRead;
                buffer.AsSpan(offset + bytesRead, silenceLength).Clear();
                return count;
            }

            return bytesRead;
        }

        /// <summary>
        /// 清空缓冲（重置读写指针和可用字节数，不清理底层数组以提升性能）
        /// </summary>
        public void ClearBuffer()
        {
            _writePosition = 0;
            _readPosition = 0;
            _availableBytes = 0;
        }

        #endregion 公共方法

        #region 私有辅助方法

        /// <summary>
        /// 将长度对齐到帧大小的整数倍（避免数据错位导致的播放失真）
        /// </summary>
        /// <param name="rawLength">原始长度（字节）</param>
        /// <returns>对齐后的长度</returns>
        private int AlignToFrameSize(int rawLength)
        {
            if (rawLength % _frameSize == 0)
                return rawLength;

            // 向上取整到最近的帧大小倍数
            return ((rawLength + _frameSize - 1) / _frameSize) * _frameSize;
        }

        #endregion 私有辅助方法
    }
}