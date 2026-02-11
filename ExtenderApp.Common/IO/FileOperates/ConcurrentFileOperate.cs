using System.Collections.Concurrent;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.IO.FileOperates
{
    /// <summary>
    /// 基于文件偏移的并发文件操作实现。
    /// - 使用区间列表 <see cref="_wrPositions"/> 跟踪当前正在进行的读/写（或其他需要互斥的）操作区间；
    /// - 当新请求与已有区间重叠时，进入等待，直到有线程释放重叠区间；
    /// - 通过 <see cref="_wakeup"/> 以“单次唤醒”的方式通知等待者重新竞争，避免惊群。
    /// </summary>
    internal class ConcurrentFileOperate : FileOperate
    {
        /// <summary>
        /// 当前已登记的正在操作的区间集合，元素为 (Start, end)。
        /// </summary>
        private readonly List<(long, long)> _wrPositions;

        /// <summary>
        /// 保护 <see cref="_wrPositions"/> 的互斥锁。
        /// </summary>
        private readonly object _wrLock;

        /// <summary>
        /// 等待/唤醒原语：当有区间释放时唤醒一个等待者以重试登记。
        /// </summary>
        private readonly SemaphoreSlim _wakeup;

        public ConcurrentFileOperate(LocalFileInfo info) : base(info)
        {
            _wrPositions = new();
            _wrLock = new();
            _wakeup = new(0, int.MaxValue);
        }

        public ConcurrentFileOperate(FileOperateInfo operateInfo) : base(operateInfo)
        {
            _wrPositions = new();
            _wrLock = new();
            _wakeup = new(0, int.MaxValue);
        }

        protected override void ChangeCapacity(long length)
        {
            Stream.SetLength(length);
        }

        #region Read

        protected override byte[] ExecuteRead(long filePosition, int length)
        {
            EnterOperate(filePosition, length);
            try
            {
                byte[] bytes = new byte[length];
                RandomAccess.Read(Stream.SafeFileHandle, bytes, filePosition);
                return bytes;
            }
            finally
            {
                ExitOperate(filePosition, length);
            }
        }

        protected override int ExecuteRead(long filePosition, Span<byte> span)
        {
            EnterOperate(filePosition, span.Length);
            try
            {
                return RandomAccess.Read(Stream.SafeFileHandle, span, filePosition);
            }
            finally
            {
                ExitOperate(filePosition, span.Length);
            }
        }

        protected override async ValueTask<byte[]> ExecuteReadAsync(long filePosition, int length, CancellationToken token)
        {
            await EnterOperateAsync(filePosition, length);
            try
            {
                byte[] bytes = new byte[length];
                await RandomAccess.ReadAsync(Stream.SafeFileHandle, bytes, filePosition, token);
                return bytes;
            }
            finally
            {
                ExitOperate(filePosition, length);
            }
        }

        protected override async ValueTask<long> ExecuteReadAsync(long filePosition, long length, AbstractBuffer<byte> buffer, CancellationToken token)
        {
            await EnterOperateAsync(filePosition, buffer.Committed);
            try
            {
                var sequence = buffer.CommittedSequence;
                SequencePosition position = sequence.Start;
                while (sequence.TryGet(ref position, out ReadOnlyMemory<byte> memory))
                {
                    await RandomAccess.WriteAsync(Stream.SafeFileHandle, memory, filePosition);
                    filePosition += memory.Length;
                }
                return buffer.Committed;
            }
            finally
            {
                ExitOperate(filePosition, buffer.Committed);
            }
        }

        #endregion Read

        #region Write

        protected override void ExecuteWrite(long filePosition, ReadOnlySpan<byte> span)
        {
            EnterOperate(filePosition, span.Length);
            try
            {
                RandomAccess.Write(Stream.SafeFileHandle, span, filePosition);
            }
            finally
            {
                ExitOperate(filePosition, span.Length);
            }
        }

        protected override long ExecuteWrite(long filePosition, AbstractBuffer<byte> buffer)
        {
            EnterOperate(filePosition, buffer.Committed);
            try
            {
                var sequence = buffer.CommittedSequence;
                SequencePosition position = sequence.Start;
                while (sequence.TryGet(ref position, out ReadOnlyMemory<byte> memory))
                {
                    RandomAccess.Write(Stream.SafeFileHandle, memory.Span, filePosition);
                    filePosition += memory.Length;
                }
                return buffer.Committed;
            }
            finally
            {
                ExitOperate(filePosition, buffer.Committed);
            }
        }

        protected override async ValueTask<long> ExecuteWriteAsync(long filePosition, AbstractBuffer<byte> buffer, CancellationToken token)
        {
            await EnterOperateAsync(filePosition, buffer.Committed);
            try
            {
                var sequence = buffer.CommittedSequence;
                SequencePosition position = sequence.Start;
                while (sequence.TryGet(ref position, out ReadOnlyMemory<byte> memory))
                {
                    await RandomAccess.WriteAsync(Stream.SafeFileHandle, memory, filePosition);
                    filePosition += memory.Length;
                }
                return buffer.Committed;
            }
            finally
            {
                ExitOperate(filePosition, buffer.Committed);
            }
        }

        #endregion Write

        /// <summary>
        /// 检查给定区间 [position, position + length) 是否可以立即操作（与已登记区间无重叠）。
        /// </summary>
        /// <param name="position">操作起始偏移。</param>
        /// <param name="length">操作长度。</param>
        /// <returns>可操作返回 true；否则返回 false。</returns>
        /// <remarks>内部加锁，保证在检查期间不会被并发修改。</remarks>
        private bool CanOperate(long position, long length)
        {
            lock (_wrLock)
            {
                for (int i = 0; i < _wrPositions.Count; i++)
                {
                    var (start, len) = _wrPositions[i];
                    if (Overlaps(position, length, start, len))
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 同步进入指定区间的“操作临界区”： 若当前与已登记区间重叠则阻塞等待，直到成功登记该区间后返回。
        /// </summary>
        /// <param name="position">操作起始偏移。</param>
        /// <param name="length">操作长度（小于等于 0 时直接返回）。</param>
        /// <remarks>
        /// - 使用循环“检查-等待-重试”模式处理可能的竞争与虚假唤醒；
        /// - 唤醒顺序不保证公平性；
        /// - 调用者必须在 finally 中调用 <see cref="ExitOperate(long, long)"/> 释放区间。
        /// </remarks>
        private void EnterOperate(long position, long length)
        {
            if (length <= 0) return;

            while (true)
            {
                if (TryAddRange(position, length))
                    return;

                // 等待被唤醒后重试
                _wakeup.Wait();
            }
        }

        /// <summary>
        /// 异步进入指定区间的“操作临界区”： 若当前与已登记区间重叠则异步等待，直到成功登记该区间后返回。
        /// </summary>
        /// <param name="position">操作起始偏移。</param>
        /// <param name="length">操作长度（小于等于 0 时直接返回）。</param>
        /// <param name="token">取消令牌，取消将抛出异常并结束等待。</param>
        /// <remarks>
        /// - 使用 <see cref="SemaphoreSlim.WaitAsync(CancellationToken)"/> 进行轻量阻塞；
        /// - 采用循环“检查-等待-重试”以应对竞争；
        /// - 调用者必须在 finally 中调用 <see cref="ExitOperate(long, long)"/> 释放区间。
        /// </remarks>
        private async ValueTask EnterOperateAsync(long position, long length, CancellationToken token = default)
        {
            if (length <= 0) return;

            while (true)
            {
                if (TryAddRange(position, length))
                    return;

                await _wakeup.WaitAsync(token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 退出并释放先前登记的区间，随后唤醒一个等待者进行重试。
        /// </summary>
        /// <param name="position">先前登记的起始偏移。</param>
        /// <param name="length">先前登记的长度。</param>
        /// <remarks>
        /// - 若存在多个等待者，每次仅唤醒一个，减少惊群；
        /// - 若 Exit 调用与 Enter 不成对，可能导致区间泄漏或过度唤醒。
        /// </remarks>
        private void ExitOperate(long position, long length)
        {
            if (length <= 0) return;

            lock (_wrLock)
            {
                for (int i = 0; i < _wrPositions.Count; i++)
                {
                    var (s, l) = _wrPositions[i];
                    if (s == position && l == length)
                    {
                        _wrPositions.RemoveAt(i);
                        break;
                    }
                }
            }

            _wakeup.Release();
        }

        /// <summary>
        /// 在不与已登记区间重叠时登记一个新区间。
        /// </summary>
        /// <param name="position">起始偏移。</param>
        /// <param name="length">长度。</param>
        /// <returns>登记成功返回 true；若与现有区间重叠返回 false。</returns>
        /// <remarks>调用方需在成功后保证对应的 <see cref="ExitOperate(long, long)"/> 被调用。 注意：内部调用 <see cref="CanOperate(long, long)"/>，该方法也会加锁（可重入），可视需要内联以减少重入成本。</remarks>
        private bool TryAddRange(long position, long length)
        {
            lock (_wrLock)
            {
                if (!CanOperate(position, length))
                    return false;

                _wrPositions.Add((position, length));
                return true;
            }
        }

        /// <summary>
        /// 判断两个半开区间 (aStart, aStart + aLen) 与 (bStart, bStart + bLen) 是否重叠。
        /// </summary>
        private static bool Overlaps(long aStart, long aLen, long bStart, long bLen)
            => aStart < bStart + bLen && bStart < aStart + aLen;
    }
}